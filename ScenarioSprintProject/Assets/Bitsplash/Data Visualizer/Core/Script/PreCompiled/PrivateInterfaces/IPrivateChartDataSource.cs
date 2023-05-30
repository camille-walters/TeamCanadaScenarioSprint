using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataVisualizer{
    public interface IPrivateChartDataSource
    {
        Dictionary<string, DataSeriesCategory> Categories { get; }
        Dictionary<string, IDataSeries> DataSeries { get; }
        IDataSeries GetSeries(string category);
        IDataSeries RemoveSeries(string category);
        void SetSeries(string category, IDataSeries series);
        void NotifyViewChanged(IPrivateAxisSystem system);
        void UniformUpdate();
        void OnValidate();
        void Awake();
    }
}
