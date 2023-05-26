using System;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    public GameObject paintingRoomDoors;
    public bool paintingInProgress;

    Vector3 m_DoorPosition;
    void Start()
    {
        m_DoorPosition = paintingRoomDoors.transform.position;
    }

    void Update()
    {
        paintingRoomDoors.transform.localPosition = paintingInProgress ? new Vector3(m_DoorPosition.x, -14, m_DoorPosition.z) : m_DoorPosition;
    }
}
