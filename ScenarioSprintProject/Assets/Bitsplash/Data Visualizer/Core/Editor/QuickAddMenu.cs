using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class QuickAddMenu
{
    [MenuItem("Tools/Graph And Chart/Add Data Series Chart")]
    public static void AddItem()
    {
        GameObject addUnder = Selection.activeGameObject;
        if(addUnder == null)
        {
            var canvas = GameObject.FindObjectOfType<Canvas>();
            if(canvas != null)
                addUnder = canvas.gameObject;
        }
        if (addUnder != null && AssetDatabase.Contains(addUnder))
            addUnder = null;
        if (addUnder == null)
        {
            EditorUtility.DisplayDialog("No parent selected", "Use the hierarchy view to select a parent object for the chart", "ok");
            return;
        }
        var prefab = (GameObject)Resources.Load("Prefabs/DataSeriesChart");
        if (prefab == null)
        {
            EditorUtility.DisplayDialog("Can't locate prefab", "The menu prefab was not imported with Graph and Chart, please import everything under the \"core\" folder", "ok");
            return;
        }
        var obj = GameObject.Instantiate(prefab, addUnder.transform);
        obj.name = prefab.name;
    }
}
