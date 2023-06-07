using System;

namespace Unity.DigitalTwins.Live.Sdk.Samples.Services.Models
{
    [Serializable]
    public class FacilityServiceResult
    {
        public bool IsLoggedIn;
        public bool IsConnectedToMessagingService;
        public string WorkspaceId;
        public string FacilityId;
    }
}
