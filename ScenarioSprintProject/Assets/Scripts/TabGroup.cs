using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabGroup : MonoBehaviour
{
    public List<TabClick> tabButtons;
    public Color tabIdle;
    public Color tabHover;
    public Color tabActive;

    public List<GameObject> objectsToSwap;
    // Start is called before the first frame update
    public void Subscribe(TabClick button)
    {
        if(tabButtons == null)
        {
            tabButtons = new List<TabClick>();
        }

        tabButtons.Add(button);
    }

    public void OnTabEnter(TabClick button)
    {
        Debug.Log("here");
        ResetTabs();
        button.background.color = tabHover;
    }

    public void OnTabExit(TabClick button)
    {
        ResetTabs();
        button.background.color = tabIdle;
    }

    public void OnTabSelected(TabClick button)
    {
        ResetTabs();
        button.background.color = tabActive;
        int index = button.transform.GetSiblingIndex();
        for (int i =0; i<objectsToSwap.Count; i++)
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

    public void ResetTabs()
    {
        Debug.Log("resetting");
        foreach(TabClick button in tabButtons)
        {
            button.background.color = tabIdle;
        }
    }
}
