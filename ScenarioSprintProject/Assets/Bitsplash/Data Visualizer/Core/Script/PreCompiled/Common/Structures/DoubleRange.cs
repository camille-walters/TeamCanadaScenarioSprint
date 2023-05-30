using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataVisualizer{
    public struct DoubleRange
    {
        double mMin, mMax;
        bool mInverted;

        public DoubleRange(double first, double last)
        {
            if (first < last)
            {
                mInverted = false;
                mMin = first;
                mMax = last;
            }
            else
            {
                mInverted = true;
                mMax = first;
                mMin = last;
            }
        }
        public double First
        {
           get
           {
                return mInverted ? mMax : mMin;
           }
        }
        public double Last
        {
            get
            {
                return mInverted ? mMin : mMax;
            }
        }
        public double Size
        {
            get { return mMax - mMin; }
        }

        public double Max
        {
            get{return mMax;}
        }

        public double Min
        {
            get { return mMin; }
        }
    }
}
