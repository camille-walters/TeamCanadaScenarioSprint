using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotArmController : MonoBehaviour
{
    [Tooltip("Robots end node")]
    public Transform robotEndNode;
    
    [Tooltip("Robots IK target 0 (from)")]
    public Transform robotIKTarget0;

    [Tooltip("Robots IK target 1 (to)")]
    public Transform robotIKTarget1;

    [Tooltip("Minimum distance to target before moving to next node")]
    public float minDistToTarget = 0.01f;

    [Tooltip("Path to follow")]
    public RobotPath robotPath;

    [Tooltip("Distance of target node from surface of path node")]
    public float distanceFromSurface = 0;
    
    [Tooltip("Spray behavior")]
    public SprayBehavior sprayBehavior;

    // Current path node index
    int currentPathNodeIndex = -1;
    
    void Start()
    {
        if (sprayBehavior != null)
        {
            sprayBehavior.Stop();
        }
        
        if (robotPath != null)
        {// for testing, initialize path in inspector
            SetNewRobotPath(robotPath);
        }
    }

    void Update()
    {
        UpdateRobotTarget();
    }

    public void SetNewRobotPath(RobotPath robotPath)
    {
        this.robotPath = robotPath;
        if (robotPath != null && robotPath.pathNodes != null && robotPath.pathNodes.Length > 0)
        {
            currentPathNodeIndex = 0;
        }
    }

    /// <summary>
    /// Update robot target to follow path
    /// </summary>
    void UpdateRobotTarget()
    {
        if (currentPathNodeIndex >= 0 && robotPath != null && robotPath.pathNodes != null && robotPath.pathNodes.Length > 0)
        {
            var currNode = robotPath.pathNodes[currentPathNodeIndex];
            var pos1 = currNode.transform.position + currNode.transform.up * distanceFromSurface;
            var pos0 = currNode.transform.position + currNode.transform.up * (distanceFromSurface + 0.5f);
            robotIKTarget0.localPosition = robotIKTarget0.transform.parent.InverseTransformPoint(pos0);
            robotIKTarget1.localPosition = robotIKTarget1.transform.parent.InverseTransformPoint(pos1);
            if (Vector3.Distance(pos1, robotEndNode.position) < minDistToTarget)
            {
                currentPathNodeIndex = (currentPathNodeIndex < robotPath.pathNodes.Length - 1) ? currentPathNodeIndex + 1 : -1;

                if (sprayBehavior != null)
                {
                    if (currentPathNodeIndex != -1)
                    {
                        sprayBehavior.Play();
                    }
                    else
                    {
                        sprayBehavior.Stop();
                    }
                }
            }
        }
    }
}
