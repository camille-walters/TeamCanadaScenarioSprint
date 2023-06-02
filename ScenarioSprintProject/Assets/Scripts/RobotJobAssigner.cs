using UnityEngine;

public class RobotJobAssigner : MonoBehaviour
{
    public RobotArmController robotArmController;
    public int pathIndex = 0;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PaintableCar paintableCar))
        {
            if (pathIndex < paintableCar.paths.Length)
            {
                Debug.Log($"Assigning new job ({paintableCar.name}) to robot ({robotArmController.name})");
                robotArmController.SetNewRobotPath(paintableCar.paths[pathIndex]);
            }
        }
    }
    
}
