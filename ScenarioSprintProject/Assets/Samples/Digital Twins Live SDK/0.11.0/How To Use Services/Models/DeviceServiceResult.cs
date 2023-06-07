using System;
using UnityEngine;

namespace Unity.DigitalTwins.Live.Sdk.Samples.Services.Models
{
    [Serializable]
    public class DeviceServiceResult
    {
        public LiveDeviceResultView[] LiveDevices;
        public LiveDeviceResultView[] SelectedLiveDevices;
    }
}
