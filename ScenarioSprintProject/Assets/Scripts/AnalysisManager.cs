using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class AnalysisManager : MonoBehaviour
{
    SimulationManager m_SimulationManager;
    Thread m_ReceiveThread;
    UdpClient m_Client;
    int m_Port;
    int m_Counter = 0;
    
    void Start()
    {
        m_SimulationManager = this.gameObject.GetComponent<SimulationManager>();
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
                
                // Assign data to car 
                var car = m_SimulationManager.GetCar(m_Counter);
                
                var search = "Minor flaws: ";
                var minor = text.Substring(text.IndexOf(search) + search.Length, 3);
                if (minor.Contains(','))
                {
                    minor = minor.Substring(0, minor.LastIndexOf(","));
                }
                
                search = "Major flaws: ";
                var major = text.Substring(text.IndexOf(search) + search.Length, 3);
                if (major.Contains(','))
                {
                    major = major.Substring(0, major.LastIndexOf(","));
                }

                car.minorFlaws = float.Parse(minor, CultureInfo.InvariantCulture.NumberFormat);
                car.majorFlaws = float.Parse(major, CultureInfo.InvariantCulture.NumberFormat);

                m_Counter += 1;

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
