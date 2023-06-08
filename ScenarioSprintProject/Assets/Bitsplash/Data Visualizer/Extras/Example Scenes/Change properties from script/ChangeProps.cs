using DataVisualizer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeProps : MonoBehaviour
{
    public Material Fill1;
    public Material Fill2;
    public DataSeriesChart chart;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(UpdateProps());
    }

    IEnumerator UpdateProps()
    {
        yield return new WaitForSeconds(2);
        while (true)
        {
            var cat = chart.DataSource.GetCategory("dataseries-1");

            //obtain the point visual property
            var point = cat.GetVisualFeature<GraphPointVisualFeature>("Graph Point-0");
            //randomize a point size
            point.PointSize = Random.value * 1f + 1f;

            //obtain the fill visual property
            var fill = cat.GetVisualFeature<GraphFillVisualFeature>("Graph Fill-0");
            
            //randomize the fill material 
            fill.CanvasSortOrder = Random.value > 0.5 ? -1 : 1;
            fill.FillMaterial = Random.value > 0.5 ? Fill1 : Fill2;
            
            //get the x divisions of the axis
            var xDiv = chart.Axis.GetVisualFeature<FixedDivision2DAxisVisualFeature>("XDiv");
            // randomize a new gap value
            xDiv.GapUnits = Random.Range(10, 1000);

            //get the axis labels 
            var itemLables = chart.Axis.GetVisualFeature<AxisLables2DVisualFeature>("2D Item Labels-0");
            //randomize the font size
            itemLables.FontSize = Random.Range(12, 20);
            // update properties every 2 seconds
            yield return new WaitForSeconds(2);
        }
    }
    // Update is called once per frame
    void Update()
    {
    }
}
