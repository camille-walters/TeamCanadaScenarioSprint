#define Graph_And_Chart_PRO
using UnityEngine;
using ChartAndGraph;
using System.Collections;

public class GraphChartFeed : MonoBehaviour
{
    //void Start()
    //{
    //    GraphChartBase graph = GetComponent<GraphChartBase>();
    //    if (graph != null)
    //    {

    //        graph.Scrollable = false;
    //        graph.HorizontalValueToStringMap[0.0] = "Zero"; // example of how to set custom axis strings
    //        graph.DataSource.StartBatch();
    //        graph.DataSource.ClearCategory("Player 1");
    //        graph.DataSource.ClearAndMakeBezierCurve("Player 2");

    //        for (int i = 0; i < 5; i++)
    //        {
    //            graph.DataSource.AddPointToCategory("Player 1", i * 5, Random.value * 10f + 20f);
    //            if (i == 0)
    //                graph.DataSource.SetCurveInitialPoint("Player 2", i * 5, Random.value * 10f + 10f);
    //            else
    //                graph.DataSource.AddLinearCurveToCategory("Player 2",
    //                                                                new DoubleVector2(i * 5, Random.value * 10f + 10f));
    //        }
    //        graph.DataSource.MakeCurveCategorySmooth("Player 2");
    //        graph.DataSource.EndBatch();
    //    }
    //    // StartCoroutine(ClearAll());
    //}

    //IEnumerator ClearAll()
    //{
    //    yield return new WaitForSeconds(5f);
    //    GraphChartBase graph = GetComponent<GraphChartBase>();

    //    graph.DataSource.Clear();
    //}

    public GraphChartBase Graph;
    public int TotalPoints = 5;
    float lastTime = 0f;
    float lastX = 0f;

    void Start()
    {
        if (Graph == null) // the ChartGraph info is obtained via the inspector
            return;
        float x = 0f;
        /////   Graph.DataSource.StartBatch(); // do not call StartBatch for realtime calls , it will only slow down performance.

        Graph.DataSource.ClearCategory("Player 1"); // clear the "Player 1" category. this category is defined using the GraphChart inspector
        Graph.DataSource.ClearCategory("Player 2"); // clear the "Player 2" category. this category is defined using the GraphChart inspector

        for (int i = 0; i < TotalPoints; i++)  //add random points to the graph
        {
            Graph.DataSource.AddPointToCategoryRealtime("Player 1", x, Random.value * 20f + 10f); // each time we call AddPointToCategory 
            Graph.DataSource.AddPointToCategoryRealtime("Player 2", x, Random.value * 10f); // each time we call AddPointToCategory 
            x += Random.value * 3f;
            lastX = x;

        }
        ////  Graph.DataSource.EndBatch(); // do not batch reatlime calls
    }

    void Update()
    {
        float time = Time.time;
        if (lastTime + 2f < time)
        {
            lastTime = time;
            lastX += Random.value * 3f;
            Graph.DataSource.AddPointToCategoryRealtime("Player 1", lastX, Random.value * 20f + 10f, 1f); // each time we call AddPointToCategory 
            Graph.DataSource.AddPointToCategoryRealtime("Player 2", lastX, Random.value * 10f, 1f); // each time we call AddPointToCategory
        }

    }
}
