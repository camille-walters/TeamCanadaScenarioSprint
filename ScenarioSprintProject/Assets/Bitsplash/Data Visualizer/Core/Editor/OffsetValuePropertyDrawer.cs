using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace DataVisualizer.Editors
{
    [CustomPropertyDrawer(typeof(OffsetValue))]
    class OffsetValuePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var pixProp = property.FindPropertyRelative("pixels");
            var pcProp = property.FindPropertyRelative("percent");
            var rect = EditorGUI.PrefixLabel(position, new GUIContent(property.displayName));
            
            pixProp.doubleValue = EditorGUI.DoubleField(new Rect(rect.x, rect.y, rect.width * 0.4f, rect.height), pixProp.doubleValue);
            GUI.Label(new Rect(rect.x + rect.width * 0.4f, rect.y, rect.width * 0.1f, rect.height), "px");
            pcProp.doubleValue = EditorGUI.DoubleField(new Rect(rect.x + rect.width * 0.5f, rect.y, rect.width * 0.4f, rect.height), pcProp.doubleValue);
            GUI.Label(new Rect(rect.x + rect.width * 0.9f, rect.y, rect.width * 0.1f, rect.height), "%");
            EditorGUI.EndProperty();
        }
    }
}
