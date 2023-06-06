using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Car : MonoBehaviour
{
    public string carID;
    public Room currentRoom;
    public bool paintingComplete;
    public bool analysisComplete;
    public bool fixingComplete;

    public float minorFlaws = 0;
    public float majorFlaws = 0;

    GameObject m_CentralConveyors;
    List<GameObject> m_Conveyors = new();
    Dictionary<int, string> m_Rooms = new Dictionary<int, string>();

    // Note: Ready to exit painting room at 29.68
    // static readonly List<float> k_RoomBordersZ = new List<float> {96.78f, 79.92f, 22.87f, -15.53f, -28.41f};

    void Start()
    {
        carID = Guid.NewGuid().ToString();
        currentRoom = Room.SpawnRoom;
        
        m_CentralConveyors = GameObject.Find("CentralConveyor");

        for (var i = 0; i < m_CentralConveyors.transform.childCount; ++i)
        {
            m_Conveyors.Add(m_CentralConveyors.transform.GetChild(i).gameObject);
        }

        foreach (var room in Enum.GetValues(typeof(Room)))
        {
            m_Rooms.Add((int)room, room.ToString());
        }
    }

    void Update()
    {
        UpdateRoom();
    }

    void UpdateRoom()
    {
        var conveyorColliders = m_Conveyors.Select(c => c.GetComponent<BoxCollider>()).ToList();
        
        for (var i = 0; i < conveyorColliders.Count; ++i)
        {
            var b = new Bounds(conveyorColliders[i].center, conveyorColliders[i].size);
            var p = conveyorColliders[i].transform.InverseTransformPoint(this.transform.position);
            
            if (b.Contains(p))
            {
                Enum.TryParse(m_Rooms[i], out currentRoom);

                switch (currentRoom)
                {
                    case Room.BufferRoom:
                        // Painting is complete
                        paintingComplete = true;
                        break;
                    case Room.QARoom:
                        // CV Capture is complete
                        analysisComplete = true;
                        break;
                }

                break;
            }
            
            if (currentRoom == Room.QARoom && i == 4 && !b.Contains(p))
            {
                currentRoom = Room.ProcessedRoom;
            }
        }
    }
}

public enum Room
{
    SpawnRoom,
    PaintingRoom,
    BufferRoom,
    CVRoom,
    QARoom,
    ProcessedRoom
}
