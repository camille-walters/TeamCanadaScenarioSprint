using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace DataVisualizer.Editors
{
    [CustomEditor(typeof(AxisSystem))]
    class DataSeriesAxisEditor : Editor
    {
        const int ScrollableHeightExpand = 300;
        const int ScrollableHeightShrink = 100;

        Vector2 mViewScroll = new Vector2(); 
        private Dictionary<string, Editor> mVisualFeatureEditors = new Dictionary<string, Editor>();
        string Renaming = null;
        string newName = null;
        bool startRenaming = false;
        SerializedProperty[] mProperties;
        int[] mOrders;
        bool cancel = false;
        string catName = "~AxisSystem";
        int scrollHeight = ScrollableHeightShrink;
        public bool ViewMode = false;
        string GetVisualFeatureTypeName(Type visualProp)
        {
            var field = visualProp.GetField("mVisualFeatureTypeName", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            if (field == null)
            {
                ChartEditorCommon.SupportLog("mVisualFeatureTypeName not found for type " + visualProp.Name);
                return null;
            }
            return field.GetValue(null) as string;
        }

        void AddVisualFeatureContext()
        {
            GenericMenu menu = new GenericMenu();
            foreach (Type t in ChartEditorCommon.GetDerivedTypes<AxisVisualFeature>())
            {
                string name = GetVisualFeatureTypeName(t);
                if (name == null)
                    continue;
                menu.AddItem(new GUIContent(name), false, AddVisualFeature, t);
            }
            menu.ShowAsContext();
        }

        string FindName(Type t)
        {
            string defName = GetVisualFeatureTypeName(t) + "-";
            SerializedProperty innerItems = serializedObject.FindProperty("mSerielizedProperties");
            int size = innerItems.arraySize;
            int max = -1;
            for (int i = 0; i < size; i++)
            {
                SerializedProperty SerializedVisualPropEntry = innerItems.GetArrayElementAtIndex(i);
                AxisVisualFeature prop = SerializedVisualPropEntry.objectReferenceValue as AxisVisualFeature;
                string name = prop.Name;
                if (name.StartsWith(defName))
                {
                    name = name.Substring(defName.Length);
                    int res;
                    if (int.TryParse(name, out res))
                        max = Math.Max(max, res);
                }
            }
            return defName + (max + 1);
        }

        void ClearVisualFeature(SerializedProperty prop)
        {
            var toDelete = prop.objectReferenceValue as AxisVisualFeature;
            if (toDelete == null)
            {
                ChartEditorCommon.SupportLog("can't removed visual property");
                return;
            }
            //ChartEditorCommon.DestoryInnerObject(toDelete.gameObject, target);
            Undo.DestroyObjectImmediate(toDelete.gameObject);
        }

        int FindMaxOrder()
        {
            SerializedProperty innerItems = serializedObject.FindProperty("mSerielizedProperties");
            int size = innerItems.arraySize;
            int viewOrder = -1;
            for (int i = 0; i < size; i++)
            {
                SerializedProperty item = innerItems.GetArrayElementAtIndex(i);
                AxisVisualFeature propRef = item.objectReferenceValue as AxisVisualFeature;
                viewOrder = Math.Max(propRef.ViewOrder, viewOrder);

            }
            return viewOrder;
        }

        void AddVisualFeature(object arg)
        {
            int order = FindMaxOrder() + 1;
            SerializedProperty innerItems = serializedObject.FindProperty("mSerielizedProperties");
            Type t = (Type)arg;
            string newName = FindName(t);
            innerItems.InsertArrayElementAtIndex(0);
            SerializedProperty newItem = innerItems.GetArrayElementAtIndex(0);
            GameObject parent = ((AxisSystem)target).gameObject;
            var prop = ChartEditorCommon.CreateInnerObject<AxisVisualFeature>(t, catName + "-" + newName, parent);
            ((IPrivateSetName)prop).SetName(newName);
            prop.ViewOrder = order;
            newItem.objectReferenceValue = prop;
            //EditorUtility.SetDirty(target);
            //AssetDatabase.SaveAssets();
            //AssetDatabase.Refresh();
            serializedObject.ApplyModifiedProperties();
            Repaint();
        }


        void CreateVisualFeatureOptionsMenu(string name, SerializedProperty SerializedVisualPropEntry)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Rename"), false, StartRenameVisualFeature, name);
            menu.AddItem(new GUIContent("Delete"), false, DeleteVisualFeature, SerializedVisualPropEntry);
            menu.ShowAsContext();
        }

        void StartRenameVisualFeature(object arg)
        {
            string str = arg as string;
            if (str == null)
                return;
            startRenaming = true;
            Renaming = str;
            newName = str;
            Repaint();
        }

        /// <summary>
        /// returns true if the editor should leave renaming state
        /// </summary>
        /// <param name="serlizedToRename"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        bool EndRenameVisualFeature(SerializedProperty serlizedToRename, string newName)
        {
            if (newName.Length <= 0)
            {
                EditorUtility.DisplayDialog("Error", "visual property name can't be an empty string", "close");
                return false;
            }
            AxisVisualFeature propRefToRename = serlizedToRename.objectReferenceValue as AxisVisualFeature;
            SerializedProperty innerItems = serializedObject.FindProperty("mSerielizedProperties");
            int size = innerItems.arraySize;
            for (int i = 0; i < size; i++)  // check that the new name is not already present in the category
            {
                SerializedProperty SerializedVisualPropEntry = innerItems.GetArrayElementAtIndex(i);
                AxisVisualFeature propRef = SerializedVisualPropEntry.objectReferenceValue as AxisVisualFeature;
                if (propRef == propRefToRename) // this is the object being renamed , skip it
                    continue;
                if (propRef.Name == newName)
                {
                    EditorUtility.DisplayDialog("Error", "visual property named \"" + newName + "\"already exist in this category", "close");
                    return false;
                }
            }
            Undo.SetCurrentGroupName("Rename");
            Undo.RecordObject(propRefToRename, "");
            ((IPrivateSetName)propRefToRename).SetName(newName);
            Undo.RecordObject(propRefToRename.gameObject, "");
            propRefToRename.gameObject.name = catName + "-" + newName;
            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            return true;
        }

        void DeleteVisualFeature(object arg)
        {
            SerializedProperty SerializedVisualPropEntry = arg as SerializedProperty;
            if (arg == null)
                return;
            var propref = (SerializedVisualPropEntry.objectReferenceValue as AxisVisualFeature);
            if (EditorUtility.DisplayDialog("Delete \"" + propref.Name + "\"", "Are you sure you want to delete \"" + propref.Name + "\"?", "Yes", "No") == false)
                return;
            Undo.RegisterCompleteObjectUndo(target,"Delete");
            ClearVisualFeature(SerializedVisualPropEntry);
            SerializedVisualPropEntry.objectReferenceValue = null;
            SerializedProperty innerItems = serializedObject.FindProperty("mSerielizedProperties");
            ChartEditorCommon.ClearNullElements(innerItems);
            serializedObject.ApplyModifiedProperties();
            Repaint();
        }

        void TriggerOnValidate()
        {
            var behave = (target as MonoBehaviour);
            behave = behave.GetComponentInParent<DataSeriesChart>();
            if (behave != null)
                behave.SendMessage("OnValidate", SendMessageOptions.DontRequireReceiver);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            bool isPrefab = PrefabUtility.IsPartOfAnyPrefab(target);
            if (ViewMode == true)
            {
                SerializedProperty view = serializedObject.FindProperty("view");
                //EditorGUILayout.PropertyField(view, true);
                var autoVer = view.FindPropertyRelative("automaticVerticallView");
                var directionVer = view.FindPropertyRelative("verticalDirection");
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(directionVer, true);
                EditorGUILayout.PropertyField(autoVer, true);
                if (autoVer.boolValue == false)
                {
                    var sizeVer = view.FindPropertyRelative("verticalViewSize");
                    var originVer = view.FindPropertyRelative("verticalViewOrigin");
                    var scrollVer = view.FindPropertyRelative("verticalScrolling");
                    var autoScrollVer = view.FindPropertyRelative("autoScrollVertically");
                    var panVer = view.FindPropertyRelative("verticalPanning");
                    var zoomVer = view.FindPropertyRelative("verticalZooming");
                    EditorGUILayout.PropertyField(originVer, true);
                    EditorGUILayout.PropertyField(scrollVer, true);
                    EditorGUILayout.PropertyField(sizeVer, true);
                    EditorGUILayout.PropertyField(autoScrollVer, true);
                    EditorGUILayout.PropertyField(panVer, true);
                    EditorGUILayout.PropertyField(zoomVer, true);
                }
                EditorGUILayout.EndVertical();
                var autoHor = view.FindPropertyRelative("automaticHorizontalView");
                var directionHor = view.FindPropertyRelative("horizontalDirection");
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(directionHor, true);
                EditorGUILayout.PropertyField(autoHor, true);
                if (autoHor.boolValue == false)
                {
                    var sizeHor = view.FindPropertyRelative("horizontalViewSize");
                    var originHor = view.FindPropertyRelative("horizontalViewOrigin");
                    var scrollHor = view.FindPropertyRelative("horizontalScrolling");
                    var autoScrollHor = view.FindPropertyRelative("autoScrollHorizontally");
                    var panHor = view.FindPropertyRelative("horizontalPanning");
                    var zoomHor = view.FindPropertyRelative("horizontalZooming");
                    EditorGUILayout.PropertyField(originHor, true);
                    EditorGUILayout.PropertyField(scrollHor, true);
                    EditorGUILayout.PropertyField(sizeHor, true);
                    EditorGUILayout.PropertyField(autoScrollHor, true);
                    EditorGUILayout.PropertyField(panHor, true);
                    EditorGUILayout.PropertyField(zoomHor, true);
                }
                EditorGUILayout.EndVertical();
                var diag = view.FindPropertyRelative("thicknessBaseDiagonal");
                EditorGUILayout.PropertyField(diag, true);

                //              var end = view.GetEndProperty();
                //                view.Next(true);
            }
            else
            {
                SerializedProperty innerItems = serializedObject.FindProperty("mSerielizedProperties");
                int size = innerItems.arraySize;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                EditorGUILayout.LabelField("Visual Properties");
                scrollHeight = ScrollableHeightExpand;
                EditorGUILayout.EndHorizontal();

                GUI.enabled = !isPrefab;
                if (GUILayout.Button("Add New..."))
                {
                    AddVisualFeatureContext();
                }
                GUI.enabled = true;
                if (mProperties == null || mProperties.Length != size)
                    mProperties = new SerializedProperty[size];
                if (mOrders == null || mOrders.Length != size)
                    mOrders = new int[size];

                for (int i = 0; i < size; i++)
                {
                    SerializedProperty SerializedVisualPropEntry = innerItems.GetArrayElementAtIndex(i);
                    mProperties[i] = SerializedVisualPropEntry;
                    AxisVisualFeature propRef = SerializedVisualPropEntry.objectReferenceValue as AxisVisualFeature;
                    mOrders[i] = propRef.ViewOrder;
                }

                Array.Sort(mOrders, mProperties);
                mViewScroll = EditorGUILayout.BeginScrollView(mViewScroll, GUILayout.Height(scrollHeight));

                for (int i = 0; i < size; i++)
                {
                    SerializedProperty SerializedVisualPropEntry = mProperties[i];
                    AxisVisualFeature propRef = SerializedVisualPropEntry.objectReferenceValue as AxisVisualFeature;
                    string name = propRef.Name;

                    Editor edit;
                    if (propRef == null || name == null)
                        continue;

                    if (mVisualFeatureEditors.TryGetValue(name, out edit) == false || edit.target == null)
                    {
                        if (edit != null)
                            DestroyImmediate(edit);
                        edit = Editor.CreateEditor(propRef, typeof(AxisVisualFeatureEditor));
                        mVisualFeatureEditors[name] = edit;
                    }

                    EditorGUILayout.Separator();
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

                    bool showMore = false;
                    bool upPress = false;
                    bool downPress = false;

                    if (Event.current.type == EventType.Layout && cancel)
                    {
                        cancel = false;
                        Renaming = null;
                        newName = null;
                    }

                    if (Renaming == name)
                    {
                        bool returnClick = ((Event.current.type == EventType.KeyDown) && (Event.current.keyCode == KeyCode.Return));
                        GUI.SetNextControlName("RenamingItem");
                        newName = EditorGUILayout.TextField(newName);
                        Rect r = GUILayoutUtility.GetLastRect();

                        if (GUILayout.Button("Apply", EditorStyles.miniButtonRight) || returnClick)
                        {
                            if (EndRenameVisualFeature(SerializedVisualPropEntry, newName))
                            {
                                cancel = true;
                                Repaint();
                            }
                        }

                        if (startRenaming)
                        {
                            startRenaming = false;
                            EditorGUI.FocusTextInControl("RenamingItem");
                        }
                        if (Event.current.type == EventType.MouseUp && (r.Contains(Event.current.mousePosition) == false))
                        {
                            cancel = true;
                            Repaint();
                        }
                    }
                    else
                    {
                        EditorGUI.indentLevel++;
                        SerializedVisualPropEntry.isExpanded = EditorGUILayout.Foldout(SerializedVisualPropEntry.isExpanded,name + " (" + propRef.VisualFeatureTypeName + ")");
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        GUI.enabled = !isPrefab;
                        upPress = GUILayout.Button("\u25B2", EditorStyles.miniButtonRight);
                        downPress = GUILayout.Button("\u25BC", EditorStyles.miniButtonRight);
                        showMore = GUILayout.Button("...", EditorStyles.miniButtonRight);
                        GUI.enabled = true;
                        EditorGUI.indentLevel--;

                    }

                    EditorGUILayout.EndHorizontal();

                    if (showMore)
                        CreateVisualFeatureOptionsMenu(name, SerializedVisualPropEntry);

                    if (upPress)
                    {
                        if (i > 0)
                        {
                            SerializedProperty prev = mProperties[i - 1];
                            var prevRef = prev.objectReferenceValue as AxisVisualFeature;
                            int tmp = prevRef.ViewOrder;
                            prevRef.ViewOrder = propRef.ViewOrder;
                            propRef.ViewOrder = tmp;
                            Repaint();
                            TriggerOnValidate();
                        }
                    }

                    if (downPress)
                    {
                        if (i < size - 1)
                        {
                            SerializedProperty next = mProperties[i + 1];
                            var nextRef = next.objectReferenceValue as AxisVisualFeature;
                            int tmp = nextRef.ViewOrder;
                            nextRef.ViewOrder = propRef.ViewOrder;
                            propRef.ViewOrder = tmp;
                            Repaint();
                            TriggerOnValidate(); 
                        }
                    }

                    //    edit.Repaint();
                    if (SerializedVisualPropEntry.isExpanded)
                    {
                        EditorGUI.indentLevel++;
                        edit.OnInspectorGUI();
                        EditorGUI.indentLevel--;
                    }
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();

                ChartEditorCommon.ClearNullElements(innerItems);
            }
            serializedObject.ApplyModifiedProperties();



        }

        public void OnEnable()
        {
            AxisSystem cat = (target as AxisSystem);
            if (cat != null)
                cat.ClearUnusedObjects();
            mVisualFeatureEditors = new Dictionary<string, Editor>();
        }
    }
}
