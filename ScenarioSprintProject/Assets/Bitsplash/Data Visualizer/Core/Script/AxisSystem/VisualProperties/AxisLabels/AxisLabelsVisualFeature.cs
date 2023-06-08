using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    public abstract class AxisLabelsVisualFeature : AxisVisualFeature
    {
        /// <summary>
        /// a string sperated with '|' indicating which axis visual properties are affected by this property
        /// </summary>
        [SerializeField]
        protected string ApplyToProperties;
        

        /// IMPORTANT: Add both realtive offset and fixed offset. The relative offset allows you to positions the labels according to the chart size (so setting x= 0.5 is the horizontal middle of the chart)
    }
}
