using DataVisualizer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class multipleseriesexample : MonoBehaviour
{
    public DataSeriesChart chart;
    public int TotalPoint = 100;
    float time = 1f;

    // Use this for initialization
    double x = 0;

    void Start()
    {

        //clear all cateogry data
        var series1 = chart.DataSource.GetCategory("series-1").Data;
        var series2 = chart.DataSource.GetCategory("series-2").Data;
        var series3 = chart.DataSource.GetCategory("series-3").Data;

        series1.Clear();
        series2.Clear();
        series3.Clear();
    }

    // Update is called once per frame
    void Update()
    {
        time -= Time.deltaTime;
        if (time < 0f && x < TotalPoint)
        {
            time = 0.01f;
            //obtain category objects
            var series1 = chart.DataSource.GetCategory("series-1").Data;
            var series2 = chart.DataSource.GetCategory("series-2").Data;
            var series3 = chart.DataSource.GetCategory("series-3").Data;

            //append a point to each category
            series1.Append(x, (double)Random.Range(10f, 15f));
            series2.Append(x, (double)Random.Range(5f, 10f));
            series3.Append(x, (double)Random.Range(0f, 5f));
            x++;
        }

    }
}
