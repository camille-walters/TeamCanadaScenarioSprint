using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace DataVisualizer{
    [Serializable]
    public class GraphPointVisualFeature : DataSeriesVisualFeature
    {
        protected static string mVisualFeatureTypeName = "Graph Point";
        [SerializeField]
        [Tooltip("The size of a point in the series")]
        private double pointSize = 5.0;

        /// <summary>
        /// The size of a point in the series
        /// </summary>
        public double PointSize
        {
            get { return pointSize; }
            set
            {
                pointSize = value;
                DataChanged();
            }
        }

        [SerializeField]
        [Tooltip("If true , the point thickness would scale with zooming of the chart. Otherwise the point thickness remains constant when zooming")]
        private bool scalesWithView;

        /// <summary>
        /// If true , the point thickness would scale with zooming of the chart. Otherwise the point thickness remains constant when zooming
        /// </summary>
        public bool ScalesWithView
        {
            get { return scalesWithView; }
            set
            {
                scalesWithView = value;
                DataChanged();
            }
        }

        [SerializeField]
        [ChartSpecificMaterial]
        [Tooltip("the material used for point drawing")]
        [FormerlySerializedAs("pointMaterital")]
        private Material pointMaterial;

        /// <summary>
        /// the material used for point drawing
        /// </summary>
        public Material PointMaterial
        {
            get
            {
                return pointMaterial;
            }
            set
            {
                pointMaterial = value;
                DataChanged();
            }
        }

        public override string VisualFeatureTypeName
        {
            get { return mVisualFeatureTypeName; }
        }

        public override IDataSeries GenerateSeries(GameObject item)
        {
            return item.AddComponent<GraphPointDataSeries>();
        }
    }
}
