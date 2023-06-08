using DataVisualizer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensorInput : MonoBehaviour
{
    public DataSeriesChart chart;
    void Start()
    {
        var data = chart.DataSource.GetCategory("cat12").Data;
        data.Append(0, 0);
        data.Append(1, 0);
        StartCoroutine(UpdateRoutine());
    }
    IEnumerator UpdateRoutine() // simulate sensor input
    {
        var data = chart.DataSource.GetCategory("cat12").Data;
        while(true)
        {
            float start = Time.time;
            float duration = Random.value * 10f;
            while (start + duration >= Time.time)
            {
                data.SetLast(new DoubleVector3(Time.time, 0.0));
                yield return 0;
            }
            foreach (object o in Noise())
                yield return o;
        }
    }
    IEnumerable Noise()
    {
        var data = chart.DataSource.GetCategory("cat12").Data;
        float start = Time.time;
        float duration = Random.value * 3f;
        double val = 0;
        while (start + duration >= Time.time)
        {
            for (int i = 0; i < 10; i++)
            {
                val += (Random.value*2f-1f) * 0.1f;
                data.Append(Time.time, val);
            }
            yield return 0;
        }
        data.Append(Time.time, 0);
        data.Append(Time.time, 0);
    }
    // Update is called once per frame
    void Update()
    {
        var data = chart.DataSource.GetCategory("cat12").Data;
        //;
    }
}
