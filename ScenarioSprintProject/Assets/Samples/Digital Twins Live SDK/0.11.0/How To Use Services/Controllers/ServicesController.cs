using System;
using Unity.Cloud.Common;
using Unity.Cloud.Common.Runtime;
using Unity.Cloud.Identity;
using Unity.Cloud.Identity.Runtime;
using Unity.DigitalTwins.Live.Sdk.Abstractions;
using Unity.DigitalTwins.Live.Sdk.Implementations;
using Unity.DigitalTwins.Live.Sdk.Interfaces;
using Unity.DigitalTwins.Live.Sdk.Services.Implementations;
using Unity.DigitalTwins.Live.Sdk.Services.Interfaces;
using Unity.DigitalTwins.Live.Sdk.Settings;
using Unity.Cloud.Storage;
using UnityEngine;

namespace Unity.DigitalTwins.Live.Sdk.Samples.Services.Controllers
{
    public class ServicesController : MonoBehaviour
    {
        public IDeviceService DeviceService { get; private set; }
        public INotificationService NotificationService { get; private set; }
        public IFacilityService FacilityService { get; private set; }
        public ITelemetryHistoryService TelemetryHistoryService { get; private set; }
        public IDataAssociationsService DataAssociationsService { get; private set; }

        public EnvironmentSettings EnvironmentSettings;
        public CompositeAuthenticator Authenticator;
        public IUserInfoProvider UserInfoProvider;
        public ISceneProvider SceneProvider;
        
        public ISignalBus signalBus { get; private set; }

        IHttpClient m_HttpClient;
        MessagingClientWrapper m_MessagingClientWrapper;

        void Awake()
        {
            signalBus = new DebugSignalBus();
            m_HttpClient = new UnityHttpClient();

            var authenticationPlatformSupport = PlatformSupportFactory.GetAuthenticationPlatformSupport();
            var playerSettings = UnityCloudPlayerSettings.Instance;

            var compositeAuthenticatorSettings = new CompositeAuthenticatorSettingsBuilder(m_HttpClient, authenticationPlatformSupport)
                .AddDefaultPersonalAccessTokenProvider()
                .AddAuthenticator(new CommandLineAccessTokenProvider(authenticationPlatformSupport))
                .AddDefaultPkceAuthenticator(playerSettings, playerSettings)
                .Build();

            Authenticator = new CompositeAuthenticator(compositeAuthenticatorSettings);

            var serviceHttpClient = new ServiceHttpClient(
                m_HttpClient,
                Authenticator,
                playerSettings);

            var serviceHttpClientWrapper = new ServiceHttpClientWrapper(EnvironmentSettings, serviceHttpClient);
            var serviceHostConfiguration = ServiceHostConfigurationFactory.Create();
            UserInfoProvider = new UserInfoProvider(serviceHttpClient, serviceHostConfiguration);
            SceneProvider = new SceneProvider(serviceHttpClient, serviceHostConfiguration);
            var webSocketClient = WebSocketClientFactory.Create();
            var messagingClient = new LiveMessagingClient(webSocketClient, Authenticator);
            m_MessagingClientWrapper = new MessagingClientWrapper(messagingClient, EnvironmentSettings);
            FacilityService = new FacilityService(m_MessagingClientWrapper, EnvironmentSettings, signalBus);
            DeviceService = new DeviceService(EnvironmentSettings, serviceHttpClientWrapper, signalBus);
            NotificationService = new NotificationService(EnvironmentSettings, serviceHttpClientWrapper, m_MessagingClientWrapper, signalBus);
            TelemetryHistoryService = new TelemetryHistoryService(EnvironmentSettings, serviceHttpClientWrapper, m_MessagingClientWrapper, signalBus);
            DataAssociationsService = new DataAssociationsService(EnvironmentSettings, serviceHttpClientWrapper, signalBus);
        }

        void OnDestroy()
        {
            m_MessagingClientWrapper?.Dispose();
            DeviceService?.Dispose();
            Authenticator?.Dispose();
            m_HttpClient = null;
        }
    }
}
