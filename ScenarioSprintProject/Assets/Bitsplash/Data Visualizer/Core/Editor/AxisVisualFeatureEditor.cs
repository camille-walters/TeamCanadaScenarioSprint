using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
namespace DataVisualizer.Editors
{
    [CustomEditor(typeof(AxisVisualFeature), true)]
    class AxisVisualFeatureEditor : Editor
    {
        private static readonly string[] mToExclude = new string[] { "m_Script" };
        public override void OnInspectorGUI()  
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, mToExclude);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
