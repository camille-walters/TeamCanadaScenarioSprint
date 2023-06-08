using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class ControlPanel : MonoBehaviour
{
    // Conveyor
    public TMP_InputField conveyorSpeed;

    public float defaultConveyorSpeed = 1;
    
    // Paint 
    public TMP_InputField sprayRadius;
    public TMP_InputField sprayAngle;
    public TMP_InputField sprayPressure;

    public float defaultSprayRadius = 0.1f;
    public int defaultSprayAngle = 30;
    public int defaultSprayPressure = 5000;

    private SprayBehavior[] sprayers;
    SimulationManager m_SimulationManager;


    private void Awake()
    {
        sprayers = FindObjectsOfType<SprayBehavior>();
        m_SimulationManager = FindObjectOfType<SimulationManager>();

        LoadInputFieldValues();

    }

    private void OnDisable()
    {
        SaveInputFieldValues();
    }

    //Add values to be persisted here
    private void LoadInputFieldValues()
    {
        // Conveyor
        conveyorSpeed.text = PlayerPrefs.HasKey("conveyorSpeed") ? PlayerPrefs.GetFloat("conveyorSpeed").ToString() : defaultConveyorSpeed.ToString();
        OnConveyorSpeedChange();
        
        // Paint
        sprayRadius.text = PlayerPrefs.HasKey("sprayRadius") ? PlayerPrefs.GetFloat("sprayRadius").ToString() : defaultSprayRadius.ToString();
        OnSprayRadiusChange();
        sprayAngle.text = PlayerPrefs.HasKey("sprayAngle;") ? PlayerPrefs.GetFloat("sprayAngle").ToString() : defaultSprayAngle.ToString();
        OnSprayAngleChange();
        sprayPressure.text = PlayerPrefs.HasKey("sprayPressure") ? PlayerPrefs.GetFloat("sprayPressure").ToString() : defaultSprayPressure.ToString();
        OnSprayPressureChange();
    }

    //Add values to be persisted here
    private void SaveInputFieldValues()
    {
        // Conveyor
        PlayerPrefs.SetFloat("conveyorSpeed", ParseFloatValue(conveyorSpeed));
        
        // Paint
        PlayerPrefs.SetFloat("sprayRadius", ParseFloatValue(sprayRadius));
        PlayerPrefs.SetFloat("sprayAngle", ParseFloatValue(sprayAngle));
        PlayerPrefs.SetFloat("sprayPressure", ParseFloatValue(sprayPressure));
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

    #region Cameras

    #endregion

    #region Conveyance
    public void OnConveyorSpeedChange()
    {
        if (conveyorSpeed.text == "")
        {
            conveyorSpeed.text = defaultConveyorSpeed.ToString();
        }

        m_SimulationManager.conveyorSpeedFactor = ParseFloatValue(conveyorSpeed);
    }
    #endregion

    #region Paint

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
        if (sprayRadius.text == "")
        {
            sprayRadius.text = defaultSprayRadius.ToString();
        }
        UpdateSprayers((spray) =>
        {
            var shape = spray.shape;
            shape.radius = ParseFloatValue(sprayRadius);
        });
    }

    public void OnSprayAngleChange()
    {
        if (sprayAngle.text == "")
        {
            sprayAngle.text = defaultSprayAngle.ToString();
        }
        UpdateSprayers((spray) =>
        {
            var shape = spray.shape;            
            shape.angle = ParseFloatValue(sprayAngle);      
        });
    }

    public void OnSprayPressureChange()
    {
        if(sprayPressure.text == "")
        {
            sprayPressure.text = defaultSprayPressure.ToString();
        }
        UpdateSprayers((spray) =>
        {
            var emission = spray.emission;
            emission.rateOverTime = ParseFloatValue(sprayPressure);
        });
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
