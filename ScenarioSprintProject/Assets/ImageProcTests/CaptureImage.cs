using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureImage : MonoBehaviour
{
    int m_Counter = 0;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            ScreenCapture.CaptureScreenshot("Assets/ImageProcTests/Screenshots/screenshot"+ m_Counter +".png");
            Debug.Log("A screenshot was taken!");
            m_Counter += 1;
        }
    }
}
