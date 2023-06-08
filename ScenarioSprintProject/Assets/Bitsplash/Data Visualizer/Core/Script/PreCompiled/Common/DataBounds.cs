using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataVisualizer{
    [Serializable]
    public class DataBounds
    {
        /// <summary>
        /// the bounds of the data in this axis system
        /// </summary>
        public double? MaxX, MaxY, MinX, MinY, MaxRadius;

        public void Clear()
        {
            MaxX = null;
            MinX = null;
            MaxY = null;
            MinY = null;
            MaxRadius = null;
        }


        public override string ToString()
        {
            return "maxx " + MaxX + " maxy " + MaxY + " minx " + MinX + " miny " + MinY;
        }
        private double? Select(double? a,double? b,bool isMax)
        {
            if (a.HasValue && b.HasValue)
            {
                if(isMax)
                    return Math.Max(a.Value, b.Value);
                return Math.Min(a.Value, b.Value);
            }
            if (b.HasValue)
                return b.Value;
            if (a.HasValue)
                return a.Value;
            return null;
        }

        public void ModifyMinMax(DataBounds dataBounds)
        {
            MaxX = Select(MaxX, dataBounds.MaxX, true);
            MaxY = Select(MaxY, dataBounds.MaxY, true);
            MinX = Select(MinX, dataBounds.MinX, false);
            MinY = Select(MinY, dataBounds.MinY, false);
            MaxRadius = Select(MaxRadius, dataBounds.MaxRadius, true);
        }

        public void ModifyMinMax(DoubleRect boundingVolume)
        {
            DoubleVector3 max = boundingVolume.Max;
            DoubleVector3 min = boundingVolume.Min;
            if(min.x > max.x)
            {
                double tmp = min.x;
                min.x = max.x;
                max.x = tmp;
            }
            if (min.y > max.y)
            {
                double tmp = min.y;
                min.y = max.y;
                max.y = tmp;
            }

            if (MaxX.HasValue == false || MaxX.Value < max.x)
                MaxX = max.x;
            if (MinX.HasValue == false || MinX.Value > min.x)
                MinX = min.x;
            if (MaxY.HasValue == false || MaxY.Value < max.y)
                MaxY = max.y;
            if (MinY.HasValue == false || MinY.Value > min.y)
                MinY = min.y;
        }

        public void ModifyMinMax(DoubleVector3 point)
        {
            if (MaxRadius.HasValue == false || MaxRadius.Value < point.z)
                MaxRadius = point.z;
            if (MaxX.HasValue == false || MaxX.Value < point.x)
                MaxX = point.x;
            if (MinX.HasValue == false || MinX.Value > point.x)
                MinX = point.x;
            if (MaxY.HasValue == false || MaxY.Value < point.y)
                MaxY = point.y;
            if (MinY.HasValue == false || MinY.Value > point.y)
                MinY = point.y;
        }

    }
}
