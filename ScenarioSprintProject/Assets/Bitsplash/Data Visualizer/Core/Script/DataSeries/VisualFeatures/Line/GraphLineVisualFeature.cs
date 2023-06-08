using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace DataVisualizer{
    [Serializable]
    public class GraphLineVisualFeature : UvLengthVisualFeature
    {
        protected static string mVisualFeatureTypeName = "Graph Line";
        [SerializeField]
        [Tooltip("If true , the line thickness would scale with zooming of the chart. Otherwise the line thickness remains constant when zooming")]
        private bool scalesWithView;

        /// <summary>
        /// If true , the line thickness would scale with zooming of the chart. Otherwise the line thickness remains constant when zooming
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

        //[SerializeField]
        //[Tooltip("show line caps (smoother lines)")]
        //private bool lineCap = true;

        //public bool LineCap
        //{
        //    get { return lineCap; }
        //    set
        //    {
        //        lineCap = value;
        //        DataChanged();
        //    }
        //}

        [SerializeField]
        [Tooltip("the thickness of the graph line")]
        private double lineThickness;

        /// <summary>
        /// the thickness of the graph line
        /// </summary>
        public double LineThickness
        {
            get { return lineThickness; }
            set
            {
                lineThickness = value;
                DataChanged();
            }
        }

      //  [SerializeField]
      //  [HideInInspector]
        [Tooltip("These are the tiling settings for the line material. You can change this property to tile a texture along the line. (such as dotted line texture)")]
        [FormerlySerializedAs("lineMateritalTiling")]
        private MaterialTiling lineMaterialTiling = new MaterialTiling();

        /// <summary>
        /// These are the tiling settings for the line material. You can change this property to tile a texture along the line. (such as dotted line texture)
        /// </summary>
        protected MaterialTiling LineMaterialTiling
        {
            get
            {
                return lineMaterialTiling;
            }
            set
            {
                ChartCommon.RuntimeWarning("Line material tiling is not yet supported");
                //lineMateritalTiling = value;
                //DataChanged();
            }
        }

        [SerializeField]
        [ChartSpecificMaterial]
        [Tooltip("the material used for line drawing")]
        [FormerlySerializedAs("lineMaterital")]
        private Material lineMaterial;

        /// <summary>
        /// the material used for line drawing
        /// </summary>
        public Material LineMaterial
        {
            get
            {
                return lineMaterial;
            }
            set
            {
                lineMaterial = value;
                DataChanged();
            }
        }

       // [SerializeField]
        [Tooltip("the source channel from the chart data")]
        VectorDataSource fromDataChannel = VectorDataSource.Positions;

        protected VectorDataSource FromDataChannel
        {
            get { return fromDataChannel; }
            set
            {
                fromDataChannel = value;
                DataChanged();
            }
        }

        //[SerializeField]
        [Tooltip("the source channel from the chart data")]
        VectorDataSource toDataChannel = VectorDataSource.Positions;

        protected VectorDataSource ToDataChannel
        {
            get { return toDataChannel; }
            set
            {
                toDataChannel = value;
                DataChanged();
            }
        }

        public override string VisualFeatureTypeName
        {
            get { return mVisualFeatureTypeName; }
        }

        public override IDataSeries GenerateSeries(GameObject item)
        {
            return item.AddComponent<GraphLineDataSeries>();
        }
    }
}
