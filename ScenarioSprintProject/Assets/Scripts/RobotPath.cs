using UnityEngine;

public class RobotPath : MonoBehaviour
{
    public RobotPathNode[] pathNodes { get; private set; }

    // Start is called before the first frame update
    void Awake()
    {
        pathNodes = GetComponentsInChildren<RobotPathNode>();
    }

    void OnDrawGizmos()
    {
        pathNodes = GetComponentsInChildren<RobotPathNode>();
        Gizmos.color = Color.cyan;
        for (int i = 0; i < pathNodes.Length - 1; ++i)
        {
            Gizmos.DrawLine(pathNodes[i].transform.position, pathNodes[i+1].transform.position);
        }
    }    
}
