using UnityEngine;
using UnityEditor;
using DataVisualizer;
using System;
using System.Linq;
using System.Collections.Generic;

namespace DataVisualizer.Editors
{
    [CustomEditor(typeof(CanvasDataSeriesChart), true)]
    public class SeriesChartDataEditor : Editor
    {

        int mPopupSelectedIndex = 0;
        string[] mNames;
        int[] mOrderItems;
        private Dictionary<string, Editor> mCategoryEditors = new Dictionary<string, Editor>();
        string Renaming = null;
        string NewName = null;
        bool startRename = false;
        bool cancel = false;
        DataSeriesAxisEditor mAxisEditor = null;
        GUIStyle SectionNameStyle = new GUIStyle();
        int PropertySelection = 0;
        SerializedProperty findItemInArray(string name, SerializedProperty array)
        {
            int size = array.arraySize;
            for (int i = 0; i < size; i++)
            {
                SerializedProperty Serializedcategory = array.GetArrayElementAtIndex(i);
                var cat = Serializedcategory.objectReferenceValue as DataSeriesCategory;
                if (cat.Name == name)
                    return Serializedcategory;
            }
            return null;
        }

        void ClearUnusedObjects()
        {
            var data = serializedObject.FindProperty("mSeriesData");
            if (data == null)
            {
                ChartEditorCommon.SupportLog("editor error");
                return;
            }
            SerializedProperty innerItems = data.FindPropertyRelative("mSerielizedCategories");
            HashSet<GameObject> objects = new HashSet<GameObject>();
            int size = innerItems.arraySize;
            for (int i = 0; i < size; i++)
            {
                SerializedProperty Serializedcategory = innerItems.GetArrayElementAtIndex(i);
                DataSeriesCategory cat = Serializedcategory.objectReferenceValue as DataSeriesCategory;
                if (cat == null)
                    continue;
                cat.ClearUnusedObjects();
                objects.Add(cat.gameObject);
            }
            GameObject settings = GetSettingsObject();

            foreach (Transform t in settings.transform)
            {
                if (objects.Contains(t.gameObject))
                    continue;
                ChartCommon.SafeDestroy(t.gameObject);
            }
        }


        void DeleteCategory(string name)
        {
            var data = serializedObject.FindProperty("mSeriesData");
            if (data == null)
            {
                ChartEditorCommon.SupportLog("editor error");
                return;
            }
            SerializedProperty innerItems = data.FindPropertyRelative("mSerielizedCategories");
            int size = innerItems.arraySize;
            for (int i = 0; i < size; i++)
            {
                SerializedProperty Serializedcategory = innerItems.GetArrayElementAtIndex(i);
                var cat = Serializedcategory.objectReferenceValue as DataSeriesCategory;
                if (name == cat.Name)
                {
                    DataSeriesCategory cayObj = Serializedcategory.objectReferenceValue as DataSeriesCategory;
                    ChartEditorCommon.DestoryInnerObject(cayObj.gameObject, target);
                    Serializedcategory.objectReferenceValue = null;
                    innerItems.DeleteArrayElementAtIndex(i);
                    ChartEditorCommon.ClearNullElements(innerItems);
                    serializedObject.ApplyModifiedProperties();
                    return;
                }
            }
        }

        GameObject GetSettingsObject()
        {
            GameObject parent = ((CanvasDataSeriesChart)target).gameObject;
            Transform t = parent.transform.Find("Settings");
            if (t != null)
                return t.gameObject;
            //      Debug.LogError("Settings object is null");
            GameObject obj = new GameObject("Settings");
            obj.hideFlags = HideFlags.None;// HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            obj.transform.SetParent(parent.transform);
            return obj;
        }

        void AddNewCategory()
        {
            string defName = "New Category-";
            var data = serializedObject.FindProperty("mSeriesData");
            if (data == null)
            {
                ChartEditorCommon.SupportLog("editor error");
                return;
            }

            SerializedProperty innerItems = data.FindPropertyRelative("mSerielizedCategories");
            int size = innerItems.arraySize;
            int maxOrder = -1;
            for (int i = 0; i < size; i++)
            {
                SerializedProperty Serializedcategory = innerItems.GetArrayElementAtIndex(i);
                var searchcCat = Serializedcategory.objectReferenceValue as DataSeriesCategory;
                string name = searchcCat.Name;

                if (name.StartsWith(defName))
                {
                    name = name.Substring(defName.Length);
                    int res;
                    if (int.TryParse(name, out res))
                        maxOrder = Math.Max(maxOrder, res);
                }
            }

            ++maxOrder;
            string newName = defName + maxOrder;
            GameObject settings = GetSettingsObject();
            DataSeriesCategory cat = ChartEditorCommon.CreateInnerObject<DataSeriesCategory>(typeof(DataSeriesCategory), newName, settings);

            ((IPrivateSetName)cat).SetName(newName);
            cat.ViewOrder = size;
            innerItems.InsertArrayElementAtIndex(0);
            SerializedProperty newitem = innerItems.GetArrayElementAtIndex(0);
            newitem.objectReferenceValue = cat;
            serializedObject.ApplyModifiedProperties();
            mPopupSelectedIndex = size;    // the last index is the the new category
        }

