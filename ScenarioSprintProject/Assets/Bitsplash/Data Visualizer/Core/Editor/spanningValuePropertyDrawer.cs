using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace DataVisualizer.Editors
{
    [CustomPropertyDrawer(typeof(SpanningValue))]
    public class spanningValuePropertyDrawer : PropertyDrawer
    {
        const float ComboSize = 80;

        string[] scrollOptions = new string[]
        {
             "Units","Seconds", "Minutes","Hours","Days"
        };
        double[] multipliers = new double[]
        {
            1.0,1.0,60.0,3600.0,86400.0
        };
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var valProp = property.FindPropertyRelative("Value");
            //var typeProp = property.FindPropertyRelative("Type");
            float height = EditorGUI.GetPropertyHeight(valProp);
            return height;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var valProp = property.FindPropertyRelative("Value");
            var typeProp = property.FindPropertyRelative("Type");
            Rect propRect = new Rect(position.x, position.y, position.width - ComboSize, EditorGUI.GetPropertyHeight(property));
            int chosenType = typeProp.intValue;
            
            valProp.doubleValue = valProp.doubleValue / multipliers[chosenType];
            EditorGUI.PropertyField(propRect, valProp,new GUIContent(property.displayName));
            valProp.doubleValue = valProp.doubleValue * multipliers[chosenType];
            //            Rect prefRect = EditorGUI.PrefixLabel(propRect, new GUIContent(property.displayName));
            //            double value = valProp.doubleValue;
            //            value /= multipliers[chosenType];
            //            value = EditorGUI.DoubleField(prefRect, value);
            //            value*= multipliers[chosenType];
            //               valProp.doubleValue = value;

            chosenType = EditorGUI.Popup(new Rect(position.width - ComboSize, position.y, ComboSize, EditorGUI.GetPropertyHeight(property)), chosenType, scrollOptions);
            typeProp.intValue = chosenType;
            EditorGUI.EndProperty();
        }
    }
}
