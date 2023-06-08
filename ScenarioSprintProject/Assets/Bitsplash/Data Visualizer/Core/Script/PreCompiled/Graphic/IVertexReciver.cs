using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThetaList;
using UnityEngine;

namespace DataVisualizer{
    public interface IVertexReciver
    {
        MeshArrays GetArrays();
        //void Append(SimpleList<UIVertex> v);
        //void AppendFast(Vector3 pos,Vector2 uv,Vector4 tangent,Color32 color);
       // void AppendFast(ref ChartMeshVertex v);
    }
}
