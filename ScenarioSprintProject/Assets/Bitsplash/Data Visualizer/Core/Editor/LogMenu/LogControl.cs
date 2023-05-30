//#define MENUITEM

using UnityEditor;
using System.Linq; 
using UnityEngine;
using System.Collections.Generic;
using DataVisualizer;


[InitializeOnLoad]
public static class LogControl
{
    private const string MENU_TAG_ALL = "Tools/Graph And Chart/Log/Show all logs";
    private const string MENU_TAG_Axis = "Tools/Graph And Chart/Log/Axis";
    private const string MENU_TAG_UvDataSeries = "Tools/Graph And Chart/Log/UvDataSeries";
    private const string MENU_TAG_DataManipulation = "Tools/Graph And Chart/Log/DataManipulation";
    private const string MENU_TAG_DataSeries = "Tools/Graph And Chart/Log/DataSeries";
    private const string MENU_TAG_Graphic = "Tools/Graph And Chart/Log/Graphic";
    private const string MENU_TAG_MultipleGraphic = "Tools/Graph And Chart/Log/MultipleGraphic";
    private const string MENU_TAG_GraphicOptimization = "Tools/Graph And Chart/Log/GraphicOptimization";
    private const string MENU_TAG_GraphicArrayManagers = "Tools/Graph And Chart/Log/GraphicArrayManagers";
    private const string MENU_TAG_Mapping = "Tools/Graph And Chart/Log/Mapping";
    private const string MENU_TAG_UvMapping = "Tools/Graph And Chart/Log/UvMapping";
    private const string MENU_TAG_OTHER = "Tools/Graph And Chart/Log/Other logs";

    private const string MENU_DONTHIDE_NAME = "Tools/Graph And Chart/Dont Hide Inner Objects";
    private const string MENU_ASSERT_NAME = "Tools/Graph And Chart/Integrity/Enable Assert";
    private const string MENU_ASSERT_PERFORMANCE_NAME = "Tools/Graph And Chart/Integrity/Enable Performance Intensive Asserts";
    private const string MENU_NAME = "Tools/Graph And Chart/Log/Enable Logs";

    private const string LogName = "ChartDevLogEnabled";
    private const string DontHideName = "DONTHIDEINNEROBJECTS";
    private const string AssertName = "ChartDevAssert";
    private const string AssertNamePerformance = "ChartDevAssertPerformance";

    static LogControl()
    {
        EditorApplication.delayCall += () => {
            CheckMenuEnabled(MENU_NAME, LogName);
            CheckMenuEnabled(MENU_ASSERT_NAME, AssertName);
            CheckMenuEnabled(MENU_ASSERT_PERFORMANCE_NAME, AssertNamePerformance);
        };
    }

    static bool IsDefined(string name)
    {
        var target = EditorUserBuildSettings.activeBuildTarget;
        var group = BuildPipeline.GetBuildTargetGroup(target);
        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
        foreach (string s in defines.Split(';'))
        {
            if (s == name)
                return true;
        }
        return false;
    }

    static void ToogleTagOption(string tagName)
    {
        string options = PlayerPrefs.GetString("GraphAndChartLogTags", "");
        options = ToogleOption(options, tagName);
        PlayerPrefs.SetString("GraphAndChartLogTags", options);
        LogOptions.LoadOptions();
    }

    static string ToogleOption(string options, string toogle)
    {
        var items = options.Split(';');
        int toogleIndex = -1;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == toogle)
            {
                toogleIndex = i;
                break;
            }
        }
        if (toogleIndex == -1)
            return string.Join(";", items.Concat(new string[] { toogle }).ToArray());
        items[toogleIndex] = items[0];  // put the last define where this one was to delete it.
        var skipItem = items.Skip(1); // skip the first item because it was copied
        return string.Join(";", skipItem.ToArray());
    }

    static void ToogleDefine(string option)
    {
        var target = EditorUserBuildSettings.activeBuildTarget;
        var group = BuildPipeline.GetBuildTargetGroup(target);
        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
        string toogled = ToogleOption(defines, option);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(group, toogled);
    }
