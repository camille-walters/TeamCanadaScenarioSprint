// Source: https://www.youtube.com/watch?v=hC1QZ0h4oco
using System;
using UnityEngine;

public class ConveyorBeltBehavior : MonoBehaviour
{
    public float speed = 0.2f;
    public bool stop = false;

    float m_InternalSpeed;
    Rigidbody m_RigidBody;
    Vector3 m_OriginalPositionState;
    Quaternion m_OriginalRotationState;

    void Start()
    {
        m_InternalSpeed = speed;
        m_RigidBody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (stop)
        {
            m_InternalSpeed = 0;
            return;
        }
        else
        {
            m_InternalSpeed = speed;
        }
        
        Vector3 pos = m_RigidBody.position;
        m_RigidBody.position += -transform.up * m_InternalSpeed * Time.fixedDeltaTime;
        m_RigidBody.MovePosition(pos);
    }

    public void StartAuthoring()
    {
        enabled = false;
        Debug.Log($"Setting transform [{transform.position}] to [{m_OriginalPositionState}]");
        transform.position = m_OriginalPositionState;
        transform.rotation = m_OriginalRotationState;
    }

    public void StartSimulating()
    {
        enabled = true;
        SaveState();
    }

    public void SaveState()
    {
        m_OriginalPositionState = transform.position;
        m_OriginalRotationState = transform.rotation;
    }
}

