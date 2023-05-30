using DataVisualizer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialDataLoader : MonoBehaviour
{
    public CanvasDataSeriesChart chart;
    void Start()
    {
        var data = chart.DataSource.GetCategory("dataseries-1").Data;
        data.Append(0, 1);
        data.Append(1, 5);
        data.Append(2, 2);
        data.Append(3, 8);
    }
}
