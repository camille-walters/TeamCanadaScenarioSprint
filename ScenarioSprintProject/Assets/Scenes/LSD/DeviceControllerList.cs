using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.DigitalTwins.Live.Sdk.Models;
using Unity.DigitalTwins.Live.Sdk.Samples.Services.Controllers;
using Unity.DigitalTwins.Live.Sdk.Services.Interfaces;
using UnityEngine;
using Unity.DigitalTwins.Live.Sdk.Signals;

public class DeviceControllerList : MonoBehaviour
{
    IDeviceService m_deviceService;
    [SerializeField]
    ServicesController m_ServicesController;
    
    ITelemetryHistoryService m_telemetryHistoryService;

    void Start()
    {
        m_deviceService = m_ServicesController.DeviceService;
        
        m_telemetryHistoryService = m_ServicesController.TelemetryHistoryService; m_ServicesController.signalBus.Subscribe<DeviceTelemetriesReceivedSignal>(OnDeviceTelemetriesReceived);

        const float deviceRetrievalDelay = 5.0f;
        Invoke(nameof(GetAllDevices), deviceRetrievalDelay);
    }
    
    void OnDeviceTelemetriesReceived(DeviceTelemetriesReceivedSignal signal)
    {
        string output = $"Telemetry received for device ID: {signal.DeviceTelemetries.Device.Id}";
        output = signal.DeviceTelemetries.Telemetries.Aggregate(output,
            (current, telemetry) => $"{current}{Environment.NewLine}{telemetry.Key}: {telemetry.Value} @ {telemetry.Timestamp}");
        Debug.Log($"YAY! {output}");
    }
    
    public async Task GetAllDevices()
    {
        var liveDevices = await m_deviceService.GetDevicesAsync();

        LiveDevice[] devices = liveDevices as LiveDevice[] ?? liveDevices.ToArray();
        string deviceIds = string.Join(Environment.NewLine,
            devices.Select(device => $"Source ID: {device.Device.IotSourceId} -> ID: {device.Device.Id}"));
        string message = $"There are {devices.Length} devices in the Facility.{Environment.NewLine}{deviceIds}";
        Debug.Log($"YAY! {message}");
        
        await m_telemetryHistoryService.SubscribeToDeviceTelemetriesAsync(devices.Select(device => device.Device.Id));
    }
}
