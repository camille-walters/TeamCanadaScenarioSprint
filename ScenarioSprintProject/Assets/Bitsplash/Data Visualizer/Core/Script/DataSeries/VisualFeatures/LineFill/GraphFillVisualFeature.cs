using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace DataVisualizer{
    [Serializable]
    public class GraphFillVisualFeature : UvLengthVisualFeature
    {
        protected static string mVisualFeatureTypeName = "Graph Fill";

       // [SerializeField]
        [Tooltip("If true , the graph fill is streched over the fill area. Otherwise ")]
        private FillUv uvMapping = FillUv.Clamp;

        /// <summary>
        /// If true , the line thickness would scale with zooming of the chart. Otherwise the line thickness remains constant when zooming
        /// </summary>
        protected FillUv UvMapping
        {
            get { return uvMapping; }
            set
            {
                uvMapping = value;
                DataChanged();
            }
        }

        /// <summary>
        /// indicates where the minimum texture edge is mapped to
        /// </summary>
        [SerializeField]
        [Tooltip("indicates where the minimum texture edge is mapped to ")]
        private FillUvMapping minimumUv = FillUvMapping.Auto;

        /// <summary>
        /// indicates where the minimum texture edge is mapped to
        /// </summary>
        public FillUvMapping MinimumUv
        {
            get { return minimumUv; }
            set
            {
                minimumUv = value;
                DataChanged();
            }
        }

        /// <summary>
        /// indicates where the maximum texture edge is mapped to
        /// </summary>
        [SerializeField]
        [Tooltip("indicates where the maximum texture edge is mapped to")]
        private FillUvMapping maximumUv = FillUvMapping.Auto;

        /// <summary>
        ///indicates where the maximum texture edge is mapped to
        /// </summary>
        public FillUvMapping MaximumUv
        {
            get { return maximumUv; }
            set
            {
                maximumUv = value;
                DataChanged();
            }
        }

        [SerializeField]
        [Tooltip("the material used for line drawing")]
        [ChartSpecificMaterial]
        [FormerlySerializedAs("fillMaterital")]
        private Material fillMaterial;

        /// <summary>
        /// the material used for line drawing
        /// </summary>
        public Material FillMaterial
        {
            get
            {
                return fillMaterial;
            }
            set
            {
                fillMaterial = value;
                DataChanged();
            }
        }

        public override string VisualFeatureTypeName
        {
            get { return mVisualFeatureTypeName; }
        }

        public override IDataSeries GenerateSeries(GameObject item)
        {
            return item.AddComponent<GraphLineFillDataSeries>();
        }
    }
}
