using UnityEngine;
using System.Collections;
using DataVisualizer;

public class ReatimeDataSeriesTest : MonoBehaviour
{
    public DataSeriesChart chart;
    /// <summary>
    /// amount of elements in the data array
    /// </summary>
    public int amount = 1000;

    /// <summary>
    /// realtime data array
    /// </summary>
    DoubleVector3[] dataArray;
    void Start()
    {
        var data = chart.DataSource.GetCategory("cat12").Data;
        data.Clear();
        //initialize the data array
        dataArray = new DoubleVector3[amount];
        for (int i=0; i< amount; i++)
        {
            data.Append(i, Random.value);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //obtain the category object from the chart
        var data = chart.DataSource.GetCategory("cat12").Data;
        //repopulate the data array with random data
        for (int i = 0; i < amount; i++)
            dataArray[i] = new DoubleVector3(i, Random.value);
        //apply the data to the chart each frame
        data.SetPositionArray(dataArray, 0, dataArray.Length);
    }
}
