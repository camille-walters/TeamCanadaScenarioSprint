using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace DataVisualizer.Editors
{
    [CustomPropertyDrawer(typeof(ChartSpecificMaterialAttribute))]
    public class MaterialPropertyDrawer :  PropertyDrawer
    {
        const float SpaceSize = 10f;
        const float warningSize = 50f;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = base.GetPropertyHeight(property, label);
            var refValue = property.objectReferenceValue as Material;
            if (refValue != null && refValue.shader != null)
            {
                if (refValue.shader.name != "DataVisualizer/Canvas/Solid")
                {
                    height += warningSize + SpaceSize;
                }
            }
            return height;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(new Rect(position.x,position.y,position.width, base.GetPropertyHeight(property,label)), property,new GUIContent(property.displayName)); 
            var refValue = property.objectReferenceValue as Material;
            if(refValue != null && refValue.shader != null)
            {
                if (refValue.shader.name != "DataVisualizer/Canvas/Solid")
                    EditorGUI.HelpBox(new Rect(position.x, position.yMax - warningSize, position.width, warningSize),
                        "Shader is not \"DataVisualizer/Canvas/Solid\". This will cause a performance decrease", MessageType.Warning);
            }
            EditorGUI.EndProperty();
        }
    }
}
