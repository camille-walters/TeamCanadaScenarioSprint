using System;
using System.Collections.Generic;
using Unity.DigitalTwins.Live.Sdk.Models;
using UnityEngine;

namespace Unity.DigitalTwins.Live.Sdk.Samples.Services.Models
{
    [Serializable]
    public class LiveDeviceResultView
    {
        public string DeviceId;
        public List<GameObject> GameObjects;

        public LiveDeviceResultView(LiveDevice liveDevice)
        {
            DeviceId = liveDevice.Device.Id;
            GameObjects = liveDevice.GameObjects;
        }
    }
}
