using System;
using System.Threading.Tasks;
using Unity.DigitalTwins.Live.Sdk.Samples.Services.Controllers;
using UnityEditor;
using UnityEngine;

namespace Unity.DigitalTwins.Live.Sdk.Samples.Services.Editor
{
    [CustomEditor(typeof(FacilityController))]
    public class FacilityControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var controller = (FacilityController)target;

            if (GUILayout.Button("Use Service"))
                Task.Run(() => controller.GetResults());
        }
    }
}
