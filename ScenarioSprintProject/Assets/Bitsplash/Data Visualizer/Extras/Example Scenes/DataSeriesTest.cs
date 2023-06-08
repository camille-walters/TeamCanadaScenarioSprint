 using UnityEngine;
using System.Collections;
using DataVisualizer;
using UnityEngine.UI;

public class DataSeriesTest : MonoBehaviour
{
    public DataSeriesChart chart;
    public int PointsPerFrame = 100;
    public int TotalPoints = 1000000;
    public float Fuzziness = 5f;
    public bool Clamp = true;
    public Text InfoBox;
    double y = 5.0;
    int count = 0;
    double x = 0;

    void Start()
    {
        var data = chart.DataSource.GetCategory("dataseries-1").Data;
        data.Clear();
    }
    private void OnValidate()
    {
    }


    // Update is called once per frame
    void Update()
    {        
        if (x < TotalPoints)
        { 
            var data = chart.DataSource.GetCategory("dataseries-1").Data;
            //append 100 points at a time
            for (int i = 0; i < PointsPerFrame; i++)
            {
                y = (float)y + Random.Range(-Fuzziness * 0.01f, Fuzziness * 0.01f);
                if (Clamp)
                    y = Mathf.Clamp((float)y, 0f, 10f);
                data.Append(x, y);
                x++;
                count++;
            }            
            if(InfoBox != null)
            {
                InfoBox.text = "Total Points: " + count;
            }
        }
    }
}
