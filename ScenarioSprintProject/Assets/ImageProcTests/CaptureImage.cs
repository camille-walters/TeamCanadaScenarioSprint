using UnityEngine;

public class CaptureImage : MonoBehaviour
{
    int m_Counter = 0;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            ScreenCapture.CaptureScreenshot("Assets/ImageProcTests/Images/base.png");
            Debug.Log("Base screenshot was taken!");
        }
        
        if (Input.GetKeyDown(KeyCode.C))
        {
            ScreenCapture.CaptureScreenshot("Assets/ImageProcTests/Images/"+ m_Counter +".png");
            Debug.Log("A screenshot was taken!");
            m_Counter += 1;
        }
    }
}
