using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace DataVisualizer{    
    public class FixedDivision2DAxisVisualFeature : FixedDivisionAxisVisualFeature
    {
        protected static string mVisualFeatureTypeName = "Fixed Divisions 2D";

        [SerializeField]
        [Tooltip("If true , the line thickness would scale with zooming of the chart. Otherwise the line thickness remains constant when zooming")]
        private bool scalesWithView = false;

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

        [SerializeField]
        [Tooltip("the thickness of the graph line")]
        private double lineThickness;

        /// <summary>
        /// the thickness of the axis line
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

        [SerializeField]
        [HideInInspector]
        [FormerlySerializedAs("lineMateritalTiling")]
        [Tooltip("These are the tiling settings for the line material. You can change this property to tile a texture along the line. (such as dotted line texture)")]
        private MaterialTiling lineMaterialTiling;

        /// <summary>
        /// These are the tiling settings for the line material. You can change this property to tile a texture along the line. (such as dotted line texture)
        /// </summary>
        public MaterialTiling LineMaterialTiling
        {
            get
            {
                return lineMaterialTiling;
            }
            set
            {
                lineMaterialTiling = value;
                DataChanged();
            }
        }
        

        [SerializeField]
        [ChartSpecificMaterial]
        [FormerlySerializedAs("lineMaterital")]
        [Tooltip("the material used for line drawing")]
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
#pragma warning disable 0414
        VectorDataSource fromDataChannel = VectorDataSource.Positions;

        VectorDataSource toDataChannel = VectorDataSource.EndPositions;
#pragma warning restore 0414
        public override string VisualFeatureTypeName
        {
            get { return mVisualFeatureTypeName; }
        }

        public override IChartVisualObject GenerateAxisObject(GameObject addTo)
        {
            return new FixedDivisionAxisGenerator<GraphLineDataSeries>(Name, addTo);
        }
    }
}
