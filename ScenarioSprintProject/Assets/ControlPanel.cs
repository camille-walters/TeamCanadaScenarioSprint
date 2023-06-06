using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using System;

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
    public InputField sprayRadius;
    public InputField sprayAngle;
    public InputField sprayPressure;
    SprayBehavior[] sprayers;
    public void OnSprayRadiusChange()
    {
        sprayers = Resources.FindObjectsOfTypeAll<SprayBehavior>();
        foreach (var sprayer in sprayers)
        {
            ParticleSystem.ShapeModule shape = sprayer.ps.shape;
            shape.radius = UpdateSprayerInfo(sprayRadius);
        }
    }

    public void OnSprayAngleChange()
    {
        sprayers = Resources.FindObjectsOfTypeAll<SprayBehavior>();
        foreach (var sprayer in sprayers)
        {
            ParticleSystem.ShapeModule shape = sprayer.ps.shape;
            shape.radius = UpdateSprayerInfo(sprayAngle);
        }
    }

    public void OnSprayPressureChange()
    {
        sprayers = Resources.FindObjectsOfTypeAll<SprayBehavior>();
        foreach (var sprayer in sprayers)
        {
            ParticleSystem.EmissionModule emission = sprayer.ps.emission;
            emission.rateOverTime = UpdateSprayerInfo(sprayPressure);
        }
    }

    float UpdateSprayerInfo(InputField inputField)
    {
        return float.Parse(inputField.text, CultureInfo.InvariantCulture.NumberFormat);

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
