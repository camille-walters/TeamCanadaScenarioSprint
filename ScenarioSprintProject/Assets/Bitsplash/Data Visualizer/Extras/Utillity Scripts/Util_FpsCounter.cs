using UnityEngine;
using System.Collections;
using System.Diagnostics;
using UnityEngine.UI;

public class Util_FpsCounter : MonoBehaviour
{


    string text = "";
    double count = 0;
    double time = 0;
    public float TimeSpan = 0.33f;
    private void Start()
    {

    }

    void Update()
    {
        count++;
        time += Time.deltaTime;
        if (time >= TimeSpan)
        {
            time -= TimeSpan;
            double fps = (count/ TimeSpan);
            text = string.Format("({0:0.} fps)", fps);
            count = 0;
            var uiText = GetComponent<Text>();
            if(uiText != null)
                uiText.text = text;
        }
    }
}