using System;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorController : MonoBehaviour
{
    public float speed = 0.4f;
    public float stopTime = 5;
    public bool stop = false;
    public bool stopForTime = false;
    
    List<ConveyorBeltBehavior> m_ChildConveyorBelts = new();
    bool m_InMotion = true;
    float m_StopTimeValue; 
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
        
        if (stopForTime && m_InMotion)
        {
            if (m_StopTimeValue == 0)
            {
                m_StopTimeValue = stopTime;
            }
            
            ApplyMotionToAllBelts(true);
            m_InMotion = false;
            StopCountdown();
        }
        
        // Update speed if changed
        if (Math.Abs(gameObject.transform.GetChild(0).GetComponent<ConveyorBeltBehavior>().speed - speed) > 0.1f)
        {
            foreach (Transform child in gameObject.transform)
            {
                var childBeltBehavior = child.gameObject.GetComponent<ConveyorBeltBehavior>();
                childBeltBehavior.speed = speed;
            }
            
        }
    }

    void ApplyMotionToAllBelts(bool stopBelts)
    {
        foreach (var belt in m_ChildConveyorBelts)
        {
            belt.stop = stopBelts;
        }
    }

    void StopCountdown()
    {
        if (stopTime > 0)
        {
            stopTime -= Time.deltaTime;
        }
        else
        {
            stopTime = m_StopTimeValue;
            m_StopTimeValue = 0;
            ApplyMotionToAllBelts(false);
            stopForTime = false;
            m_InMotion = true;
        }
    }
}
