using System;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    public GameObject paintingRoomDoors;
    public GameObject simulationViews;
    public bool paintingInProgress;
    public int currentView;

    Vector3 m_DoorPosition;
    List<Camera> m_Views = new();
    GameObject m_Cars;
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

        m_Cars = new GameObject();
    }

    void Update()
    {
        paintingRoomDoors.transform.localPosition = paintingInProgress ? new Vector3(m_DoorPosition.x, -10, m_DoorPosition.z) : m_DoorPosition;

        if (Input.GetKeyDown(KeyCode.C) && simulationViews != null)
        {
            m_Views[currentView].enabled = false;
            
            if (currentView < 3)
                currentView += 1;
            else
                currentView = 0;
            
            m_Views[currentView].enabled = true;
        }
    }
}
