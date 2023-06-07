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

    private SprayBehavior[] sprayers;

    //This assumes that there are no sprayers added at runtime. 
    private void Awake()
    {
        sprayers = FindObjectsOfType<SprayBehavior>();
    }

    private void UpdateSprayers(Action<ParticleSystem> updateAction)
    {
        foreach (var sprayer in sprayers)
        {
            ParticleSystem spray = sprayer.gameObject.GetComponentInChildren<ParticleSystem>();
            if (spray != null)
            {
                updateAction(spray);
            }
        }
    }

    public void OnSprayRadiusChange()
    {
        UpdateSprayers((spray) =>
        {
            var shape = spray.shape;
            shape.radius = ParseFloatValue(sprayRadius);
        });
    }

    public void OnSprayAngleChange()
    {
        UpdateSprayers((spray) =>
        {
            var shape = spray.shape;
            shape.angle = ParseFloatValue(sprayAngle);
        });
    }

    public void OnSprayPressureChange()
    {
        UpdateSprayers((spray) =>
        {
            var emission = spray.emission;
            emission.rateOverTime = ParseFloatValue(sprayPressure);
        });
    }

    private float ParseFloatValue(TMP_InputField inputField)
    {
        float value = 0f;
        if (float.TryParse(inputField.text, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
        {
            return value;
        }
        Debug.Log("Could not parse value in " + inputField.name + ". Make sure input is a number.");
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
