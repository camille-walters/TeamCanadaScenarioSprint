using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataVisualizer{
    /// <summary>
    /// Controls how uv coordinates are mapped into a graph fill 
    /// </summary>
    public enum FillUv
    {
        /// <summary>
        /// the uv is streched along the fill of the graph
        /// </summary>
        Strech,
        /// <summary>
        /// the uv will be clamped by the graph line. This is useful to keep aspect ration for some materials
        /// </summary>
        Clamp
    }
}
