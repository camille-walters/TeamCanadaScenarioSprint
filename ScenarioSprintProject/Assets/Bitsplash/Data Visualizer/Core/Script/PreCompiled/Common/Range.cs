using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataVisualizer{
    public struct Range
    {
        public static readonly Range Empty = new Range(float.NaN, float.NaN);
        float mMin, mMax;

        public Range(float first, float last)
        {
            if (first < last)
            {
                mMin = first;
                mMax = last;
            }
            else
            {
                mMax = first;
                mMin = last;
            }
        }
        public bool IsEmpty
        {
            get
            {
                return float.IsNaN(mMin) || float.IsNaN(mMax);
            }
        }
        public float Size
        {
            get { return mMax - mMin; }
        }

        public float Max
        {
            get { return mMax; }
        }

        public float Min
        {
            get { return mMin; }
        }
    }
}
