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

    public TMP_Text totalDefects;
    public TMP_Text majorDefects;
    public TMP_Text minorDefects;


    // Start is called before the first frame update
    void Start()
    {
        PopulateDropdownList();
    }

    // Update is called once per frame
    void Update()
    {
        //probably doesnt need to be done in update
        totalDefects.text = AnalyticsData.Instance.totalDefects.ToString();
        majorDefects.text = AnalyticsData.Instance.majorDefects.ToString();
        minorDefects.text = AnalyticsData.Instance.minorDefects.ToString();
    }

    public void DropdownIndexChanged(int index)
    {
        if (index == 0)
        {
            Debug.Log("showing throughput");//TODO: replace this accordingly
        }
        else if (index == 1)
        {
            Debug.Log("showing defects");//TODO: replace accordingly
        }
        else return;
    }
    void PopulateDropdownList()
    {
        dropdown.AddOptions(dropdownOptions);
    }
}