#if MENUITEM
    [MenuItem(LogControl.MENU_DONTHIDE_NAME)]
#endif
    private static void ToggleDontHide()
    {
        ToogleDefine(DontHideName);
        CheckMenuEnabled(MENU_DONTHIDE_NAME, DontHideName);
        var target = EditorUserBuildSettings.activeBuildTarget;
    }
#if MENUITEM
    [MenuItem(LogControl.MENU_ASSERT_NAME)]
#endif
    private static void ToggleAssertAction()
    {
        ToogleDefine(AssertName);
        CheckMenuEnabled(MENU_ASSERT_NAME, AssertName);
        var target = EditorUserBuildSettings.activeBuildTarget;
    }

#if MENUITEM
    [MenuItem(LogControl.MENU_ASSERT_PERFORMANCE_NAME)]
#endif
    private static void TogglePerformanceAssertAction()
    {
        ToogleDefine(AssertNamePerformance);
        CheckMenuEnabled(MENU_ASSERT_PERFORMANCE_NAME, AssertNamePerformance);
    }
#if MENUITEM
    [MenuItem(LogControl.MENU_NAME)]
#endif
    private static void ToggleAction()
    {
        ToogleDefine(LogName);
        CheckMenuEnabled(MENU_NAME, LogName);
    }

    private static void ValidateDefineMenu(string menu, string defineName)
    {
        bool enabled = IsDefined(defineName);
        Menu.SetChecked(menu, enabled);
    }

    private static void ValidateMenu(string menu, string optionName)
    {
        bool enabled = LogOptions.IsTagEnabledExplicit(optionName);
        Menu.SetChecked(menu, enabled);
    }
#if MENUITEM
    [MenuItem(LogControl.MENU_ASSERT_NAME, true)]
#endif
    private static bool ToggleAssertActionValidate()
    {
        CheckMenuEnabled(MENU_ASSERT_NAME, AssertName);
        return true;
    }
#if MENUITEM
    [MenuItem(LogControl.MENU_ASSERT_PERFORMANCE_NAME, true)]
#endif
    private static bool ToggleAssertPerformanceActionValidate()
    {
        CheckMenuEnabled(MENU_ASSERT_PERFORMANCE_NAME, AssertNamePerformance);
        return true;
    }

#if MENUITEM
    [MenuItem(LogControl.MENU_NAME, true)]
#endif
    private static bool ToggleActionValidate()
    {
        CheckMenuEnabled(MENU_NAME, LogName);
        return true;
    }

    /// <summary>
    /// sets the menu visiblity
    /// </summary>
    public static void CheckMenuEnabled(string menuName, string defineName)
    {
        bool enabled = IsDefined(defineName);
        // Debug.Log("enabled" + enabled);
#if MENUITEM
        Menu.SetChecked(menuName, enabled);
#endif
        EditorPrefs.SetBool(menuName, enabled);
   //     mEnabled = enabled;
    }

    public static void CheckAssertEnabled()
    {
        bool enabled = IsDefined(AssertName);
        // Debug.Log("enabled" + enabled);
        Menu.SetChecked(MENU_ASSERT_NAME, enabled);
        EditorPrefs.SetBool(MENU_ASSERT_NAME, enabled);
   //     mEnabled = enabled;
    }

#if MENUITEM
    [MenuItem(MENU_TAG_ALL)]
#endif
    private static void ToggleOptionAll()
    {
        ToogleTagOption(LogOptions.ALL);
        ValidateMenu(MENU_TAG_ALL, LogOptions.ALL);
    }
    static bool IsEnabled()
    {
        return IsDefined(LogName);
    }
#if MENUITEM
    [MenuItem(MENU_TAG_ALL, true)]
