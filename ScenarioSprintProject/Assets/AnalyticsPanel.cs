using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    // Start is called before the first frame update
    void Start()
    {
        PopulateDropdownList();
        StartCoroutine("UpdateValues");
    }

    public void DropdownIndexChanged(int index)
    {
        if (index == 0)
        {
            Debug.Log("showing throughput");//TODO: replace this accordingly with graph
        }
        else if (index == 1)
        {
            Debug.Log("showing defects");//TODO: replace this accordingly with graph
        }
        else return;
    }
    void PopulateDropdownList()
    {
        dropdown.AddOptions(dropdownOptions);
    }

    WaitForSeconds waitForSeconds = new WaitForSeconds(5f);//maybe should be longer?
    IEnumerator UpdateValues()
    {
        throughPutOverTime.text = AnalyticsData.Instance.throughPutOverTime.ToString();
        throughPutOverCar.text = AnalyticsData.Instance.throughPutOverCar.ToString();
        paintAmount.text = AnalyticsData.Instance.paintAmount.ToString();
        energyConsumption.text = AnalyticsData.Instance.energyConsumption.ToString();
        workerUtilization.text = AnalyticsData.Instance.workerUtilization.ToString();

        totalDefects.text = AnalyticsData.Instance.totalDefects.ToString();
        majorDefects.text = AnalyticsData.Instance.majorDefects.ToString();
        minorDefects.text = AnalyticsData.Instance.minorDefects.ToString();

        yield return waitForSeconds;
    }
}
