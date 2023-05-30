using DataVisualizer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class addRemoveCategories : MonoBehaviour
{
    public int TotalPoints = 30;
    public DataSeriesChart seriesChart;

    public DataSeriesCategory CategoryPrefab;
    public DataSeriesVisualFeature VisualFeaturePrefab;

    // Start is called before the first frame update
    void Start()
    {
        var cat12 = seriesChart.DataSource.GetCategory("cat12"); // obtain the category object
        cat12.RemoveVisualFeature("Graph Line-0"); //remove the graph line visual feature

        seriesChart.DataSource.RemoveCategory("cat12");// remove category

        seriesChart.DataSource.AddCategoryFromPrefab("newCat", CategoryPrefab); // add a category named newCat
        var categoryObject = seriesChart.DataSource.GetCategory("newCat"); // obtain the category object for newCat
        categoryObject.AddVisualFeature("NewGraphLine",VisualFeaturePrefab); // add a graph line to newCat
        var data = categoryObject.Data;
        data.Clear(); // clear the data of newCat
        Load(data); // load some data to newCat
    }
    void Load(CategoryDataHolder data)
    {
        // Load some data into the chart
        for (int i = 0; i < TotalPoints; i++)
           data.Append(i, (double)Random.Range(0f, 10f)); // append a point to the data object
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
