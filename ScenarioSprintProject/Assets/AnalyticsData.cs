using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//holds analyics data
public class AnalyticsData : MonoBehaviour
{
    public SimulationManager simulationManager;
    public static AnalyticsData Instance { get; set; }

    //save these to a csv
    public List<float> minorDefectsList = new List<float>();
    public List<float> majorDefectsList = new List<float>();
    public List<float> totalDefectsList = new List<float>();
    public List<float> throughPutOverTimeList = new List<float>();
    public List <float> throughPutOverCarList = new List<float>();
    public List <float> workerUtilizationList = new List<float> ();

    public float avg_throughPutOverTime { get { return GetRollingAverage(throughPutOverTimeList); } }
    public float avg_throughPutOverCar { get { return GetRollingAverage(throughPutOverCarList); } }
    public float avg_paintAmount { get; set; }
    public float avg_energyConsumption { get; set; }
    public float avg_workerUtilization { get { return GetRollingAverage(workerUtilizationList); } }
    public float avg_minorDefects { get { return GetRollingAverage(minorDefectsList); } }
    public float avg_majorDefects { get { return GetRollingAverage(majorDefectsList); } }
    public float avg_totalDefects { get { return GetRollingAverage(totalDefectsList); } }

    public float throughPutOverTime { get { return simulationManager.carsProcessedPerMinute; } }
    public float throughPutOverCar { get { return simulationManager.carProcessingTime; } }
    public float paintAmount { get; set; }
    public int energyConsumption { get; set; }
    public float workerUtilization { get { return simulationManager.totalOperatorUtilization * 100; } }
    public float minorDefects { get {;  return simulationManager.totalMinorDefects; } }
    public float majorDefects { get { return simulationManager.totalMajorDefects; } }
    public float totalDefects { get { return simulationManager.totalMinorDefects + simulationManager.totalMajorDefects; } }

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        StartCoroutine(AddValuesToList());
    }

    private void Update()
    {
    }


    int numPointToAverage = 5;
    private float GetRollingAverage(List<float> list)
    {
        int numPoints = Math.Min(list.Count, numPointToAverage);
        int startPoint = Math.Max(0, list.Count - numPoints);
        float sum = 0;

        for (int i = startPoint; i < list.Count; i++)
        {
            sum += list[i];
        }

        return sum / numPoints;
    }

    WaitForSeconds waitForSeconds = new WaitForSeconds(10f);//maybe should be longer?
    IEnumerator AddValuesToList()
    {
        while (true)
        {
            //add time as a column too
            minorDefectsList.Add(minorDefects);
            majorDefectsList.Add(majorDefects);
            totalDefectsList.Add(totalDefects);
            throughPutOverTimeList.Add(throughPutOverTime);
            throughPutOverCarList.Add(throughPutOverCar);
            workerUtilizationList.Add(workerUtilization);

            yield return waitForSeconds;
        }        
    }
}
