using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataVisualizer{
    public interface IChartData
    {
        void InvalidateCachedMinMax();
        /// <summary>
        /// updates the chart data object
        /// </summary>
        void Update();
        /// <summary>
        /// event handler for after deserialize
        /// </summary>
        void OnAfterDeserialize();
        /// <summary>
        /// event handler for before serialize
        /// </summary>
        void OnBeforeSerialize();
    }
}
