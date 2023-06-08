using DataVisualizer;
using UnityEngine;

public class DataLoader : MonoBehaviour
{
    public DataSeriesChart chart; // the chart object
    // Start is called before the first frame update
    void Start()
    {
        var category = chart.DataSource.GetCategory("dataseries-1").Data; // obtain category data
        category.Append(0.0, 0.0); // call append to add a new point to the graph
        category.Append(100.0, 2.0);
        category.Append(200.0, 1.0);
        category.Append(300.0, 5.0);
        category.Append(500.0, 2.0);
    }
}
