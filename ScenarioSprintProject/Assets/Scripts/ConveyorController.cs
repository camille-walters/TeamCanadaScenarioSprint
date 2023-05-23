using System.Collections.Generic;
using UnityEngine;

public class ConveyorController : MonoBehaviour
{
    public float speed = 0.4f;
    public bool stop = false;
    
    List<ConveyorBeltBehavior> m_ChildConveyorBelts = new List<ConveyorBeltBehavior>();
    bool m_InMotion = true;
    void Start()
    {
        foreach (Transform child in gameObject.transform)
        {
            var childBeltBehavior = child.gameObject.GetComponent<ConveyorBeltBehavior>();
            childBeltBehavior.speed = speed;
            
            m_ChildConveyorBelts.Add(childBeltBehavior);
        }
    }

    void Update()
    {
        switch (stop)
        {
            case true when m_InMotion:
                ApplyMotionToAllBelts(true);
                m_InMotion = false;
                break;
            case false when !m_InMotion:
                ApplyMotionToAllBelts(false);
                m_InMotion = true;
                break;
        }
    }

    void ApplyMotionToAllBelts(bool stopBelts)
    {
        foreach (var belt in m_ChildConveyorBelts)
        {
            belt.stop = stopBelts;
        }
    }
}
