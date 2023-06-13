using System.Linq;
using TMPro;
using Unity.DigitalTwins.Live.Sdk.Models;
using Unity.DigitalTwins.Live.Sdk.Samples.Services.Controllers;
using Unity.DigitalTwins.Live.Sdk.Services.Interfaces;
using Unity.DigitalTwins.Live.Sdk.Signals;
using UnityEngine;

public class ThermostatController : MonoBehaviour
{
    ITelemetryHistoryService m_telemetryHistoryService;

    [SerializeField]
    ServicesController m_ServicesController;
    public string m_DeviceId;
    public string m_TelemetryName = "Temperature";

    public TMP_Text m_DisplayText;

    private void Start()
    {
        m_telemetryHistoryService = m_ServicesController.TelemetryHistoryService;
        m_ServicesController.signalBus.Subscribe<DeviceTelemetriesReceivedSignal>(
            OnDeviceTelemetriesReceived);

        if (!string.IsNullOrWhiteSpace(m_DeviceId))
        {
            m_telemetryHistoryService.SubscribeToDeviceTelemetriesAsync(new[] { m_DeviceId });
        }
        else
        {
            Debug.LogError(
                "NAAY! Unable to retrieve thermostat temperature as no Device ID was provided.");
        }
    }

    private void OnDeviceTelemetriesReceived(DeviceTelemetriesReceivedSignal signal)
    {
        if (signal.DeviceTelemetries.Device.Id.Equals(m_DeviceId))
        {
            Telemetry lastTelemetry = signal.DeviceTelemetries.Telemetries.LastOrDefault();
            if (lastTelemetry is not null && lastTelemetry.Key == m_TelemetryName)
            {
                double temperature = lastTelemetry.Value;
                m_DisplayText.text = $"{temperature:F1}\u00b0";
            }
        }
    }
}
