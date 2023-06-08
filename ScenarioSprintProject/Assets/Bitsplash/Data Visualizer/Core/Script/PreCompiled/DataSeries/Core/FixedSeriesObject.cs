using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataVisualizer{
    /// <summary>
    /// a series object with a fixed item count
    /// </summary>
    public abstract class FixedSeriesObject : CoreSeriesObject
    {

        public override bool EnsureItemCount(DataSeriesBase mapper)
        {
            return false;
        }
    }
}
