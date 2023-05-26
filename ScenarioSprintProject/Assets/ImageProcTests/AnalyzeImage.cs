using System.IO;
using UnityEngine;
using ImageProcessor;
using SixLabors.ImageSharp;

public class AnalyzeImage : MonoBehaviour
{
    string m_CaptureLocation;
    DirectoryInfo m_Directory;
    // Start is called before the first frame update
    void Start()
    {
        m_CaptureLocation = Application.dataPath + "/ImageProcTests/Screenshots";
        m_Directory = new DirectoryInfo(m_CaptureLocation);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            // Processing images
            
            Debug.Log(m_CaptureLocation);

            var files = m_Directory.GetFiles("*.png");
            foreach (var file in files)
            {
                Debug.Log(file.Name);
            }
            
            var baseImage = File.ReadAllBytes(m_CaptureLocation + "base.png");

        }
    }
}
