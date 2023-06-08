using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    public abstract class DivisionAxisVisualFeature : AxisVisualFeature
    {
        protected InputType optimizationHint = InputType.GeneralPurpose;
        protected UvLengthVisualFeature.MaterialTilingMethodOptions materialTilingMethod = UvLengthVisualFeature.MaterialTilingMethodOptions.NoTilingBestPerformance;
        /// <summary>
        /// the direction this visual property defines
        /// </summary>
        [SerializeField]
        protected AxisDimension dimension;

        public AxisDimension Dimension
        {
            get { return dimension; }
            set {
                dimension = value;
                DataChanged();
            }
        }
    }
}
