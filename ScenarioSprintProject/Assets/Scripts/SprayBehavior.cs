using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SprayBehavior : MonoBehaviour
{
    ParticleSystem ps;
    public bool playOnStart = false;
    public bool playOnEnable = false;

    void Start()
    {
        ps = GetComponentInChildren<ParticleSystem>();
        if (ps == null)
        {
            Debug.Log("Can't find particle system for " + gameObject.name);
        }

        if (playOnStart)
            Play();
    }

    private void OnEnable()
    {
        if (playOnEnable)
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
