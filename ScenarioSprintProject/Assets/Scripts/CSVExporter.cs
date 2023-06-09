using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class CSVExporter : MonoBehaviour
{
    public string csvPath;
    public ControlPanel input;
    public AnalyticsData output;
    DateTime m_StartTime;

    void Awake()
    {
        csvPath = Application.dataPath + "/SimulationData.csv";
        m_StartTime = DateTime.Now;
    }

    public void OnExportButtonClick()
    {
        try
        {
            using (var writer = new StreamWriter(csvPath))
            {
                Debug.Log("Exporting to CSV...");

                var count = new List<int>
                {
                    output.minorDefectsList.Count,
                    output.majorDefectsList.Count,
                    output.totalDefectsList.Count,
                    output.throughPutOverTimeList.Count,
                    output.throughPutOverCarList.Count,
                    output.workerUtilizationList.Count
                }.Min();
                
                writer.WriteLine("time,minorDefects,majorDefects,totalDefects,throughPutOverTime,throughPutOverCar,workerUtilization");
                for (var i = 0; i < count; ++i)
                {
                    var rowTime = m_StartTime.AddSeconds(i * 10);
                    writer.WriteLine(rowTime.ToString("HH:mm:ss") + "," + 
                        output.minorDefectsList[i] + "," + 
                        output.majorDefectsList[i] + "," + 
                        output.totalDefectsList[i] + "," + 
                        output.throughPutOverTimeList[i] + "," + 
                        output.throughPutOverCarList[i] + "," + 
                        output.workerUtilizationList[i] + ",");
                }

                writer.Flush();
                // writer.Close();
                Debug.Log("Export done");
            }
        }
        catch(Exception exp)  
        {  
            Debug.Log(exp.Message);  
        }
    }
}
