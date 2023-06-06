using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabGroup : MonoBehaviour
{
    public List<TabClick> tabButtons;
    public List<GameObject> objectsToSwap;

    public void Subscribe(TabClick button)
    {
        if(tabButtons == null)
        {
            tabButtons = new List<TabClick>();
        }

        tabButtons.Add(button);
    }

    public void OnTabSelected(TabClick button)
    {
        int index = button.transform.GetSiblingIndex();
        for (int i = 0; i<objectsToSwap.Count; i++)
        {
            if (i == index)
            {
                objectsToSwap[i].SetActive(true);
            }
            else
            {
                objectsToSwap[i].SetActive(false);
            }
        }
    }
}
