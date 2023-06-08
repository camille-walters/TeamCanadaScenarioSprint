using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataVisualizer{
    [Flags]
    public enum DataSeriesRefreshType : byte
    {
        /// <summary>
        /// No refresh
        /// </summary>
        None = 0,
        /// <summary>
        /// Invalidate the data series graphic only
        /// </summary>
        InvalidateGraphic = 1,
        /// <summary>
        /// re create all the underlying data entries only
        /// </summary>
        RecreateEntries =2,
        /// <summary>
        /// This like RecreateEntries | InvalidateGraphic . complete recreation of the data series
        /// </summary>
        FullRefresh = 3
    }
}
