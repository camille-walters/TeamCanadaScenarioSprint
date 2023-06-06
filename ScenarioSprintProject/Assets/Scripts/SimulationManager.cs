using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    // Public fields
    public GameObject car;
    public GameObject simulationViews;
    // public GameObject paintingRoomDoors;
    public GameObject centralConveyor;
    public GameObject cvCapturePositions;
    public Material panelMaterial;
    public GameObject operatorSpawnPoint;
    public GameObject operatorPrefab;
    public float conveyorSpeedFactor = 1;
    public int currentView;
    public int numberOfOperators = 2;
    public int totalCarsProcessed;

    List<ConveyorController> m_ConveyorControllers = new();
    float m_PrevConveyorSpeedFactor;
    
    // Vector3 m_DoorPosition; // For painting room door close
    List<Camera> m_Views = new(); // Camera views
    int m_TotalViews;
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
        {Room.QARoom, new List<int>()},
        {Room.ProcessedRoom, new List<int>()}
    };
    
    // CV capture resolution (16:9 aspect)
    const int k_ResWidth = 782;
    const int k_ResHeight = 440;

    Texture2D m_Texture2D;
    void Start()
    {
        // m_DoorPosition = paintingRoomDoors.transform.localPosition;

        for (var i = 0; i < centralConveyor.transform.childCount; ++i)
        {
            m_ConveyorControllers.Add(centralConveyor.transform.GetChild(i).GetComponent<ConveyorController>());
        }

        if (simulationViews != null)
        {
            m_TotalViews = simulationViews.transform.childCount;
            for (var i = 0; i < m_TotalViews; ++i)
            {
                m_Views.Add(simulationViews.transform.GetChild(i).gameObject.GetComponent<Camera>());
                m_Views[i].enabled = false;
            }

            // Enabling first camera view only
            currentView = 0;
            m_Views[0].enabled = true;
        }

        if (cvCapturePositions != null)
        {
            m_CVCaptureCameras.Add(cvCapturePositions.transform.GetChild(0).gameObject.GetComponent<Camera>());
            m_CVCaptureCameras.Add(cvCapturePositions.transform.GetChild(1).gameObject.GetComponent<Camera>());
            m_CVCaptureCameras.Add(cvCapturePositions.transform.GetChild(2).gameObject.GetComponent<Camera>());
        }

        m_CarsGameObject = new GameObject("Cars");
        m_Texture2D = new Texture2D(782, 440);
        
        // Conveyor speeds
        UpdateConveyorSpeeds();
        m_PrevConveyorSpeedFactor = conveyorSpeedFactor;
        
        // Spawn Operators
        SpawnOperators();
    }

    void Update()
    {
        // paintingRoomDoors.transform.localPosition = paintingInProgress ? new Vector3(m_DoorPosition.x, -10, m_DoorPosition.z) : m_DoorPosition;
        
        // Update Conveyor speeds
        if (Math.Abs(conveyorSpeedFactor - m_PrevConveyorSpeedFactor) > 0.001f) 
            UpdateConveyorSpeeds();
        m_PrevConveyorSpeedFactor = conveyorSpeedFactor;

        // Switch camera view when C key is pressed
        // TODO: Connect with UI
        if (Input.GetKeyDown(KeyCode.C) && simulationViews != null)
        {
            m_Views[currentView].enabled = false;
            
            if (currentView < simulationViews.transform.childCount - 1)
                currentView += 1;
            else
                currentView = 0;
            
            m_Views[currentView].enabled = true;
        }
        ManageCarSpawn();
        UpdateCarRooms();
    }

    void SpawnOperators()
    {
        for (var i = 0; i < numberOfOperators; ++i)
        {
            var newOp = Instantiate(operatorPrefab, new Vector3(-4, 0, -42+i*2), Quaternion.identity);
            newOp.transform.parent = operatorSpawnPoint.transform;
        }
    }

    void UpdateConveyorSpeeds()
    {
        foreach (var conveyorController in m_ConveyorControllers)
        {
            conveyorController.speed = conveyorController.speed * conveyorSpeedFactor;
        }
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
        var newCar = Instantiate(car, new Vector3(0, 0.6f, 92), Quaternion.Euler(0, 180, 0));
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

                if (m_CarCurrentRooms[i] == Room.QARoom)
                    DisplayDefectsOnPanel(i);

                if (m_CarCurrentRooms[i] == Room.ProcessedRoom)
                {
                    totalCarsProcessed += 1;
                }
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
        
        // Reposition the car
        var carRigidBody = m_Cars[carIndex].gameObject.GetComponent<Rigidbody>();
        carRigidBody.isKinematic = true;
        carRigidBody.position = new Vector3(0, 0.59f, -19f);
        carRigidBody.rotation = Quaternion.Euler(0, 180f, 0);

        // Capture images 
        StartCoroutine(CaptureFromAllPositions(carIndex, carRigidBody));
    }

    IEnumerator CaptureFromAllPositions(int carIndex, Rigidbody carRigidBody)
    {
        var cameraViewCounter = 0;
        foreach (var cvCamera in m_CVCaptureCameras)
        {
            yield return new WaitForSeconds(0.2f);
            CaptureImageFromOnePosition(cvCamera, carIndex, cameraViewCounter);
            cameraViewCounter += 1;
        }
        Debug.Log($"{cameraViewCounter} screenshots were taken for car {carIndex}!");
        carRigidBody.isKinematic = false;
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

    void DisplayDefectsOnPanel(int carNumber)
    {
        var fileData = File.ReadAllBytes($"{Application.dataPath}/CVCaptures/Contours/contours{carNumber}_0.png");
        m_Texture2D.LoadImage(fileData);
        panelMaterial.mainTexture = m_Texture2D;
        
    }

    public Car GetCar(int index)
    {
        return m_Cars[index];
    }
}
