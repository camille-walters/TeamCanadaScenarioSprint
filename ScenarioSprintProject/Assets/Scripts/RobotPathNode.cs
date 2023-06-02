using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotPathNode : MonoBehaviour
{
    [Tooltip("Robot action index to pass to the controller. 0 means no new action")]
    public int actionIndex = 0;
    
    [Tooltip("Pause duration in seconds before moving to next node")]
    public float pauseDuration = 0;
}
