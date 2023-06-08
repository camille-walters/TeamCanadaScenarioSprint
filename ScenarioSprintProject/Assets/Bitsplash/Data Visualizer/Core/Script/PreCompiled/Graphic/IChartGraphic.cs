
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThetaList;
using UnityEngine;

namespace DataVisualizer{
    public interface IChartGraphic
    {
        bool TringleStrip { get; set; }
        MappingFunction UvMapping { get; set; }
        bool HasColor { get; set; }
        bool HasTangent { get; set; }
        float ExtrusionAmount { get; set; }
        bool MaintainAngles { get; set; }
        Matrix4x4 DirectionTranform { get; set; }
        void SetMaterial(Material mat, bool isShared);

        bool HasFeature(string featureName);
        bool HasFeature(int id);

        void Invalidate();

        bool IsObjectActive { get; }
        GameObject gameObject { get; }

        int VertexCapacity { get; }
        int VertexCount { get; }

        IChartGraphic ParentGraphic { get; set; }
        /// <summary>
        /// called when the graphic is created
        /// set targetVertexCount to -1 to use default setting
        /// </summary>
        /// <param name="targetVertexCount"></param>
        void OnCreated(int targetVertexCount);

        /// <summary>
        /// builds the invalidated geometry of the graphic
        /// </summary>
        void SetVisible(bool enabled);


    }
}
