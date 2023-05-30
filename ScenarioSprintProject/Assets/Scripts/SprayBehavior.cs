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
        if(ps == null)
        {
            Debug.Log("Can't find particle system");
        }

        //comment/uncomment this for testing purposes (will just start spraying when scene starts)
        Play();
        
    }

    void Play()
    {
        ps?.Play();
    }

    private void Stop()
    {
        ps?.Stop();
    }

}
