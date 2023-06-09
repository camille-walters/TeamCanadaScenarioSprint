using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ChartAndGraph;

//updates Panel with Data from AnalyticsData
public class AnalyticsPanel : MonoBehaviour 
{
    public TMP_Dropdown dropdown;
    List<string> dropdownOptions = new List<string>() { "Throughput over Time", "Defects over Time" };

    public TMP_Text throughPutOverTime;
    public TMP_Text throughPutOverCar;
    public TMP_Text paintAmount;
    public TMP_Text energyConsumption;
    public TMP_Text workerUtilization;
    public TMP_Text totalDefects;
    public TMP_Text majorDefects;
    public TMP_Text minorDefects;

    public GraphChart defectsGraph;
    public GraphChart throughputGraph;

    // Start is called before the first frame update
    void Start()
    {
        PopulateDropdownList();
        throughputGraph.gameObject.SetActive(true);
        defectsGraph.gameObject.SetActive(false);

    }
    private void Update()
    {
        StartCoroutine("UpdateValues");
    }

    public void DropdownIndexChanged(int index)
    {
        if (index == 0)
        {
            throughputGraph.gameObject.SetActive(true);
            defectsGraph.gameObject.SetActive(false);
        }
        else if (index == 1)
        {
            throughputGraph.gameObject.SetActive(false);
            defectsGraph.gameObject.SetActive(true);
        }
        else return;
    }
    void PopulateDropdownList()
    {
        dropdown.AddOptions(dropdownOptions);
    }

    WaitForSeconds waitForSeconds = new WaitForSeconds(10f);//maybe should be longer?
    IEnumerator UpdateValues()
    {
        throughPutOverTime.text =  AnalyticsData.Instance.avg_throughPutOverTime.ToString();//casting to int just for the aesthetics
        throughPutOverCar.text = ((int)AnalyticsData.Instance.avg_throughPutOverCar).ToString();
        paintAmount.text = ((int)AnalyticsData.Instance.avg_paintAmount).ToString();
        energyConsumption.text = ((int)AnalyticsData.Instance.avg_energyConsumption).ToString();
        workerUtilization.text = AnalyticsData.Instance.avg_workerUtilization.ToString();

        totalDefects.text = ((int)AnalyticsData.Instance.avg_totalDefects).ToString();
        majorDefects.text = ((int)AnalyticsData.Instance.avg_majorDefects).ToString();
        minorDefects.text = ((int)AnalyticsData.Instance.avg_minorDefects).ToString();

        yield return waitForSeconds;
    }
}
