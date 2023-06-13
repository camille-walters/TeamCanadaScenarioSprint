using System;
 using System.Collections.Generic;
 using System.Linq;
 using System.Threading.Tasks;
 using Unity.DigitalTwins.Live.Sdk.Models;
using Unity.DigitalTwins.Live.Sdk.Samples.Services.Controllers;
using Unity.DigitalTwins.Live.Sdk.Services.Interfaces;
 using UnityEngine;

 public class TempratureHistoryController : MonoBehaviour
 {
     ITelemetryHistoryService m_telemetryHistoryService;
     IDeviceService m_deviceService;

     [SerializeField]
     ServicesController m_ServicesController;
     [SerializeField]
     string m_TelemetryKey = "Temperature";
     [SerializeField]
     private int m_TelemetryHistoryStepResolutionInSeconds = 5;
     [SerializeField]
     private int m_TelemetryHistoryIntervalInMinutes = 1;
     [SerializeField]
     float m_TelemetryHistoryRetrievalDelayInSeconds = 5.0f;
     [SerializeField]
     float m_TelemetryHistoryRetrievalTimeoutInSeconds = 60.0f;

     private void Start()
     {
         m_telemetryHistoryService = m_ServicesController.TelemetryHistoryService;
         m_deviceService = m_ServicesController.DeviceService;

         InvokeRepeating(nameof(GetTelemetryHistory),
             m_TelemetryHistoryRetrievalDelayInSeconds,
             m_TelemetryHistoryRetrievalTimeoutInSeconds);
     }

     private async Task GetTelemetryHistory()
     {
         IEnumerable<LiveDevice> liveDevices = await m_deviceService.GetDevicesAsync();
         LiveDevice liveDevice = liveDevices.FirstOrDefault();
         if (liveDevice is null)
         {
             Debug.LogError(
                 $"There must be a {nameof(liveDevice)} to successfully receive Telemetry.");
             return;
         }

         DateTimeOffset endTime = DateTimeOffset.UtcNow;
         DateTimeOffset startTime = endTime.AddMinutes(-m_TelemetryHistoryIntervalInMinutes);
         IEnumerable<TelemetryHistory> telemetryHistories =
             await m_telemetryHistoryService.GetHistoryAsync(
                 new[] { liveDevice.Device.Id },
                 m_TelemetryKey,
                 startTime,
                 endTime,
                 m_TelemetryHistoryStepResolutionInSeconds);

         foreach (var telemetryHistory in telemetryHistories)
         {
             string output =
                 $"Telemetry history query successful for device ID: {telemetryHistory.Device.Id}";
             foreach (var telemetry in telemetryHistory.Telemetries)
             {
                 output += $"\n{telemetry.Key} ({telemetry.Count()} values found):";
                 foreach (var value in telemetry)
                 {
                     output += $"\n{value.Value} @ {value.Timestamp}";
                 }
             }
             Debug.Log(output);
         }
     }
 }