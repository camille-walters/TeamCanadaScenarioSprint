using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DataVisualizer
{
    public class ChartClickEventArgs
    {
        public string Category { get; private set; }
        public int Index { get; private set; }
        public DoubleVector3 ChartPoint { get; private set; }
        public Vector3 LocalPoint { get; private set; }
        public String FormattedString { get; private set; }

        public ChartClickEventArgs(string category,int index,DoubleVector3 chartPoint,Vector3 localPoint,string formatted)
        {
            Category = category;
            Index = index;
            ChartPoint = chartPoint;
            LocalPoint = localPoint;
            FormattedString = formatted;
        }
    }
}
