using DataVisualizer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DatesAndTimeSpans : MonoBehaviour
{
    public CanvasDataSeriesChart chart;
    void Start()
    {
        var data = chart.DataSource.GetCategory("dataseries-1").Data; // obtain the category data object
        DateTime now = DateTime.Now; // start from the current date
        for (int i = 0; i < 20; i++) 
        {
            // for each i . the x value is now + i days
            data.Append(now + TimeSpan.FromDays(i), UnityEngine.Random.Range(0, 10f));
        }
    }
}
