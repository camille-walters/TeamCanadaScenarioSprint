using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

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
    public TMP_InputField sprayRadius;
    public TMP_InputField sprayAngle;
    public TMP_InputField sprayPressure;
    SprayBehavior[] sprayers;

    //This assumes that the number/instances of sprayers will not change while in runtime
    private void Awake()
    {
        sprayers = Resources.FindObjectsOfTypeAll<SprayBehavior>();
    }

    public void OnSprayRadiusChange()
    {
        foreach (var sprayer in sprayers)
        {
            ParticleSystem.ShapeModule shape = sprayer.ps.shape;
            shape.radius = UpdateSprayerInfo(sprayRadius);
        }
    }

    public void OnSprayAngleChange()
    {
        foreach (var sprayer in sprayers)
        {
            ParticleSystem.ShapeModule shape = sprayer.ps.shape;
            shape.radius = UpdateSprayerInfo(sprayAngle);
        }
    }

    public void OnSprayPressureChange()
    {
        foreach (var sprayer in sprayers)
        {
            ParticleSystem.EmissionModule emission = sprayer.ps.emission;
            emission.rateOverTime = UpdateSprayerInfo(sprayPressure);
        }
    }

    float UpdateSprayerInfo(TMP_InputField inputField)
    {
        float value = 0f;
        if (float.TryParse(inputField.text, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
        {
            return value;
        }
        return 0f;
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
