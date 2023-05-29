using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Debug = UnityEngine.Debug;

public class AnalyzeImage : MonoBehaviour
{
    string m_CaptureLocation;
    DirectoryInfo m_Directory;
    
    

    Thread receiveThread;
    UdpClient client;
    int port;

    string lastReceivedUDPPacket = "";
    string allReceivedUDPPackets = "";

    Vector3 up;
    bool jump;
    
    void Start()
    {
        m_CaptureLocation = Application.dataPath + "/ImageProcTests/Screenshots";
        m_Directory = new DirectoryInfo(m_CaptureLocation);

        InitializeUDP();
    }

    void InitializeUDP()
    {
        print ("UPDSend.init()");
        port = 5065;

        print ("Sending to 127.0.0.1 : " + port);

        receiveThread = new Thread (new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start ();

    }

    void ReceiveData()
    {
        client = new UdpClient (port);
        while (true) 
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Parse("0.0.0.0"), port);
                var data = client.Receive(ref anyIP);

                var text = Encoding.UTF8.GetString(data);
                Debug.Log("Received: " + text);
                lastReceivedUDPPacket=text;
                allReceivedUDPPackets=allReceivedUDPPackets+text;

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
            // Processing images
            
            Debug.Log("processing images");
        }
    }

	void OnApplicationQuit(){
		if (receiveThread != null) {
			receiveThread.Abort();
		}
	}
}
