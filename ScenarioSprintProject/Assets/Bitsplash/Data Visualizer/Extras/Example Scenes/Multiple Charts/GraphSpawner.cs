using DataVisualizer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphSpawner : MonoBehaviour
{
    public int Count = 16;
    public CanvasDataSeriesChart Prefab;
    public RectTransform Parent;

    // Start is called before the first frame update
    void Start()
    {
        for(int i=0; i<Count; i++)
        {
            CanvasDataSeriesChart chart = GameObject.Instantiate(Prefab, Parent);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
