using DataVisualizer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialEventHandler : MonoBehaviour
{
    public CanvasEventInteraction Interaction;
    void Start()
    {
        Interaction.ClickEvent.AddListener(OnClick);
    }

    void OnClick(ChartClickEventArgs click)
    {
        Debug.Log(click.Category + "," + click.Index);
        Debug.Log(click.FormattedString);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
