using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class ControlPanel : MonoBehaviour
{
    // Camera 
    
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
    
    // Robots
    public TMP_InputField distanceFromCar;
    public TMP_InputField movementSpeed;

    public float defaultDistanceFromCar = 0.2f;
    public float defaultMovementSpeed = 100;
    
    
    // Workers
    public TMP_InputField numberOfWorkers;
    public TMP_InputField timeToFixMinorDefects;
    public TMP_InputField timeToFixMajorDefects;
    
    public int defaultNumberOfWorkers = 2;
    public float defaultTimeToFixMinorDefects = 1;
    public float defaultTimeToFixMajorDefects = 2;

    private SprayBehavior[] sprayers;
    RobotArmController[] m_RobotArmControllers;
    HybridInverseKinematicsNode[] m_HybridIks;
    SimulationManager m_SimulationManager;
    List<Camera> m_Views = new();
    int m_CurrentView = 0;
    
    WaitForSeconds m_WaitForSeconds = new WaitForSeconds(10f);
    public List<float> conveyorSpeedList = new();
    float m_CurrentConveyorSpeed;
    public List<float> sprayRadiusList = new();
    float m_CurrentSprayRadius;
    public List<float> sprayAngleList = new();
    float m_CurrentSprayAngle;
    public List<float> sprayPressureList = new();
    float m_CurrentSprayPressure;
    public List<float> distanceFromCarList = new();
    float m_CurrentDistanceFromCar;
    public List<float> movementSpeedList = new();
    float m_CurrentMovementSpeed;
    public List<int> numberOfWorkersList = new();
    int m_CurrentNumberOfWorkers;
    public List<float> timeToFixMinorDefectsList = new();
    float m_CurrentTimeToFixMinorDefects;
    public List<float> timeToFixMajorDefectsList = new();
    float m_CurrentTimeToFixMajorDefects;
    


    private void Awake()
    {
        sprayers = FindObjectsOfType<SprayBehavior>();
        m_RobotArmControllers = FindObjectsOfType<RobotArmController>();
        m_HybridIks = FindObjectsOfType<HybridInverseKinematicsNode>();
        
        m_SimulationManager = FindObjectOfType<SimulationManager>();
        m_Views = m_SimulationManager.GetCameraViews();
        
        LoadInputFieldValues();
        StartCoroutine(AddValuesToList());
    }

    private void OnDisable()
    {
        SaveInputFieldValues();
    }

    public void SetTimeInterval(float interval)
    {
        m_WaitForSeconds = new WaitForSeconds(interval);
    }
    
    IEnumerator AddValuesToList()
    {
        while (true)
        {
            conveyorSpeedList.Add(m_CurrentConveyorSpeed);
            sprayRadiusList.Add(m_CurrentSprayRadius);
            sprayAngleList.Add(m_CurrentSprayAngle);
            sprayPressureList.Add(m_CurrentSprayPressure);
            distanceFromCarList.Add(m_CurrentDistanceFromCar);
            movementSpeedList.Add(m_CurrentMovementSpeed);
            numberOfWorkersList.Add(m_CurrentNumberOfWorkers);
            timeToFixMinorDefectsList.Add(m_CurrentTimeToFixMinorDefects);
            timeToFixMajorDefectsList.Add(m_CurrentTimeToFixMajorDefects);

            yield return m_WaitForSeconds;
        }        
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
        
        // Robots
        distanceFromCar.text = PlayerPrefs.HasKey("distanceFromCar") ? PlayerPrefs.GetFloat("distanceFromCar").ToString() : defaultDistanceFromCar.ToString();
        OnDistanceFromCarChange();
        movementSpeed.text = PlayerPrefs.HasKey("movementSpeed") ? PlayerPrefs.GetFloat("movementSpeed").ToString() : defaultMovementSpeed.ToString();
        OnMovementSpeedChange();
        
        // Workers
        numberOfWorkers.text = PlayerPrefs.HasKey("numberOfWorkers") ? PlayerPrefs.GetFloat("numberOfWorkers").ToString() : defaultNumberOfWorkers.ToString();
        OnNumberOfWorkersChange();
        timeToFixMinorDefects.text = PlayerPrefs.HasKey("timeToFixMinorDefects") ? PlayerPrefs.GetFloat("timeToFixMinorDefects").ToString() : defaultTimeToFixMinorDefects.ToString();
        OnTimeToFixMinorChange();
        timeToFixMajorDefects.text = PlayerPrefs.HasKey("timeToFixMajorDefects") ? PlayerPrefs.GetFloat("timeToFixMajorDefects").ToString() : defaultTimeToFixMajorDefects.ToString();
        OnTimeToFixMajorChange();
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
        
        // Robots
        PlayerPrefs.SetFloat("distanceFromCar", ParseFloatValue(distanceFromCar));
        PlayerPrefs.SetFloat("movementSpeed", ParseFloatValue(movementSpeed));
        
        // Workers
        PlayerPrefs.SetFloat("numberOfWorkers", ParseFloatValue(numberOfWorkers));
        PlayerPrefs.SetFloat("timeToFixMinorDefects", ParseFloatValue(timeToFixMinorDefects));
        PlayerPrefs.SetFloat("timeToFixMajorDefects", ParseFloatValue(timeToFixMajorDefects));
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

    void ChangeCameraView()
    {
        m_Views[m_CurrentView].enabled = true;
        m_Views[m_CurrentView].gameObject.SetActive(true);

        for (var i = 0; i < m_Views.Count; ++i)
        {
            if (i == m_CurrentView)
                continue;
            
            
            m_Views[i].enabled = false;
            m_Views[i].gameObject.SetActive(false);
        }
    }

    public void OnPaintingRoomView()
    {
        m_CurrentView = 0;
        ChangeCameraView();
    }

    public void OnBufferRoomView()
    {
        m_CurrentView = 1;
        ChangeCameraView();
    }

    public void OnCVRoomView()
    {
        m_CurrentView = 2;
        ChangeCameraView();
    }

    public void OnQARoomView()
    {
        m_CurrentView = 3;
        ChangeCameraView();
    }

    #endregion

    #region Conveyance
    public void OnConveyorSpeedChange()
    {
        if (conveyorSpeed.text == "")
        {
            conveyorSpeed.text = defaultConveyorSpeed.ToString();
        }

        m_SimulationManager.conveyorSpeedFactor = ParseFloatValue(conveyorSpeed);
        m_CurrentConveyorSpeed = m_SimulationManager.conveyorSpeedFactor;
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
            m_CurrentSprayRadius = shape.radius;
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
            m_CurrentSprayAngle = shape.angle;
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
            m_CurrentSprayPressure = ParseFloatValue(sprayPressure);
        });
    }

    #endregion

    #region Robots
    public void OnDistanceFromCarChange()
    {
        if (distanceFromCar.text == "")
        {
            distanceFromCar.text = defaultDistanceFromCar.ToString();
        }
        
        foreach (var robotArm in m_RobotArmControllers)
        {
            robotArm.distanceFromSurface = ParseFloatValue(distanceFromCar);
            m_CurrentDistanceFromCar = robotArm.distanceFromSurface;
        }
    }
    public void OnMovementSpeedChange()
    {
        if (movementSpeed.text == "")
        {
            movementSpeed.text = defaultMovementSpeed.ToString();
        }
        
        foreach (var hybridIk in m_HybridIks)
        {
            hybridIk.jointAngularAcceleration = ParseFloatValue(movementSpeed);
            m_CurrentMovementSpeed = hybridIk.jointAngularAcceleration;
        }
    }
    #endregion

    #region Workers
    public void OnNumberOfWorkersChange()
    {
        if (numberOfWorkers.text == "")
        {
            numberOfWorkers.text = defaultNumberOfWorkers.ToString();
        }

        m_SimulationManager.numberOfOperators = (int) ParseFloatValue(numberOfWorkers);
        m_CurrentNumberOfWorkers = m_SimulationManager.numberOfOperators;
    }

    public void OnTimeToFixMinorChange()
    {
        if (timeToFixMinorDefects.text == "")
        {
            timeToFixMinorDefects.text = defaultTimeToFixMinorDefects.ToString();
        }

        m_SimulationManager.fixingTimeForMinorDefects = ParseFloatValue(timeToFixMinorDefects);
        m_CurrentTimeToFixMinorDefects = m_SimulationManager.fixingTimeForMinorDefects;
    }

    public void OnTimeToFixMajorChange()
    {
        if (timeToFixMajorDefects.text == "")
        {
            timeToFixMajorDefects.text = defaultTimeToFixMajorDefects.ToString();
        }

        m_SimulationManager.fixingTimeForMajorDefects = ParseFloatValue(timeToFixMajorDefects);
        m_CurrentTimeToFixMajorDefects = m_SimulationManager.fixingTimeForMajorDefects;
    }
    #endregion
}
