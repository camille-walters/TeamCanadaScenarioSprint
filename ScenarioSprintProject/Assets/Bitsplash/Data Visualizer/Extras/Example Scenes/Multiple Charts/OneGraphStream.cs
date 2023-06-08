using DataVisualizer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneGraphStream : MonoBehaviour
{
    public DataSeriesChart chart;
    public int PointsPerFrame = 50;
    public int MaxPoints = 100000;
    public int RemovePoints = 15000;

    // Use this for initialization
    double x = 4;
    double y = 5.0;

    void Start()
    {
        var data = chart.DataSource.GetCategory("dataseries-1").Data;
        data.Clear();
    }

    // Update is called once per frame
    void Update()
    {
        var data = chart.DataSource.GetCategory("dataseries-1").Data;
        for (int i = 0; i < PointsPerFrame; i++) // append all points
        {
            y = Mathf.Clamp((float)y + Random.Range(-0.05f, 0.05f), 0f, 10f); // randomize a y value
            data.Append(x, y); // append a new point
            x++;
        }
        if (data.Count > MaxPoints) // if the point count is larger then MaxPoints
            data.RemoveAllBefore(chart.Axis.View.HorizontalViewStart); // remove all points that are left of the begining of the view portion
    }
}
