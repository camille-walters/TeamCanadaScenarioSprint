using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    // Public fields
    public GameObject car;
    public GameObject simulationViews;
    public GameObject paintingRoomDoors;
    public GameObject centralConveyor;
    public GameObject cvCapturePositions;
    public bool paintingInProgress;
    public bool cvDone;
    public int currentView;
    
    Vector3 m_DoorPosition; // For painting room door close
    List<Camera> m_Views = new(); // Camera views
    List<Camera> m_CVCaptureCameras = new(); // CV Cameras: Left, Top and Right
    
    GameObject m_CarsGameObject; // Cars GameObject spawned in scene
    List<Car> m_Cars = new(); // List of all cars in the scene
    List<Room> m_CarCurrentRooms = new(); // List of the rooms each corresponding car is in
    Dictionary<Room, List<int>> m_CarTracker = new Dictionary<Room, List<int>> // List indices of cars in each room
    {
        {Room.SpawnRoom, new List<int>()},
        {Room.PaintingRoom, new List<int>()},
        {Room.BufferRoom, new List<int>()},
        {Room.CVRoom, new List<int>()},
        {Room.QARoom, new List<int>()}
    };
    
    // CV capture resolution (16:9 aspect)
    const int k_ResWidth = 782;
    const int k_ResHeight = 440;

    void Start()
    {
        m_DoorPosition = paintingRoomDoors.transform.localPosition;

        if (simulationViews != null)
        {
            m_Views.Add(simulationViews.transform.GetChild(0).gameObject.GetComponent<Camera>());
            m_Views.Add(simulationViews.transform.GetChild(1).gameObject.GetComponent<Camera>());
            m_Views.Add(simulationViews.transform.GetChild(2).gameObject.GetComponent<Camera>());
            m_Views.Add(simulationViews.transform.GetChild(3).gameObject.GetComponent<Camera>());

            // Enabling first camera view only
            currentView = 0;
            m_Views[0].enabled = true;
            m_Views[1].enabled = false;
            m_Views[2].enabled = false;
            m_Views[3].enabled = false;
        }

        if (cvCapturePositions != null)
        {
            m_CVCaptureCameras.Add(cvCapturePositions.transform.GetChild(0).gameObject.GetComponent<Camera>());
            m_CVCaptureCameras.Add(cvCapturePositions.transform.GetChild(1).gameObject.GetComponent<Camera>());
            m_CVCaptureCameras.Add(cvCapturePositions.transform.GetChild(2).gameObject.GetComponent<Camera>());
        }

        m_CarsGameObject = new GameObject("Cars");
    }

    void Update()
    {
        paintingRoomDoors.transform.localPosition = paintingInProgress ? new Vector3(m_DoorPosition.x, -10, m_DoorPosition.z) : m_DoorPosition;

        // Switch camera view when C key is pressed
        // TODO: Connect with UI
        if (Input.GetKeyDown(KeyCode.C) && simulationViews != null)
        {
            m_Views[currentView].enabled = false;
            
            if (currentView < 3)
                currentView += 1;
            else
                currentView = 0;
            
            m_Views[currentView].enabled = true;
        }
        
        ManageCarSpawn();
        UpdateCarRooms();
    }

    void ManageCarSpawn()
    {
        // Zero cars in scene
        if (m_CarsGameObject.transform.childCount == 0)
        {
            SpawnCar();
            return;
        }

        var lastIndex = m_Cars.FindIndex(c => c == m_Cars.Last());
        if (m_CarCurrentRooms[lastIndex] == Room.SpawnRoom && m_Cars[lastIndex].currentRoom == Room.PaintingRoom)
        {
            SpawnCar();
        }
    }

    void SpawnCar()
    {
        var newCar = Instantiate(car, new Vector3(0, 0.6f, 90), Quaternion.Euler(0, 180, 0));
        newCar.transform.parent = m_CarsGameObject.transform;
        var carComponent = newCar.AddComponent<Car>();
        
        m_Cars.Add(carComponent);
        m_CarCurrentRooms.Add(Room.SpawnRoom);
        m_CarTracker[Room.SpawnRoom].Add(m_Cars.FindIndex(c => c == carComponent));
    }

    void UpdateCarRooms()
    {
        for (var i = 0; i < m_Cars.Count; ++i)
        {
            if (m_CarCurrentRooms[i] != m_Cars[i].currentRoom)
            {
                m_CarTracker[m_CarCurrentRooms[i]].Remove(i);
                m_CarTracker[m_Cars[i].currentRoom].Add(i);
                m_CarCurrentRooms[i] = m_Cars[i].currentRoom;

                if (m_CarCurrentRooms[i] == Room.CVRoom)
                    ManageAnalysisRoomOccupancy(i);
            }
        }
    }

    void ManageAnalysisRoomOccupancy(int carIndex)
    {
        // Only one car allowed in the CV room at one time
        
        // Stop the car
        var cvRoomConveyor = centralConveyor.transform.GetChild(3).GetComponent<ConveyorController>();
        cvRoomConveyor.stopTime = 2;
        cvRoomConveyor.stopForTime = true;

        // Capture images 
        StartCoroutine(CaptureFromAllPositions(carIndex));
    }

    IEnumerator CaptureFromAllPositions(int carIndex)
    {
        var cameraViewCounter = 0;
        foreach (var cvCamera in m_CVCaptureCameras)
        {
            yield return new WaitForSeconds(0.1f);
            CaptureImageFromOnePosition(cvCamera, carIndex, cameraViewCounter);
            cameraViewCounter += 1;
        }
        Debug.Log($"{cameraViewCounter} screenshots were taken for car {carIndex}!");
    }

    static void CaptureImageFromOnePosition(Camera currentCamera, int carIndex, int viewIndex)
    {
        var rt = new RenderTexture(k_ResWidth, k_ResHeight, 24);
        currentCamera.targetTexture = rt;
        var screenShot = new Texture2D(k_ResWidth, k_ResHeight, TextureFormat.RGB24, false);
        currentCamera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, k_ResWidth, k_ResHeight), 0, 0);
        currentCamera.targetTexture = null;
        RenderTexture.active = null; 
        Destroy(rt);
        
        var bytes = screenShot.EncodeToPNG();
        var filename = $"{Application.dataPath}/CVCaptures/flawed{carIndex}_{viewIndex}.png";
        System.IO.File.WriteAllBytes(filename, bytes);
    }
}
