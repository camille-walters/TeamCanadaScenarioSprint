using DataVisualizer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataVisualizer
{
    public partial class StackedGenericDataHolder
    {
        public int PickExact(DoubleVector3 point)
        {
            return PinPointPick(InnerPickExact(point), point);
        }
        public int Pick(DoubleVector3 point)
        {
            return PinPointPick( InnerPick(point),point);
        }
        int PinPointPick(int res,DoubleVector3 point)
        {
            if (res == 0)
                return 0;
            var positions = RawPositionArray(0);
            double v1 = positions[res].x;
            double v2 = positions[res - 1].x;
            if (Math.Abs(v1 - point.x) < Math.Abs(v2 - point.x))
                return res;
            return res - 1;
        }
        int InnerPick(DoubleVector3 point)
        {
            var graphView = TryGetView(GraphDataType.ViewName);
            if(graphView != null)
            {
                int count = graphView.Count;
                var raw = graphView.RawViewArray();
                var positions = graphView.RawPositionArray(0);
                var start = graphView.SubArrayOffset;

                int from = 0;
                int to = count;

                while (from <= to)
                {
                    int center = from + (to - from) / 2;
                    double centerX = positions[start + raw[center]].x;
                    if (centerX == point.x)
                        return start + raw[center];
                    if (point.x < centerX)
                        to = center - 1;
                    else
                        from = center + 1;
                }

                return start + raw[from];
            }
            return InnerPickExact(point);
        }
        int InnerPickExact(DoubleVector3 point)
        {
            var positions = RawPositionArray(0);
            int from = 0;
            int to = Count;

            while (from <= to)
            {
                int center = from + (to - from) / 2;
                double centerX = positions[center].x;
                if (centerX == point.x)
                    return center;
                if (point.x < centerX)
                    to = center - 1;
                else
                    from = center + 1;
            }

            return from;
        }
    }
}
