using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    [Serializable]
    public abstract class UvLengthVisualFeature : DataSeriesVisualFeature
    {
        public enum MaterialTilingMethodOptions
        {
            /// <summary>
            /// fits the image perfrectly into the graphical data. This method is less fast but produces best looking result
            /// </summary>
            PerfectFit,
            /// <summary>
            /// fits the image unevenly into the graphical data. This method is very fast but may look uneven.It is best to use this for vertical patterns such as gradients
            /// </summary>
            UnevenWithBetterPerformances,
            /// <summary>
            /// does not tile the image at all, just applied it to each SeriesObject. this method yields the best performance
            /// </summary>
            NoTilingBestPerformance
        }

        //[SerializeField]
       // [HideInInspector]
        [Tooltip("Sets the matreial tiling method for this visual property")]
        protected MaterialTilingMethodOptions materialTilingMethod = MaterialTilingMethodOptions.UnevenWithBetterPerformances;
        
        /// <summary>
        /// If true , the line thickness would scale with zooming of the chart. Otherwise the line thickness remains constant when zooming
        /// </summary>
        public MaterialTilingMethodOptions MaterialTilingMethod
        {
            get { return materialTilingMethod; }
            set
            {
                ChartCommon.RuntimeWarning("Material tiling is not yet supported in this version");
                //materialTilingMethod = value;
                //DataChanged();
            }
        }
    }
}
