using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChartAndGraph;

public class DefectChartFeed : MonoBehaviour
{
    public GraphChart Graph;
    public int TotalPoints = 5;
    float lastTime = 0f;
    float lastX = 0f;
    public AnalyticsData analyticsData;

    void Start()
    {
        if (Graph == null) // the ChartGraph info is obtained via the inspector
            return;
        float x = 0f;

        Graph.DataSource.ClearCategory("Total Defects"); // clear the "Total Defects" category. this category is defined using the GraphChart inspector

        for (int i = 0; i < TotalPoints; i++)  //add random points to the graph
        {
            Graph.DataSource.AddPointToCategoryRealtime("Total Defects", x, Random.value * 20f + 10f); // each time we call AddPointToCategory 
            x += Random.value * 3f;
            lastX = x;

        }

        analyticsData = GetComponentInParent<AnalyticsData>();
        if(analyticsData == null)
        {
            Debug.Log("didnt find analytics data");
        }

        //StartCoroutine(AddValueToChart());        
    }

    void Update()
    {
        float time = Time.time;
        if (lastTime + 2f < time)
        {
            lastTime = time;
            lastX += Random.value * 3f;
            Graph.DataSource.AddPointToCategoryRealtime("Total Defects", lastX, Random.value * 20f + 10f, 1f); // each time we call AddPointToCategory 
            //Graph.DataSource.AddPointToCategoryRealtime("Total Defects", lastX, AnalyticsData.avg_totalDefects, 1f); // each time we call AddPointToCategory 
        }
    }

    WaitForSeconds waitForSeconds = new WaitForSeconds(10f);//maybe should be longer?
    IEnumerator AddValueToChart()
    {
        while (true)
        {
            Graph.DataSource.AddPointToCategoryRealtime("Total Defects", Time.realtimeSinceStartup, analyticsData.avg_totalDefects, 1f); // each time we call AddPointToCategory 

            yield return waitForSeconds;
        }
    }

}
