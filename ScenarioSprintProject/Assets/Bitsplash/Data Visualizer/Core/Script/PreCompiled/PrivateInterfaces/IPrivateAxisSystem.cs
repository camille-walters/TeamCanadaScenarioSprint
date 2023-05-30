using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    public interface IPrivateAxisSystem
    {
        event Action VisualObjectsChanged;
        bool IsActive { get; }
        VisualObjectCollection GetVisualObjects();
        AxisSystemView View { get; }
        void FitInto(ViewPortion localView);
        void RegisterDataSeries(DataSeriesBase mapper);
        void UnregisterDataSeries(DataSeriesBase mapper);
        void SetParent(IPrivateDataSeriesChart parent);
        void ValidateAxis();
        void OnValidate();
        void UniformUpdate();
        void SetEnabled(bool enabled);
        ViewPortion ChartSpaceView { get; }
    }
}
