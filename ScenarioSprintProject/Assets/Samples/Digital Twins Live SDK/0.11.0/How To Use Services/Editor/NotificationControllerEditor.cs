using System;
using System.Threading.Tasks;
using Unity.DigitalTwins.Live.Sdk.Samples.Services.Controllers;
using UnityEditor;
using UnityEngine;

namespace Unity.DigitalTwins.Live.Sdk.Samples.Services.Editor
{
    [CustomEditor(typeof(NotificationController))]
    public class NotificationControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var controller = (NotificationController)target;

            if (GUILayout.Button("Use Service"))
                Task.Run(() => controller.GetResultsAsync());
        }
    }
}
