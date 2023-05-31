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
    public int currentView;

    Vector3 m_DoorPosition;
    List<Camera> m_Views = new();
    
    GameObject m_CarsGameObject;
    List<GameObject> m_Cars = new();
    List<Room> m_CarCurrentRooms = new();
        
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

        // Switch camera view 
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
        if (m_CarCurrentRooms[lastIndex] == Room.SpawnRoom && m_Cars[lastIndex].GetComponent<Car>().currentRoom == Room.PaintingRoom)
        {
            SpawnCar();
        }
    }

    void SpawnCar()
    {
        var newCar = Instantiate(car, new Vector3(0, 0.6f, 90), Quaternion.Euler(0, 180, 0));
        newCar.transform.parent = m_CarsGameObject.transform;
        newCar.AddComponent<Car>();
        m_Cars.Add(newCar);
        m_CarCurrentRooms.Add(Room.SpawnRoom);
    }
}