        void OnEnable()
        {
            foreach (Editor e in mCategoryEditors.Values)
                DestroyImmediate(e);
            mCategoryEditors.Clear();
            ClearUnusedObjects();
        }

        void ShowContextMenu(SerializedProperty prop, string name)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("New Category..."), false, AddNewCategory);
            if (prop != null && name != null)
            {
                menu.AddItem(new GUIContent("Rename \'" + name + "\'"), false, StartRenamingCategory, prop);
                menu.AddItem(new GUIContent("Delete \'" + name + "\'"), false, Delete, prop);
            }
            menu.ShowAsContext();
        }

        bool EndRenaimingCategory(SerializedProperty prop)
        {
            var cat = prop.objectReferenceValue as DataSeriesCategory;
            if (NewName.Length <= 0)
            {
                EditorUtility.DisplayDialog("Error", "visual property name can't be an empty string", "close");
                return false;
            }
            var data = serializedObject.FindProperty("mSeriesData");

            if (data == null)
            {
                ChartEditorCommon.SupportLog("editor error");
                return false;
            }

            SerializedProperty innerItems = data.FindPropertyRelative("mSerielizedCategories");
            int size = innerItems.arraySize;

            for (int i = 0; i < size; i++)
            {
                SerializedProperty Serializedcategory = innerItems.GetArrayElementAtIndex(i);
                var tmpCat = Serializedcategory.objectReferenceValue as DataSeriesCategory;
                if (cat == tmpCat)
                    continue;
                if (tmpCat.Name == NewName)
                {
                    EditorUtility.DisplayDialog("Error", "Category named \"" + NewName + "\" already exist in this chart", "close");
                    return false;
                }
            }

           ((IPrivateSetName)cat).SetName(NewName);
            cat.gameObject.name = NewName;
            return true;
        }

        void StartRenamingCategory(object obj)
        {
            var prop = (obj as SerializedProperty);
            if (prop == null)
                return;
            var cat = prop.objectReferenceValue as DataSeriesCategory;
            Renaming = cat.name;
            NewName = Renaming;
            startRename = true;
        }

        void Delete(object obj)
        {
            var prop = (obj as SerializedProperty);
            if (prop == null)
                return;
            var cat = prop.objectReferenceValue as DataSeriesCategory;
            Editor edit;
            if (mCategoryEditors.TryGetValue(cat.Name, out edit))
            {
                ((DataSeriesCategoryEditor)edit).DeleteCategory();
                mCategoryEditors.Remove(cat.Name);
                DestroyImmediate(edit);
            }
            --mPopupSelectedIndex;
            DeleteCategory(cat.Name);
        }

        public override void OnInspectorGUI()
        {
            bool isPrefab = PrefabUtility.IsPartOfAnyPrefab(target);
            SectionNameStyle.fontSize = 20;
            SectionNameStyle.fixedHeight = 30;
            //  base.OnInspectorGUI();
            ChartEditorCommon.Splitter();
            PropertySelection = EditorPrefs.GetInt("Bitsplash.DataVisualizer.EditorTab", 0);
            PropertySelection = GUILayout.Toolbar(PropertySelection, new string[] { "View Portion", "Axis", "Data","Configuration"});
            EditorPrefs.SetInt("Bitsplash.DataVisualizer.EditorTab", PropertySelection);
            EditorGUI.BeginChangeCheck();
            //EditorGUILayout.LabelField("Axis", SectionNameStyle, GUILayout.Height(30));
            var axis = serializedObject.FindProperty("mAxis");
            if (PropertySelection == 0)
            {
                if (axis.objectReferenceValue != null)
                {
                    if (mAxisEditor == null)
                    {
                        mAxisEditor = (DataSeriesAxisEditor)Editor.CreateEditor(axis.objectReferenceValue, typeof(DataSeriesAxisEditor));
                    }
                    mAxisEditor.ViewMode = true;
                    // mAxisEditor.Repaint();
                    mAxisEditor.OnInspectorGUI();
                }
                else
                {
                    Repaint();
                }
              //  Repaint();
            }

            if (PropertySelection == 1)
            {
                if (axis.objectReferenceValue != null)
                {
                    if (mAxisEditor == null)
                    {
                        mAxisEditor = (DataSeriesAxisEditor)Editor.CreateEditor(axis.objectReferenceValue, typeof(DataSeriesAxisEditor));
                    }

                    mAxisEditor.ViewMode = false;
                    //  mAxisEditor.Repaint();
                    mAxisEditor.OnInspectorGUI();
                }
                else
                {
                    Repaint();
                }
               // Repaint();
            }
            if (PropertySelection == 3)
            {
                var numProp = serializedObject.FindProperty("defaultNumberFormat");
                EditorGUILayout.PropertyField(numProp);
                var dateProp = serializedObject.FindProperty("defaultDateFormat");
                EditorGUILayout.PropertyField(dateProp);
                var prefabProp = serializedObject.FindProperty("LoadingOverlay");
                EditorGUILayout.PropertyField(prefabProp);
            }
            if (PropertySelection == 2)
            {
                var data = serializedObject.FindProperty("mSeriesData");

                if (data == null)
                {
                    ChartEditorCommon.SupportLog("editor error");
                    return;
                }

                SerializedProperty innerItems = data.FindPropertyRelative("mSerielizedCategories");
                int size = innerItems.arraySize;

                if (mNames == null || mNames.Length != size)
                    mNames = new string[size];

                if (mOrderItems == null || mOrderItems.Length != size)
                    mOrderItems = new int[size];

                for (int i = 0; i < size; i++)
                {
                    SerializedProperty Serializedcategory = innerItems.GetArrayElementAtIndex(i);
                    var cat = Serializedcategory.objectReferenceValue as DataSeriesCategory;
                    mNames[i] = cat.Name;
                    mOrderItems[i] = cat.ViewOrder;
                }

                Array.Sort(mOrderItems, mNames);
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                string selectedName = null;
                SerializedProperty prop = null;

                if (Event.current.type == EventType.Layout && cancel)
                {
                    cancel = false;
                    Renaming = null;
                    NewName = null;
                }

                if (Renaming != null)
                {
                    GUI.SetNextControlName("RenamingItem");
                    bool returnClick = ((Event.current.type == EventType.KeyDown) && (Event.current.keyCode == KeyCode.Return));
                    NewName = EditorGUILayout.TextField(NewName);
                    Rect r = GUILayoutUtility.GetLastRect();

                    if (startRename)
                    {
                        startRename = false;
                        EditorGUI.FocusTextInControl("RenamingItem");
                    }

                    selectedName = Renaming;
                    prop = findItemInArray(selectedName, innerItems);

                    if (GUILayout.Button("Apply", EditorStyles.miniButton) || returnClick)
                    {
                        if (EndRenaimingCategory(prop))
                        {
                            cancel = true;
                            Repaint();
                        }
                    }
                    if (Event.current.type == EventType.MouseUp && (r.Contains(Event.current.mousePosition) == false))
                    {
                        cancel = true;
                        Repaint();
                    }
                }
                else
                {
                    mPopupSelectedIndex = EditorGUILayout.Popup(mPopupSelectedIndex, mNames);
                    if (mPopupSelectedIndex >= 0 && mPopupSelectedIndex < mNames.Length)
                    {
                        selectedName = mNames[mPopupSelectedIndex];
                        prop = findItemInArray(selectedName, innerItems);
                    }
                    GUI.enabled = !isPrefab;
                    if (GUILayout.Button("...", EditorStyles.miniButton))
                    {
                        ShowContextMenu(prop, selectedName);
                    }
                    GUI.enabled = true;
                }

                EditorGUILayout.EndHorizontal();
                if (prop != null && selectedName != null)
                {
                    Editor edit;
                    var propRef = prop.objectReferenceValue;
                    if (mCategoryEditors.TryGetValue(selectedName, out edit) == false || edit.target == null)
                    {
                        if (edit != null)
                            DestroyImmediate(edit);
                        edit = Editor.CreateEditor(propRef, typeof(DataSeriesCategoryEditor));
                        mCategoryEditors[selectedName] = edit;
                    }

                    //EditorGUILayout.LabelField(selectedName);s
                    //edit.Repaint();
                    edit.OnInspectorGUI();
                }
            }
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                var behave = (target as MonoBehaviour);
                if (behave != null)
                    behave.SendMessage("OnValidate", SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}