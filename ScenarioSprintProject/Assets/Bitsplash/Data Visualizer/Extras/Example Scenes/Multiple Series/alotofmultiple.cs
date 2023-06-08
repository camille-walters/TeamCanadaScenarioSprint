using DataVisualizer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class alotofmultiple : MonoBehaviour
{
    public int NumCategories = 40;
    public int PointsPerCategory = 1000000;

    public CanvasDataSeriesChart chart;
    public DataSeriesCategory prefab;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < NumCategories; i++)
        {
            string catName = "cat" + i;
            chart.DataSource.AddCategoryFromPrefab(catName, prefab);
            var data = chart.DataSource.GetCategory(catName).Data;
            for (int j = 0; j < PointsPerCategory; j++)
                data.Append(j, Random.Range((float)i,(float)(i+1)));
        }

    }

    // Update is called once per frame
    void Update()
    {
    }
}
