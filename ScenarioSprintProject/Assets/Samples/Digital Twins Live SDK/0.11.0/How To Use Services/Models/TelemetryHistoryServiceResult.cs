using System;

namespace Unity.DigitalTwins.Live.Sdk.Samples.Services.Models
{
    [Serializable]
    public class TelemetryHistoryServiceResult
    {
        public string DeviceId;
        public int TelemetryHistoriesCount;
    }
}
