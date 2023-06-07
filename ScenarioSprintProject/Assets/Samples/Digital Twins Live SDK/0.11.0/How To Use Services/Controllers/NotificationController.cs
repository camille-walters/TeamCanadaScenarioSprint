using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.DigitalTwins.Live.Sdk.Samples.Services.Models;
using Unity.DigitalTwins.Live.Sdk.Services.Interfaces;
using Unity.DigitalTwins.Live.Shared.Models;
using UnityEngine;

namespace Unity.DigitalTwins.Live.Sdk.Samples.Services.Controllers
{
    public class NotificationController : MonoBehaviour
    {
        INotificationService m_NotificationService;
        IDeviceService m_DeviceService;

        [SerializeField]
        ServicesController m_ServicesController;

        [SerializeField]
        NotificationServiceResult m_Result;

        public async Task GetResultsAsync()
        {
            await SendNotificationAsync();

            m_Result.LatestNotifications = m_NotificationService
                .GetNotifications()
                .Select(notification => $"{notification.Timestamp}: {notification.Title}")
                .ToArray();
        }

        void Start()
        {
            m_NotificationService = m_ServicesController.NotificationService;
            m_DeviceService = m_ServicesController.DeviceService;
        }

        async Task SendNotificationAsync()
        {
            var liveDevices = await m_DeviceService.GetDevicesAsync();
            var liveDevice = liveDevices.FirstOrDefault();

            if (liveDevice is null || string.IsNullOrEmpty(liveDevice.Device.Id))
            {
                Debug.LogError($"There must be a {nameof(liveDevice.Device)} with correct id to successfully send a Notification.");
                return;
            }

            var notification = AnyNotificationRequestResource(new List<string>
            {
                liveDevice.Device.Id
            });

            await m_NotificationService.CreateNotificationsAsync(new List<NotificationRequestResource> { notification });
            Debug.Log("Notification created");
        }

        static NotificationRequestResource AnyNotificationRequestResource(IEnumerable<string> deviceIdList)
            => new()
            {
                Title = $"Samples Title: {Guid.NewGuid().ToString()}",
                Message = $"Samples Message: {Guid.NewGuid().ToString()}",
                DeviceIds = deviceIdList,
                Timestamp = DateTimeOffset.UtcNow,
                Severity = "LOW"
            };
    }
}
