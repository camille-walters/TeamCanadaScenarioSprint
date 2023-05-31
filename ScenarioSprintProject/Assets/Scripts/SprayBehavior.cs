using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SprayBehavior : MonoBehaviour
{
    ParticleSystem ps;
    //TODO in later PR: Expose parameters as needed here

    void Start()
    {
        ps = GetComponentInChildren<ParticleSystem>();
        if (ps == null)
        {
            Debug.Log("Can't find particle system for " + gameObject.name);
        }        
        
    }

    private void OnEnable()
    {
        //comment/uncomment this for testing purposes (will just start spraying when enabled)
        Play();
    }

    public void Play()
    {
        if (ps != null)
        {
            ps.Play();
        }
    }

    public void Stop()
    {
        if (ps != null)
        {
            ps.Stop();
        }            
    }

}
