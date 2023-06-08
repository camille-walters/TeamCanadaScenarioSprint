using DataVisualizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    public interface IDataSeries : IChartVisualObject
    {
        int CanvasViewOrder { get; }
        /// <summary>
        /// returns true if the data reflected by this series is valid for optimzation specified as a parameter. If the data changes the result of this method may change as well
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool IsValidForOptimization(string name);

        /// <summary>
        /// clears all the data in the series and sets the parament array as the data
        /// </summary>
        /// <param name="data"></param>
        void OnSet(IDataViewerNotifier data);
        /// <summary>
        /// returns the number of elements in the series
        /// </summary>
        int Count { get; }

        /// <summary>
        /// triggred when the chart starts loading the graphic data
        /// </summary>
        event Action<IDataSeries> DataStartLoading;
        /// <summary>
        /// triggerd when the chart ends loading the graphic data
        /// </summary>
        event Action<IDataSeries> DataDoneLoading;

    }
}
