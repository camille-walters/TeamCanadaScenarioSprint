using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    public static class LogOptions
    {
        public const string ALL = "all";
        public const string UvDataSeries = "UvDataSeries";
        public const string Axis = "Axis";
        public const string DataManipulation = "DataManipulation";
        public const string DataSeries = "DataSeries";
        public const string Graphic = "Graphic";
        public const string GraphicOptimization = "GraphicOptimization";
        public const string GraphicArrayManagers = "GraphicArrayManagers";
        public const string Mapping = "Mapping";    
        public const string UvMapping = "UvMapping";
        public const string MultipleGraphic = "MultipleGraphic";
        public const string Other = "other";

        private static HashSet<string> AllTagOptions = null;
        private static HashSet<string> TagOptions = null;

        public static bool IsTagEnabledExplicit(string tag)
        {
            if (TagOptions == null)
                LoadOptions();
            return TagOptions.Contains(tag);
        }

        public static bool IsTagEnabled(string tag)
        {
            if (TagOptions == null)
                LoadOptions();
            if (TagOptions.Contains("all"))
                return true;
            if (TagOptions.Contains(Other) && (AllTagOptions.Contains(tag) == false))
                return true;
            return TagOptions.Contains(tag);
        }

        public static void LoadOptions()
        {
            if (AllTagOptions == null)
            {
                AllTagOptions = new HashSet<string>();
                AllTagOptions.Add(Axis);
                AllTagOptions.Add(UvDataSeries);
                AllTagOptions.Add(DataManipulation);
                AllTagOptions.Add(DataSeries);
                AllTagOptions.Add(Graphic);
                AllTagOptions.Add(GraphicOptimization);
                AllTagOptions.Add(GraphicArrayManagers);
                AllTagOptions.Add(Mapping);
                AllTagOptions.Add(UvMapping);
                AllTagOptions.Add(MultipleGraphic);
            }
            if (TagOptions == null)
                TagOptions = new HashSet<string>();
            else
                TagOptions.Clear();

            var items = PlayerPrefs.GetString("GraphAndChartLogTags", "");            
            foreach (string str in items.Split(';'))
                TagOptions.Add(str);
            TagOptions.Remove("");
        }

    }
}
