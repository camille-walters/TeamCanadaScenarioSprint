using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    public interface IPrivateDataSeriesChart
    {
        GameObject FindChildObject(Transform parent, string name);
        GameObject CreateChildObject(Transform parent);
        IChartSeriesGraphic CreateEdgeGraphic(Transform parent);
        void MakeClippingObject(GameObject obj, bool cliping);
        Transform getTransform();
        IPrivateChartDataSource DataSource { get; }
        IPrivateAxisSystem Axis { get; }
        bool IsEnabled { get; }

        public bool IsParameterDateType(string parameter);

        public String DefaultDateFormat { get; }
        public String DefaultNumberFormat { get; }
        
    }
}