#endif
    private static bool ToggleOptionValidateAll()
    {
        ValidateMenu(MENU_TAG_ALL, LogOptions.ALL);
        return IsEnabled();
    }
#if MENUITEM
    [MenuItem(MENU_TAG_UvDataSeries)]
#endif
    private static void ToggleOptionUvDataSeries()
    {
        ToogleTagOption(LogOptions.UvDataSeries);
        ValidateMenu(MENU_TAG_UvDataSeries, LogOptions.UvDataSeries);
    }
#if MENUITEM
    [MenuItem(MENU_TAG_UvDataSeries, true)]
#endif
    private static bool ToggleOptionValidateUvDataSeries()
    {
        ValidateMenu(MENU_TAG_UvDataSeries, LogOptions.UvDataSeries);
        return IsEnabled() && (LogOptions.IsTagEnabledExplicit(LogOptions.ALL) == false);
    }
#if MENUITEM
    [MenuItem(MENU_TAG_Axis)]
#endif
    private static void ToggleOptionAxis()
    {
        ToogleTagOption(LogOptions.Axis);
        ValidateMenu(MENU_TAG_Axis, LogOptions.Axis);
    }
#if MENUITEM
    [MenuItem(MENU_TAG_Axis, true)]
#endif
    private static bool ToggleOptionValidateAxis()
    {
        ValidateMenu(MENU_TAG_Axis, LogOptions.Axis);
        return IsEnabled() && (LogOptions.IsTagEnabledExplicit(LogOptions.ALL) == false);
    }

#if MENUITEM
    [MenuItem(MENU_TAG_DataManipulation)]
#endif
    private static void ToggleOptionDataManipulations()
    {
        ToogleTagOption(LogOptions.DataManipulation);
        ValidateMenu(MENU_TAG_DataManipulation, LogOptions.DataManipulation);
    }
#if MENUITEM
    [MenuItem(MENU_TAG_DataManipulation, true)]
#endif
    private static bool ToggleOptionValidateDataManipulations()
    {
        ValidateMenu(MENU_TAG_DataManipulation, LogOptions.DataManipulation);
        return IsEnabled() && (LogOptions.IsTagEnabledExplicit(LogOptions.ALL) == false);
    }
#if MENUITEM
    [MenuItem(MENU_TAG_DataSeries)]
#endif
    private static void ToggleOptionDataSeries()
    {
        ToogleTagOption(LogOptions.DataSeries);
        ValidateMenu(MENU_TAG_DataSeries, LogOptions.DataSeries);
    }
#if MENUITEM
    [MenuItem(MENU_TAG_DataSeries, true)]
#endif
    private static bool ToggleOptionValidateDataSeries()
    {
        ValidateMenu(MENU_TAG_DataSeries, LogOptions.DataSeries);
        return IsEnabled() && (LogOptions.IsTagEnabledExplicit(LogOptions.ALL) == false);
    }
#if MENUITEM
    [MenuItem(MENU_TAG_Graphic)]
#endif
    private static void ToggleOptionGraphic()
    {
        ToogleTagOption(LogOptions.Graphic);
        ValidateMenu(MENU_TAG_Graphic, LogOptions.Graphic);
    }
#if MENUITEM
    [MenuItem(MENU_TAG_Graphic, true)]
#endif
    private static bool ToggleOptionValidateGraphic()
    {
        ValidateMenu(MENU_TAG_Graphic, LogOptions.Graphic);
        return IsEnabled() && (LogOptions.IsTagEnabledExplicit(LogOptions.ALL) == false);
    }
#if MENUITEM
    [MenuItem(MENU_TAG_GraphicArrayManagers)]
#endif
    private static void ToggleOptionGraphicArrayManagers()
    {
        ToogleTagOption(LogOptions.GraphicArrayManagers);
        ValidateMenu(MENU_TAG_GraphicArrayManagers, LogOptions.GraphicArrayManagers);
    }
