using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Data_Visualizer.Core.Script.PreCompiled.Data.DataViews.GraphDataType
{
    partial class GraphDownSample
    {
        void RemoveFromStart(int segmentIndex)
        {
            if (segmentIndex >= mSegments.Count)
            {
                Clear();
                RaiseOnClear();
                return;
            }
            int prevSegmentIndex = segmentIndex - 1;
            if(prevSegmentIndex >= 0)
            {

            }
            //var seg = info.Segments[segment];
        }
    }
}
