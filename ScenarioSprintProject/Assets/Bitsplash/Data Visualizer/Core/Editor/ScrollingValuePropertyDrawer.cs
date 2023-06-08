using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace DataVisualizer.Editors
{
    [CustomPropertyDrawer(typeof(ScrollingValue))]
    public class ScrollingValuePropertyDrawer : PropertyDrawer
    {
        const float ComboSize = 80;
        const float warningSize = 50f;
        string[] scrollOptions = new string[]
        {
             "Units", "DateTime"
        };

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var valProp = property.FindPropertyRelative("Value");
            var typeProp = property.FindPropertyRelative("Type");
            float height = EditorGUI.GetPropertyHeight(valProp);
            if(typeProp.intValue == 1)
            {
                return height * 2;
            }
            return height;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var valProp = property.FindPropertyRelative("Value");
            var typeProp = property.FindPropertyRelative("Type");
            Rect propRect = new Rect(position.x, position.y, position.width - ComboSize, EditorGUI.GetPropertyHeight(property));
            int chosenType = typeProp.intValue;
            if (chosenType == 0)
            {
                EditorGUI.PropertyField(propRect, valProp, new GUIContent(property.displayName));
            }
            else
            {
                double val = valProp.doubleValue;
                DateTime dateTime = DateUtility.ValueToDate(val);
                var prefRect = EditorGUI.PrefixLabel(propRect, new GUIContent(property.displayName));
                Rect tmpRect = prefRect;
                tmpRect.width = EditorStyles.label.CalcSize(new GUIContent("M/D/Y:")).x;
                EditorGUI.LabelField(tmpRect, "M/D/Y:");
                tmpRect.x += tmpRect.width;
                tmpRect.width = EditorStyles.numberField.CalcSize(new GUIContent("XX")).x;
                int M = EditorGUI.IntField(tmpRect, dateTime.Month);
                tmpRect.x += tmpRect.width;
                tmpRect.width = EditorStyles.numberField.CalcSize(new GUIContent("XX")).x;
                int D = EditorGUI.IntField(tmpRect, dateTime.Day);
                tmpRect.x += tmpRect.width;
                tmpRect.width = EditorStyles.numberField.CalcSize(new GUIContent("XXXX")).x;
                int Y = EditorGUI.IntField(tmpRect, dateTime.Year);
                tmpRect.x += tmpRect.width;

                tmpRect = prefRect;
                tmpRect.y += tmpRect.height;
                tmpRect.width = EditorStyles.label.CalcSize(new GUIContent("hh:mm:ss")).x;
                EditorGUI.LabelField(tmpRect, "hh:mm:ss");
                tmpRect.x += tmpRect.width;
                tmpRect.width = EditorStyles.numberField.CalcSize(new GUIContent("XX")).x;
                int h = EditorGUI.IntField(tmpRect, dateTime.Hour);
                tmpRect.x += tmpRect.width;
                tmpRect.width = EditorStyles.numberField.CalcSize(new GUIContent("XX")).x;
                int m = EditorGUI.IntField(tmpRect, dateTime.Minute);
                tmpRect.x += tmpRect.width;
                tmpRect.width = EditorStyles.numberField.CalcSize(new GUIContent("XX")).x;
                int s = EditorGUI.IntField(tmpRect, dateTime.Second);
                tmpRect.x += tmpRect.width;
                if (h < 0)
                    h = 0;
                if (h > 23)
                    h = 23;
                if (m < 0)
                    m = 0;
                if (m > 59)
                    m = 59;
                if (s < 0)
                    s = 0;
                if (s > 59)
                    s = 59;
                if (M > 12)
                    M = 12;
                if (M < 0)
                    M = 0;
                int daysInMonth = DateTime.DaysInMonth(Y, M);
                if (D > daysInMonth)
                    D = daysInMonth;
                if (D < 1)
                    D = 1;
                valProp.doubleValue = DateUtility.DateToValue(new DateTime(Y, M, D,h,m,s));

            }
            chosenType = EditorGUI.Popup(new Rect(position.width - ComboSize, position.y, ComboSize, EditorGUI.GetPropertyHeight(property)), chosenType, scrollOptions);
            typeProp.intValue = chosenType;
            EditorGUI.EndProperty();
        }
    }
}