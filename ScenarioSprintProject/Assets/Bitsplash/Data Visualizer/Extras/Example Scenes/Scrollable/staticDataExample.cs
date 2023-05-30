using DataVisualizer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class staticDataExample : MonoBehaviour
{
    public DataSeriesChart chart;
    public uint DataPoints = 300000;

    void Start()
    {
        var data = chart.DataSource.GetCategory("cat12").Data;
        data.Clear();
        Load(data);
    }


    void Load(CategoryDataHolder data)
    {
        // Load some data into the chart
        double y = 5f;
        for (int i = 0; i < DataPoints; i++)
        {
            y += (double)UnityEngine.Random.Range(-5f, 5f);
            data.Append(i, y); // append a new point to the chart
        }
    }
}
