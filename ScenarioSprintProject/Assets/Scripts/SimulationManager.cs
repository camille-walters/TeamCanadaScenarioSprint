using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    public GameObject car;
    public GameObject simulationViews;
    public GameObject paintingRoomDoors;
    public GameObject centralConveyor;
    public bool paintingInProgress;
    public bool cvDone;
    public int currentView;

    Vector3 m_DoorPosition;
    List<Camera> m_Views = new();
    
    GameObject m_CarsGameObject;
    List<Car> m_Cars = new();
    List<Room> m_CarCurrentRooms = new();
    Dictionary<Room, List<int>> m_CarTracker = new Dictionary<Room, List<int>>
    {
        {Room.SpawnRoom, new List<int>()},
        {Room.PaintingRoom, new List<int>()},
        {Room.BufferRoom, new List<int>()},
        {Room.CVRoom, new List<int>()},
        {Room.QARoom, new List<int>()}
    };

    // List<float> m_RoomBordersZ = new List<float> {99.73f, 83.42f, 25.6f, -12.89f, -25.39f};
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
        ManageAnalysisRoomOccupancy();
        
        // Debug.Log($"{m_CarTracker[Room.SpawnRoom].Count} and {m_CarTracker[Room.PaintingRoom].Count}");
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
            }
        }
    }

    void ManageAnalysisRoomOccupancy()
    {
        // Only one car allowed in the CV room at one time
        if (m_CarTracker[Room.CVRoom].Count == 1)
            Debug.Log("stop now");
    }
}
