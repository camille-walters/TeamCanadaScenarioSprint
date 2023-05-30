using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace DataVisualizer.Editors
{
    public class ChartEditorCommon  
    {
        public static IEnumerable<Type> GetDerivedTypes<T>()
        {
            Type t = typeof(T);
            Assembly assembly = t.Assembly;
            return assembly.GetTypes().Where(x => x.IsAbstract == false && t.IsAssignableFrom(x));
        }

        public static void SupportLog(string log)
        {
            Debug.LogError(log + ", please contact support@prosourcelabs.com");
        }

        internal static bool IsAlphaNum(string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;

            for (int i = 0; i < str.Length; i++)
            {
                if (!(char.IsLetter(str[i])) && (!(char.IsNumber(str[i]))) && str[i] != ' ')
                    return false;
            }

            return true;
        }

        static GUIStyle mSplitter = null;
        internal static void Splitter()
        {
            if (mSplitter == null)
            {
                mSplitter = new GUIStyle();
                mSplitter.normal.background = EditorGUIUtility.whiteTexture;
                mSplitter.stretchWidth = true;
                mSplitter.margin = new RectOffset(0, 0, 7, 7);
            }
            Rect position = GUILayoutUtility.GetRect(GUIContent.none, mSplitter, GUILayout.Height(1f));
            if (Event.current.type == EventType.Repaint)
            {
                Color restoreColor = GUI.color;
                GUI.color = new Color(0.5f, 0.5f, 0.5f);
                mSplitter.Draw(position, false, false, false, false);
                GUI.color = restoreColor;
            }
        }

        internal static void DestoryInnerObject(GameObject obj, UnityEngine.Object parent)
        {
            GameObject.DestroyImmediate(obj);
            //EditorUtility.SetDirty(parent);
            //AssetDatabase.SaveAssets();
            //AssetDatabase.Refresh();
        }

        //  internal static UnityEngine.Object CreateObject(Type t, string name, UnityEngine.Object parent)
        //  {

        //   UnityEngine.Object.
        //}

        internal static T CreateInnerObject<T>(Type t, string name, GameObject parent) where T : MonoBehaviour
        {
            GameObject obj = new GameObject(name, t);
            obj.transform.SetParent(parent.transform);
            T res = obj.GetComponent(t) as T;
            //obj.hideFlags = HideFlags.HideInHierarchy;
            return res;
        }

        internal static void ClearNullElements(SerializedProperty array)
        {
            for (int i = array.arraySize - 1; i >= 0; i--)
            {
                if (array.GetArrayElementAtIndex(i).objectReferenceValue == null)
                    array.DeleteArrayElementAtIndex(i);
            }
        }

        internal static bool HasAttributeOfType(Type type, string fieldName, Type attributeType)
        {
            FieldInfo inf = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (inf == null)
                return false;
            object[] attrb = inf.GetCustomAttributes(attributeType, true);
            if (attrb == null)
                return false;
            return attrb.Length > 0;
        }
    }
}