#if MENUITEM
    [MenuItem(MENU_TAG_GraphicArrayManagers, true)]
#endif
    private static bool ToggleOptionValidateGraphicArrayManagers()
    {
        ValidateMenu(MENU_TAG_GraphicArrayManagers, LogOptions.GraphicArrayManagers);
        return IsEnabled() && (LogOptions.IsTagEnabledExplicit(LogOptions.ALL) == false);
    }
#if MENUITEM
    [MenuItem(MENU_TAG_MultipleGraphic)]
#endif
    private static void ToggleOptionMultipleGraphic()
    {
        ToogleTagOption(LogOptions.MultipleGraphic);
        ValidateMenu(MENU_TAG_MultipleGraphic, LogOptions.MultipleGraphic);
    }
#if MENUITEM
    [MenuItem(MENU_TAG_MultipleGraphic, true)]
#endif
    private static bool ToggleOptionValidateMultipleGraphic()
    {
        ValidateMenu(MENU_TAG_MultipleGraphic, LogOptions.MultipleGraphic);
        return IsEnabled() && (LogOptions.IsTagEnabledExplicit(LogOptions.ALL) == false);
    }
#if MENUITEM
    [MenuItem(MENU_TAG_GraphicOptimization)]
#endif
    private static void ToggleOptionGraphicOptimization()
    {
        ToogleTagOption(LogOptions.GraphicOptimization);
        ValidateMenu(MENU_TAG_GraphicOptimization, LogOptions.GraphicOptimization);
    }
#if MENUITEM
    [MenuItem(MENU_TAG_GraphicOptimization, true)]
#endif
    private static bool ToggleOptionValidateGraphicOptimization()
    {
        ValidateMenu(MENU_TAG_GraphicOptimization, LogOptions.GraphicOptimization);
        return IsEnabled() && (LogOptions.IsTagEnabledExplicit(LogOptions.ALL) == false);
    }
#if MENUITEM
    [MenuItem(MENU_TAG_Mapping)]
#endif
    private static void ToggleOptionMapping()
    {
        ToogleTagOption(LogOptions.Mapping);
        ValidateMenu(MENU_TAG_Mapping, LogOptions.Mapping);
    }
#if MENUITEM
    [MenuItem(MENU_TAG_Mapping, true)]
#endif
    private static bool ToggleOptionValidateMapping()
    {
        ValidateMenu(MENU_TAG_Mapping, LogOptions.Mapping);
        return IsEnabled() && (LogOptions.IsTagEnabledExplicit(LogOptions.ALL) == false);
    }
#if MENUITEM
    [MenuItem(MENU_TAG_OTHER)]
#endif
    private static void ToggleOptionOther()
    {
        ToogleTagOption(LogOptions.Other);
        ValidateMenu(MENU_TAG_OTHER, LogOptions.Other);
    }
#if MENUITEM
    [MenuItem(MENU_TAG_OTHER, true)]
#endif
    private static bool ToggleOptionValidateOther()
    {
        ValidateMenu(MENU_TAG_OTHER, LogOptions.Other);
        return IsEnabled() && (LogOptions.IsTagEnabledExplicit(LogOptions.ALL) == false);
    }
#if MENUITEM
    [MenuItem(MENU_TAG_UvMapping)]
#endif
    private static void ToggleOptionUvMapping()
    {
        ToogleTagOption(LogOptions.UvMapping);
        ValidateMenu(MENU_TAG_UvMapping, LogOptions.Other);
    }
#if MENUITEM
    [MenuItem(MENU_TAG_UvMapping, true)]
#endif
    private static bool ToggleOptionValidateUvMapping()
    {
        ValidateMenu(MENU_TAG_UvMapping, LogOptions.UvMapping);
        return IsEnabled() && (LogOptions.IsTagEnabledExplicit(LogOptions.ALL) == false);
    }

}