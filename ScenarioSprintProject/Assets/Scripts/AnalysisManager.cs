using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class AnalysisManager : MonoBehaviour
{
    Thread m_ReceiveThread;
    UdpClient m_Client;
    int m_Port;
    
    void Start()
    {
        InitializeUDP();
    }

    void InitializeUDP()
    {
        Debug.Log("UPDSend.init()");
        m_Port = 5065;

        Debug.Log("Sending to 127.0.0.1 : " + m_Port);

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

                // break;

            }
            catch(Exception e){
                print (e.ToString());
            }
        }
    }

    void OnApplicationQuit()
    {
        m_ReceiveThread?.Abort();
    }
}
