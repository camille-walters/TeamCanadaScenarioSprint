using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChartAndGraph;

public class ThroughputChartFeed : MonoBehaviour
{
    public GraphChart Graph;
    public AnalyticsData analyticsData;
    float graphInterval = 60f;

    void Start()
    {
        if (Graph == null) // the ChartGraph info is obtained via the inspector
            return;
        float x = 0f;

        Graph.DataSource.ClearCategory("Total Defects"); // clear the "Total Defects" category. this category is defined using the GraphChart inspector

        analyticsData = GetComponentInParent<AnalyticsData>();
        if (analyticsData == null)
        {
            Debug.Log("didnt find analytics data");
        }
        StartCoroutine(RedrawChart());
    }

    private void OnEnable()
    {
        StartCoroutine(RedrawChart());
    }

 
    WaitForSeconds waitForSeconds = new WaitForSeconds(60f);//maybe should be longer?
    public void SetTimeInterval(float interval)
    {
        waitForSeconds = new WaitForSeconds(interval);
        graphInterval = interval;
    }
    //graph gets rerendered every time. This is needed because whenever we deactivate it , the values will not update properly
    IEnumerator RedrawChart()
    {
        while (true)
        {
            Graph.DataSource.ClearCategory("Total Defects");
            Graph.DataSource.StartBatch();
            var numPoints = analyticsData.totalDefectsList.Count;
            float x = 0f;
            for (int i = 0; i < numPoints; i++)  //add random points to the graph
            {
                Graph.DataSource.AddPointToCategoryRealtime("Total Defects", x, analyticsData.totalCarsProcessedList[i]); // each time we call AddPointToCategory 
                x += graphInterval;

            }
            Graph.DataSource.EndBatch();

            yield return waitForSeconds;
        }
    }

}
