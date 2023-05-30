using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotArmController : MonoBehaviour
{
    public GameObject targetObject;

    public TargetNode[] targetNodes;
    
    [Tooltip("Delay between each node in seconds")]
    public float delay = 1f;
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ExampleCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    IEnumerator ExampleCoroutine()
    {
        int currentNode = 0;
        while (true)
        {
            SetGlobalTransformation(targetObject, targetNodes[currentNode].transform);
            yield return new WaitForSeconds(delay);
            currentNode = (currentNode + 1) % targetNodes.Length;
        }
    }
    
    public static void SetGlobalTransformation(GameObject childObject, Transform newTransform)
    {
        // Get the parent transform
        Transform parentTransform = childObject.transform.parent;

        // Set the child's local transformation relative to the parent
        childObject.transform.localPosition = parentTransform.InverseTransformPoint(newTransform.position);
        childObject.transform.localRotation = Quaternion.Inverse(parentTransform.rotation) * newTransform.rotation;
        childObject.transform.localScale = newTransform.lossyScale;
    }
}
