using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChartAndGraph;

public class ThroughputChartFeed : MonoBehaviour
{
    public GraphChart Graph;
    public AnalyticsData analyticsData;

    void Start()
    {
        if (Graph == null) // the ChartGraph info is obtained via the inspector
            return;
        float x = 0f;

        Graph.DataSource.ClearCategory("Total Defects"); // clear the "Total Defects" category. this category is defined using the GraphChart inspector

        //for (int i = 0; i < TotalPoints; i++)  //add random points to the graph
        //{
        //    Graph.DataSource.AddPointToCategoryRealtime("Total Defects", x, Random.value * 20f + 10f); // each time we call AddPointToCategory 
        //    x += Random.value * 3f;
        //    lastX = x;

        //}

        analyticsData = GetComponentInParent<AnalyticsData>();
        if (analyticsData == null)
        {
            Debug.Log("didnt find analytics data");
        }


    }

    private void OnEnable()
    {
        StartCoroutine(RedrawChart());
    }

    void Update()
    {
        //float time = Time.time;
        //if (lastTime + 2f < time)
        //{
        //    lastTime = time;
        //    lastX += Random.value * 3f;
        //    Graph.DataSource.AddPointToCategoryRealtime("Total Defects", lastX, Random.value * 20f + 10f, 1f); // each time we call AddPointToCategory 
        //    //Graph.DataSource.AddPointToCategoryRealtime("Total Defects", lastX, AnalyticsData.avg_totalDefects, 1f); // each time we call AddPointToCategory 
        //}

    }

    WaitForSeconds waitForSeconds = new WaitForSeconds(2f);//maybe should be longer?
    //graph gets rerendered every time. This is needed because whenever we deactivate it , the values will not update properly
    IEnumerator RedrawChart()
    {
        while (true)
        {
            // Graph.DataSource.ClearCategory("Total Defects"); // clear the "Total Defects" category. this category is defined using the GraphChart inspector
            // Graph.DataSource.StartBatch();
            // Debug.Log("addding val");
            // lastX += Random.value * 3f;
            // Debug.Log(analyticsData.totalDefectsList.Count);

            // for (int i = 0; i < analyticsData.totalDefectsList.Count; i++)
            // {
            //     Graph.DataSource.AddPointToCategory("Total Defects", lastX, analyticsData.totalDefectsList[i]);
            // }

            //Graph.DataSource.EndBatch(); 
            Graph.DataSource.ClearCategory("Total Defects");
            Graph.DataSource.StartBatch();
            var numPoints = analyticsData.totalDefectsList.Count;
            float x = 0f;
            for (int i = 0; i < numPoints; i++)  //add random points to the graph
            {
                //Graph.DataSource.AddPointToCategoryRealtime("Total Defects", x, Random.value * 20f + 10f); // each time we call AddPointToCategory 
                Graph.DataSource.AddPointToCategoryRealtime("Total Defects", x, analyticsData.totalDefectsList[i]); // each time we call AddPointToCategory 
                x += 3f;

            }
            //TotalPoints++;
            Graph.DataSource.EndBatch();

            yield return waitForSeconds;
        }
    }

}
