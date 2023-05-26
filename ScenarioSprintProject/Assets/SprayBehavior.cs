using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SprayBehavior : MonoBehaviour
{
    ParticleSystem ps;
    // Start is called before the first frame update
    void Start()
    {
        ps = GetComponentInChildren<ParticleSystem>();
        if(ps == null)
        {
            Debug.Log("can't find ps");
        }
        ps.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
