using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{

    public struct PreMappedVertexSlim
    {
        public Vector2 uv;
        public Vector4 tangent;
        public Color32 color;
        public DoubleVector3 preMapped;
        //  public UIVertex mapped;
    }
    public struct PreMappedVertex
    {
        public Vector2 uv;
        public Vector4 tangent;
        public Color32 color;
        public float ExtrusionFactor;
        public float ExtrusionAngleInterpolator;
        public DoubleVector3 preMapped;
      //  public UIVertex mapped;
    }
}
