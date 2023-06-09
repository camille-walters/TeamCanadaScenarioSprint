using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class CSVExporter : MonoBehaviour
{
    public string csvPath;
    public float timeInterval = 10;
    public DefectChartFeed defectChartFeed;
    public ThroughputChartFeed throughputChartFeed;
    ControlPanel m_Input;
    AnalyticsData m_Output;
    DateTime m_StartTime;

    void Awake()
    {
        if (csvPath == "")
            csvPath = Application.dataPath + "/SimulationData.csv";
        m_StartTime = DateTime.Now;
        m_Input = FindObjectOfType<ControlPanel>();
        m_Output = FindObjectOfType<AnalyticsData>();
        
        // var defectChartFeed = FindObjectOfType<DefectChartFeed>();
        // var throughputChartFeed = FindObjectOfType<ThroughputChartFeed>();

        m_Input.SetTimeInterval(timeInterval);
        m_Output.SetTimeInterval(timeInterval);
        defectChartFeed.SetTimeInterval(timeInterval);
        throughputChartFeed.SetTimeInterval(timeInterval);
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
                    m_Input.conveyorSpeedList.Count,
                    m_Input.sprayRadiusList.Count,
                    m_Input.sprayAngleList.Count,
                    m_Input.sprayPressureList.Count,
                    m_Input.distanceFromCarList.Count,
                    m_Input.movementSpeedList.Count,
                    m_Input.numberOfWorkersList.Count,
                    m_Input.timeToFixMinorDefectsList.Count,
                    m_Input.timeToFixMajorDefectsList.Count,
                    m_Output.minorDefectsList.Count,
                    m_Output.majorDefectsList.Count,
                    m_Output.totalDefectsList.Count,
                    m_Output.throughPutOverTimeList.Count,
                    m_Output.throughPutOverCarList.Count,
                    m_Output.workerUtilizationList.Count,
                    m_Output.totalCarsProcessedList.Count
                }.Min();
                
                writer.WriteLine("time," + 
                    "conveyorSpeed,sprayRadius,sprayAngle,sprayPressure,distanceFromCar,movementSpeed,numberOfWorkers,timeToFixMinorDefects,timeToFixMajorDefects," +
                    "minorDefects,majorDefects,totalDefects,throughPutOverTime,throughPutOverCar,workerUtilization,totalCarsProcessed");
                for (var i = 0; i < count; ++i)
                {
                    var rowTime = m_StartTime.AddSeconds(i * timeInterval);
                    writer.WriteLine(rowTime.ToString("HH:mm:ss") + "," +
                        m_Input.conveyorSpeedList[i].ToString("0.###") + "," + 
                        m_Input.sprayRadiusList[i].ToString("0.###") + "," + 
                        m_Input.sprayAngleList[i].ToString("0.###") + "," + 
                        m_Input.sprayPressureList[i].ToString("0.###") + "," + 
                        m_Input.distanceFromCarList[i].ToString("0.###") + "," + 
                        m_Input.movementSpeedList[i].ToString("0.###") + "," + 
                        m_Input.numberOfWorkersList[i] + "," + 
                        m_Input.timeToFixMinorDefectsList[i].ToString("0.###") + "," + 
                        m_Input.timeToFixMajorDefectsList[i].ToString("0.###") + "," +
                        m_Output.minorDefectsList[i].ToString("0.###") + "," + 
                        m_Output.majorDefectsList[i].ToString("0.###") + "," + 
                        m_Output.totalDefectsList[i].ToString("0.###") + "," + 
                        m_Output.throughPutOverTimeList[i].ToString("0.###") + "," + 
                        m_Output.throughPutOverCarList[i].ToString("0.###") + "," + 
                        m_Output.workerUtilizationList[i].ToString("0.###") + "," +
                        m_Output.totalCarsProcessedList[i].ToString("0.###"));
                }

                writer.Flush();
                // writer.Close();
                Debug.Log("Export done");
            }
        }
        catch(Exception exp)  
        {  
            Debug.LogWarning("ERROR:" + exp.Message);  
        }
    }
}
