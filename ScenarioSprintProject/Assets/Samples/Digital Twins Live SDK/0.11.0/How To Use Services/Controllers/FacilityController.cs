using System;
using Unity.DigitalTwins.Live.Sdk.Samples.Services.Models;
using Unity.DigitalTwins.Live.Sdk.Services.Interfaces;
using UnityEngine;

namespace Unity.DigitalTwins.Live.Sdk.Samples.Services.Controllers
{
    public class FacilityController : MonoBehaviour
    {
        IFacilityService m_FacilityService;

        [SerializeField]
        ServicesController m_ServicesController;

        [SerializeField]
        FacilityServiceResult m_Result;

        public void GetResults()
        {
            var facilityInfo = m_FacilityService.GetFacilityInfo();
            m_Result.IsLoggedIn = facilityInfo.IsLoggedIn;
            m_Result.IsConnectedToMessagingService = facilityInfo.IsConnectedToMessagingService;
            m_Result.WorkspaceId = facilityInfo.WorkspaceId;
            m_Result.FacilityId = facilityInfo.FacilityId;
        }

        void Start()
        {
            m_FacilityService = m_ServicesController.FacilityService;
        }
    }
}
