using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class SimulationManager : MonoBehaviour
{
    // Public fields
    public GameObject car;
    public GameObject simulationViews;
    public GameObject centralConveyor;
    public GameObject cvCapturePositions;
    public Material panelMaterial;
    public GameObject operatorSpawnPoint;
    public GameObject operatorPrefab;
    public float conveyorSpeedFactor = 1;
    public float interCarDistance = 92 - 61;
    public int currentView;
    public int numberOfOperators = 2;
    public float fixingTimeForMinorDefects;
    public float fixingTimeForMajorDefects;
    public int totalCarsProcessed;
    public float carsProcessedPerMinute;
    public float carProcessingTime;
    public float totalOperatorBusyTime = 0;
    public float totalOperatorUtilization = 0;
    public float totalMajorDefects = 0f;
    public float totalMinorDefects = 0f;
    
    SimulationTimeTracker m_SimulationTimeTracker;
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
    int m_CarStartIndex = 0;
    
    // CV capture resolution (16:9 aspect)
    const int k_ResWidth = 782;
    const int k_ResHeight = 440;

    Texture2D m_Texture2D;
    
    List<Car> m_FixingVirtualBuffer = new();
    List<bool> m_OccupiedPositionsInTheBuffer = new();
    List<bool> m_OperatorOccupied = new();
    int m_BufferSize = 10;

    float lastCarProcessTime = 0;
    void Start()
    {
        // m_DoorPosition = paintingRoomDoors.transform.localPosition;
        m_SimulationTimeTracker = this.gameObject.GetComponent<SimulationTimeTracker>();

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
                m_Views[i].gameObject.SetActive(false);
            }

            // Enabling first camera view only
            currentView = 0;
            m_Views[0].enabled = true;
            m_Views[0].gameObject.SetActive(true);
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
        m_OperatorOccupied = Enumerable.Repeat(false, numberOfOperators).ToList();
        m_OccupiedPositionsInTheBuffer = Enumerable.Repeat(false, m_BufferSize).ToList();
    }

    void Update()
    {
        totalOperatorUtilization = totalOperatorBusyTime / (Time.realtimeSinceStartup * numberOfOperators);
        
        // Update Conveyor speeds
        if (Math.Abs(conveyorSpeedFactor - m_PrevConveyorSpeedFactor) > 0.001f) 
            UpdateConveyorSpeeds();
        m_PrevConveyorSpeedFactor = conveyorSpeedFactor;

        // Switch camera view when C key is pressed
        // TODO: Connect with UI
        if (Input.GetKeyDown(KeyCode.C) && simulationViews != null)
        {
            m_Views[currentView].enabled = false;
            m_Views[currentView].gameObject.SetActive(false);
            
            if (currentView < simulationViews.transform.childCount - 1)
                currentView += 1;
            else
                currentView = 0;
            
            m_Views[currentView].enabled = true;
            m_Views[currentView].gameObject.SetActive(true);
        }
        ManageCarSpawn();
        UpdateCarRooms();

        foreach (var op in m_OperatorOccupied)
        {
            if (op)
                totalOperatorBusyTime += Time.deltaTime;
        }
    }

    void SpawnOperators()
    {
        for (var i = 0; i < numberOfOperators; ++i)
        {
            var newOp = Instantiate(operatorPrefab, new Vector3(6, 0, -42+i*2), Quaternion.Euler(0, -90, 0));
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
        var newCar = Instantiate(car, new Vector3(0, 0.6f, 61 + interCarDistance), Quaternion.Euler(0, 180, 0));
        newCar.transform.parent = m_CarsGameObject.transform;
        var carComponent = newCar.AddComponent<Car>();
        
        m_Cars.Add(carComponent);
        m_CarCurrentRooms.Add(Room.SpawnRoom);
        m_CarTracker[Room.SpawnRoom].Add(m_Cars.FindIndex(c => c == carComponent));
    }

    void UpdateCarRooms()
    {
        for (var i = m_CarStartIndex; i < m_Cars.Count; ++i)
        {
            if (m_CarCurrentRooms[i] != m_Cars[i].currentRoom)
            {
                m_CarTracker[m_CarCurrentRooms[i]].Remove(i);
                m_CarTracker[m_Cars[i].currentRoom].Add(i);
                m_CarCurrentRooms[i] = m_Cars[i].currentRoom;

                if (m_CarCurrentRooms[i] == Room.CVRoom)
                    ManageAnalysisRoomOccupancy(i);

                if (m_CarCurrentRooms[i] == Room.QARoom)
                {
                    totalMajorDefects += m_Cars[i].majorFlaws;
                    totalMinorDefects += m_Cars[i].minorFlaws;
                    DisplayDefectsOnPanel(i);
                    StartCoroutine(AcquireOperatorToFixCar(i));
                }

                if (m_CarCurrentRooms[i] == Room.ProcessedRoom)
                {
                    Debug.Log($"Processed Car {m_Cars[i].carID}. Destroying it now.");
                    totalCarsProcessed += 1;
                    
                    DespawnProcessedCar(i);
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
        carRigidBody.position = new Vector3(0, 0.59f, -22f);
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
        File.WriteAllBytes(filename, bytes);
    }

    void DisplayDefectsOnPanel(int carNumber)
    {
        var fileData = new byte[] { };
        var path = $"{Application.dataPath}/CVCaptures/Contours/contours{carNumber}_0.png";
        if (File.Exists(path))
        {
            fileData = File.ReadAllBytes(path);
        }
        m_Texture2D.LoadImage(fileData);
        panelMaterial.mainTexture = m_Texture2D;
        
    }

    void DespawnProcessedCar(int index)
    {
        // Process car
        var carTemp = m_Cars[index];
        m_CarStartIndex = index + 1;
        m_Cars[index] = null;
        Destroy(carTemp.gameObject);
        
        // Update car process time
        carProcessingTime = Time.realtimeSinceStartup - lastCarProcessTime;
        lastCarProcessTime = Time.realtimeSinceStartup;
    }
    
    IEnumerator AcquireOperatorToFixCar(int index)
    {
        /*
         * Works under the assumption that each car is fixed by only one operator at a time as of now
         * So if the buffer is under utilized, only one operator will be occupied most of the time
         * If the buffer is over utilized, some cars may need to wait longer for an operator 
         */
        
        var carToFix = m_Cars[index];
        
        // Move Car object to the side (consider turning it off its too much?)
        m_FixingVirtualBuffer.Add(carToFix);
        var carPositionZ = m_OccupiedPositionsInTheBuffer.IndexOf(false);
        m_OccupiedPositionsInTheBuffer[carPositionZ] = true;
        var selectedCar = carToFix.gameObject;
        selectedCar.transform.position = new Vector3(4, 0.6f, -30 - carPositionZ * 8);
        
        var timeToFix = m_Cars[index].minorFlaws * fixingTimeForMinorDefects + m_Cars[index].majorFlaws * fixingTimeForMajorDefects;
        // var timeToFix = 3 * fixingTimeForMinorDefects + 4 * fixingTimeForMajorDefects; // temp override
        carToFix.timeTakenToFix = timeToFix;
        Debug.Log($"Car {carToFix.carID} will take {timeToFix} seconds to be fixed. Trying to acquire an operator...");
        
        if (m_OperatorOccupied.Contains(false))
        {
            StartCoroutine(FixCar(carToFix, timeToFix, carPositionZ));
        }
        else
        {
            // Keep looking for a free operator
            Debug.Log("All operators are occupied, waiting for a free operator");
            var recheckTime = 1;
            
            while (true)
            {
                yield return new WaitForSeconds(recheckTime);
                carToFix.timeTakenToFix += recheckTime;
                
                if (m_OperatorOccupied.Contains(false))
                {
                    StartCoroutine(FixCar(carToFix, timeToFix, carPositionZ));
                    break;
                }
            }
        }
    }

    IEnumerator FixCar(Car carToFix, float timeToFix, int bufferPosition)
    {
        // Assign operator and move it 
        Debug.Log($"Found an unoccupied operator for {carToFix.carID}!");
        var unoccupiedOperatorIndex = m_OperatorOccupied.IndexOf(false);
        m_OperatorOccupied[unoccupiedOperatorIndex] = true;
        // totalOperatorBusyTime += timeToFix;
        var selectedOperator = operatorSpawnPoint.transform.GetChild(unoccupiedOperatorIndex);
        selectedOperator.position = new Vector3(6, 0, -30 - bufferPosition * 8);

        yield return new WaitForSeconds(timeToFix);

        // Move Car object back onto the conveyor 
        var selectedCar = carToFix.gameObject;
        selectedCar.transform.position = new Vector3(0, 0.6f, -35);
        m_FixingVirtualBuffer.Remove(carToFix);
        
        selectedOperator.position = new Vector3(8, 0, -30 - unoccupiedOperatorIndex * 2);
        m_OperatorOccupied[unoccupiedOperatorIndex] = false;
        m_OccupiedPositionsInTheBuffer[bufferPosition] = false;
    }
    

    public Car GetCar(int index)
    {
        return m_Cars[index];
    }
    
    public void UpdateThroughputAfterTimeChange()
    {
        carsProcessedPerMinute = (float)totalCarsProcessed / m_SimulationTimeTracker.minutesPassed;
    }
}
