using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    public abstract class AxisVisualFeature : VisualFeatureBase
    {
        /// <summary>
        /// Generates a IAxisObject object that matches this Visual Feature
        /// </summary>
        /// <returns></returns>
        public abstract IChartVisualObject GenerateAxisObject(GameObject obj);

        [SerializeField]
        [Tooltip("If true , the axis feature is drawn above the chart. this can be used for labels or borders for example")]
        private bool topLevel = false;

        /// <summary>
        /// If true , the axis feature is drawn above the chart. this can be used for labels or borders for example
        /// </summary>
        public bool TopLevel
        {
            get { return topLevel; }
            set
            {
                topLevel = value;
                ViewOrderChanged();
            }
        }

    }
}
