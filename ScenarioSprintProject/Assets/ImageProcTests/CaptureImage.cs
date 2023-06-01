using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureImage : MonoBehaviour
{
    public GameObject cameraPositions;
    
    int m_Counter = 0;
    List<Transform> m_Views = new();

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
