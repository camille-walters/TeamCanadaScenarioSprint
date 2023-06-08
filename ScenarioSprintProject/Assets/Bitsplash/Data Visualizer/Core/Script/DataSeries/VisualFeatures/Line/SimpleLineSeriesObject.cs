using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataVisualizer{
    class SimpleLineSeriesObject : LineSeriesObject
    {
        public static int ConstItemSize { get { return 4; } }

        public override int ItemSize { get { return ConstItemSize; } }
    }
}
