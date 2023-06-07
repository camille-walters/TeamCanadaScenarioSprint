using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.DigitalTwins.Live.Sdk.Samples.Services.Models;
using Unity.DigitalTwins.Live.Sdk.Services.Interfaces;
using UnityEngine;

namespace Unity.DigitalTwins.Live.Sdk.Samples.Services.Controllers
{
    public class TelemetryHistoryController : MonoBehaviour
    {
        ITelemetryHistoryService m_TelemetryHistoryService;
        IDeviceService m_DeviceService;

        [SerializeField]
        ServicesController m_ServicesController;
        [SerializeField]
        string m_TelemetryKey = "WindSpeed";

        [SerializeField]
        TelemetryHistoryServiceResult m_Result;

        public async Task GetResultsAsync()
        {
            var liveDevices = await m_DeviceService.GetDevicesAsync();
            var liveDevice = liveDevices.FirstOrDefault();

            if (liveDevice is null)
            {
                Debug.LogError($"There must be a {nameof(liveDevice)} to successfully receive Telemetry.");
                return;
            }

            m_Result.TelemetryHistoriesCount = 0;

            var endTime = DateTimeOffset.UtcNow;
            var startTime = endTime.AddMinutes(-10);
            var stepResolutionInSeconds = 1;
            var telemetryHistories = await m_TelemetryHistoryService.GetHistoryAsync(
                new[] { liveDevice.Device.Id },
                m_TelemetryKey,
                startTime,
                endTime,
                stepResolutionInSeconds);

            var telemetries = telemetryHistories
                .First()
                .Telemetries
                .First(group => group.Key.Equals(m_TelemetryKey))
                .ToList();

            m_Result.DeviceId = liveDevice.Device.Id;
            m_Result.TelemetryHistoriesCount = telemetries.Count;
        }

        void Start()
        {
            m_TelemetryHistoryService = m_ServicesController.TelemetryHistoryService;
            m_DeviceService = m_ServicesController.DeviceService;
        }
    }
}
