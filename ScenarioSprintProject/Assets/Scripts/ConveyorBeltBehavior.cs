using System;
using UnityEngine;

namespace Unity.Simulation.Authoring
{
    // Source: https://www.youtube.com/watch?v=hC1QZ0h4oco
    public class ConveyorBeltBehavior : MonoBehaviour
    {
        public float speed = 0.2f ;
        Rigidbody m_RigidBody;

        Vector3 m_OriginalPositionState;
        Quaternion m_OriginalRotationState;

        void Start()
        {
            m_RigidBody = GetComponent<Rigidbody>();
        }

        void FixedUpdate()
        {
            Vector3 pos = m_RigidBody.position;
            m_RigidBody.position += -transform.up * speed * Time.fixedDeltaTime;
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
}
