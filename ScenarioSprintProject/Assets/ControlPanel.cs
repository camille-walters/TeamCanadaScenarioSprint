using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPanel : MonoBehaviour
{
    #region Cameras

    #endregion


    #region Conveyance
    public void OnConveyorSpeedChange()
    {
        Debug.Log("Test");
    }
    #endregion

    #region Paint
    public void OnSprayRadiusChange()
    {
        Debug.Log("Test");
    }

    public void OnSprayAngleChange()
    {
        Debug.Log("Test");
    }

    public void OnSprayPressureChange()
    {
        Debug.Log("Test");
    }
    #endregion

    #region Robots
    public void OnDistanceFromCarChange()
    {
        Debug.Log("Test");
    }
    public void OnMovementSpeedChange()
    {
        Debug.Log("Test");
    }
    #endregion

    #region Workers
    public void OnNumberOfWorkersChange()
    {
        Debug.Log("workerchange");
    }

    public void OnTimeToInspectChange()
    {
        Debug.Log("inspect change");
    }

    public void OnTimeToFixChange()
    {
        Debug.Log("Test");
    }
    #endregion
}
