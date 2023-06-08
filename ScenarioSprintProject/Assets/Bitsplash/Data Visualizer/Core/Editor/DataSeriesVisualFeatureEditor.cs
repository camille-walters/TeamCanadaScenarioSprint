using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using DataVisualizer;

namespace DataVisualizer.Editors
{
    [CustomEditor(typeof(DataSeriesVisualFeature),true)]
    class DataSeriesVisualFeatureEditor : Editor
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
