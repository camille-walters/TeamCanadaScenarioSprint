using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.Cloud.Identity;
using UnityEngine;

namespace Unity.DigitalTwins.Live.Sdk.Samples.Services.Controllers
{
    public class LoginController : MonoBehaviour
    {
        [SerializeField]
        ServicesController m_ServicesController;

        [SerializeField, TextArea]
        string m_AccessToken;

        async void Start()
        {
            m_ServicesController.Authenticator.AuthenticationStateChanged += OnAuthenticationStateChanged;
            await m_ServicesController.Authenticator.InitializeAsync();
            if (m_ServicesController.Authenticator.AuthenticationState == AuthenticationState.LoggedOut)
                await m_ServicesController.Authenticator.LoginAsync();
        }

        void OnAuthenticationStateChanged(AuthenticationState state)
        {
            switch (state)
            {
                case AuthenticationState.LoggedOut:
                    Debug.Log("User logged out.");
                    break;
                case AuthenticationState.LoggedIn:
                    Debug.Log("User logged in.");
                    Task.Run(FetchUserInfoAndAccessToken);
                    break;
                case AuthenticationState.AwaitingLogin:
                    Debug.Log("Awaiting login...");
                    break;
                case AuthenticationState.AwaitingLogout:
                    Debug.Log("Awaiting logout...");
                    break;
                case AuthenticationState.AwaitingInitialization:
                    Debug.Log("Awaiting initialization...");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state,
                        $"Unsupported authentication state: [{state}]");
            }
        }

        void OnDestroy()
        {
            m_ServicesController.Authenticator.AuthenticationStateChanged -= OnAuthenticationStateChanged;
        }

        async void FetchUserInfoAndAccessToken()
        {
            var userInfo = await m_ServicesController.UserInfoProvider.GetUserInfoAsync();
            var accessToken = await m_ServicesController.Authenticator.GetAccessTokenAsync();
            m_AccessToken = accessToken;

            var sceneList = await m_ServicesController.SceneProvider.ListScenesAsync();

            var scene = sceneList
                .FirstOrDefault(scene =>
                    scene.WorkspaceId.ToString().Equals(m_ServicesController.EnvironmentSettings.WorkspaceId) &&
                    scene.Id.ToString().Equals(m_ServicesController.EnvironmentSettings.FacilityId));

            if (scene is null)
            {
                Debug.LogWarning($"No {nameof(scene)} found in DT Portal for Facility: " +
                    $"{m_ServicesController.EnvironmentSettings.FacilityId} and Workspace: " +
                    $"{m_ServicesController.EnvironmentSettings.WorkspaceId}. If given Facility exists " +
                    "in LSD, only LSD services will work.");
            }

            await m_ServicesController.FacilityService.ConnectAsync(scene, userInfo, accessToken);
        }
    }
}
