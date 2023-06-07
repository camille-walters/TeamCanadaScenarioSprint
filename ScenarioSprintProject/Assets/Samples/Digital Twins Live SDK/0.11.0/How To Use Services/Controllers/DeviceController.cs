using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.DigitalTwins.Live.Sdk.Samples.Services.Models;
using Unity.DigitalTwins.Live.Sdk.Services.Interfaces;
using UnityEngine;

namespace Unity.DigitalTwins.Live.Sdk.Samples.Services.Controllers
{
    public class DeviceController : MonoBehaviour
    {
        IDeviceService m_DeviceService;

        [SerializeField]
        ServicesController m_ServicesController;
        [SerializeField]
        GameObject[] m_DeviceGameObjects;

        [SerializeField]
        DeviceServiceResult m_Result;

        public async Task GetResultsAsync()
        {
            var liveDevices = await m_DeviceService.GetDevicesAsync();
            var liveDevice = liveDevices.FirstOrDefault();

            if (liveDevice != null)
            {
                liveDevice.GameObjects?.AddRange(m_DeviceGameObjects);
                liveDevice.IsSelected = true;
            }

            m_Result.LiveDevices = m_DeviceService
                .GetCachedDevices()
                .Select(device => new LiveDeviceResultView(device))
                .ToArray();
            m_Result.SelectedLiveDevices = m_DeviceService
                .GetSelectedLiveDevices()
                .Select(device => new LiveDeviceResultView(device))
                .ToArray();
        }

        void Start()
        {
            m_DeviceService = m_ServicesController.DeviceService;
        }
    }
}
