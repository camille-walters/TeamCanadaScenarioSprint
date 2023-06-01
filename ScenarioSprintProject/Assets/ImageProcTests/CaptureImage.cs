using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureImage : MonoBehaviour
{
    public GameObject cameraPositions;
    public Camera testCamera;
    
    int m_Counter = 0;
    List<Transform> m_Views = new();
    int resWidth = 782; 
    int resHeight = 440;

    void Start()
    {
        for (var i = 0; i < cameraPositions.transform.childCount; ++i)
        {
            m_Views.Add(cameraPositions.transform.GetChild(i));
        }
        Debug.Log($"total views are {m_Views.Count}");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            // Captures base images from each pov
            StartCoroutine(CaptureFromAllPositions(1.5f, "base"));
        }
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            // Captures base images from each pov
            StartCoroutine(CaptureFromAllPositions(1.5f, "flawed"));
        }
        
        if (Input.GetKeyDown(KeyCode.T)) 
        {
            RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
            testCamera.targetTexture = rt;
            Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            testCamera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            testCamera.targetTexture = null;
            RenderTexture.active = null; // JC: added to avoid errors
            Destroy(rt);
            byte[] bytes = screenShot.EncodeToPNG();
            string filename = ScreenShotName(resWidth, resHeight);
            System.IO.File.WriteAllBytes(filename, bytes);
            Debug.Log(string.Format("Took screenshot to: {0}", filename));
        }
    }
 
    static string ScreenShotName(int width, int height) {
        return string.Format("{0}/ImageProcTests/screen_{1}x{2}_{3}.png", 
            Application.dataPath, 
            width, height, 
            System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
    }
    
    
    IEnumerator CaptureFromAllPositions(float time, string imageType)
    {
        var viewCounter = 0;
        foreach (var view in m_Views)
        {
            gameObject.transform.position = view.position;
            gameObject.transform.rotation = view.rotation;
            
            yield return new WaitForSeconds(time);
            
            ScreenCapture.CaptureScreenshot($"Assets/ImageProcTests/Images/{imageType}{viewCounter}.png");
            
            yield return new WaitForSeconds(time);
            viewCounter += 1;
        }
        Debug.Log($"{viewCounter} {imageType} screenshots were taken!");
    }
}
