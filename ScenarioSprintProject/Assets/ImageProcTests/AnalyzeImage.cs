using System;
using System.IO;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Debug = UnityEngine.Debug;

public class AnalyzeImage : MonoBehaviour
{
    // string m_CaptureLocation;
    // DirectoryInfo m_Directory;
    
    Thread m_ReceiveThread;
    UdpClient m_Client;
    int m_Port;
    
    void Start()
    {
        // m_CaptureLocation = Application.dataPath + "/ImageProcTests/Images";
        // m_Directory = new DirectoryInfo(m_CaptureLocation);

        InitializeUDP();
    }

    void InitializeUDP()
    {
        print ("UPDSend.init()");
        m_Port = 5065;

        print ("Sending to 127.0.0.1 : " + m_Port);

        m_ReceiveThread = new Thread (new ThreadStart(ReceiveData))
        {
            IsBackground = true
        };
        m_ReceiveThread.Start ();

    }

    void ReceiveData()
    {
        m_Client = new UdpClient (m_Port);
        while (true) 
        {
            try
            {
                var anyIP = new IPEndPoint(IPAddress.Parse("0.0.0.0"), m_Port);
                var data = m_Client.Receive(ref anyIP);

                var text = Encoding.UTF8.GetString(data);
                Debug.Log("Received: " + text);

                break;

            }
            catch(Exception e){
                print (e.ToString());
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            /*
            // Processing images by executing Python code
            
            Debug.Log("processing images");
            var strCmdText= "/C C:/Users/priyanka.cs/anaconda3/python " + "Assets/ImageProcTests/image-analyzer-test.py & pause";   //This command to open a new notepad
            Process.Start("CMD.exe",strCmdText); //Start cmd process
            */
        }
    }

	void OnApplicationQuit()
    {
        m_ReceiveThread?.Abort();
    }
}
