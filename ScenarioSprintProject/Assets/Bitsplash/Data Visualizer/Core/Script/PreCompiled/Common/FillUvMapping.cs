using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataVisualizer{
    [Serializable]
    public struct FillUvMapping
    {
        public static FillUvMapping Auto
        {
            get
            {
                FillUvMapping map = new FillUvMapping();
                map.Automatic = true;
                return map;
            }
        }

        /// <summary>
        /// if true , uv is mapped base on the minimum and maximum values of the series
        /// </summary>
        public bool Automatic;
        /// <summary>
        /// if "Automatic" is false , This value is taken to considration. This is the chart space value at which the edge of the fill texture will be mapped to 
        /// </summary>
        public double Value;
    }
}
