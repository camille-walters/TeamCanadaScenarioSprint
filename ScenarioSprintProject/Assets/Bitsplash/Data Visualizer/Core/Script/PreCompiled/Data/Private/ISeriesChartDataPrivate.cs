using DataVisualizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataVisualizer{ 
    interface ISeriesChartDataPrivate
    {
        IDataSeries GetSeries(string category);

        IDataSeries RemoveSeries(string category);

        void SetSeries(string category, IDataSeries series);
    }
}
