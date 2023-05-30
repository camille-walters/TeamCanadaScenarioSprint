using DataVisualizer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifyViewPortion : MonoBehaviour
{
    public int TotalPoints = 100000;
    public DataSeriesChart chart;

    void Start()
    {
        var data = chart.DataSource.GetCategory("cat12").Data;
        data.Clear();
        Load(data);
    }

    void Load(CategoryDataHolder data)
    {
        // Load some data into the chart
        var arr = new DoubleVector3[TotalPoints];
        for (int i = 0; i < arr.Length; i++)
            data.Append(i, (double)Random.Range(0f, 10f));
    }

    private void Update()
    {
        //update the view portion using chart.Axis.View
        chart.Axis.View.AutomaticHorizontalView = false; // disable automatic view
        chart.Axis.View.AutomaticVerticallView = false;
        chart.Axis.View.HorizontalScrolling = Mathf.Sin(Time.time) * 10000; // set the horzontal scrollnig
        chart.Axis.View.VerticalScrolling = Mathf.Sin(Time.time * 2) * 2; // set the vertical scrolling
    }
}
