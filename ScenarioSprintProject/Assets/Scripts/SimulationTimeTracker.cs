using System;
using UnityEngine;
public class SimulationTimeTracker : MonoBehaviour
{
    public int minutesPassed = 0;
    public float timeLeftForAMinute = 60;
    SimulationManager m_SimulationManager;
    void Start()
    {
        m_SimulationManager = this.gameObject.GetComponent<SimulationManager>();
    }
    void Update()
    {
        timeLeftForAMinute -= Time.deltaTime;
        if ( timeLeftForAMinute < 0 )
        {
            minutesPassed += 1;
            Debug.Log($"{minutesPassed} minutes completed");
            m_SimulationManager.UpdateThroughputAfterTimeChange();
            timeLeftForAMinute = 60;
        }
    }
}
