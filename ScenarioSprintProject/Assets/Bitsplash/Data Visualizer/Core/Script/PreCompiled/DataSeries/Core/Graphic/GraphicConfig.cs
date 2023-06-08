
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    public class GraphicConfig
    { 
        public GraphicConfig()
        {
            MaintainAngles = true;
            DirectionTranform = Matrix4x4.identity;
            UvMapping = MappingFunction.One;
            ExtrusionAmount = 0f;
            TringleStrip = false;           
        }
        public bool TringleStrip { get; set; }
        public MappingFunction UvMapping { get; set; }
        public bool HasColor { get; set; }
        public bool HasTangent { get; set; }
        public float ExtrusionAmount { get; set; }
        public bool MaintainAngles { get; set; }
        public Matrix4x4 DirectionTranform { get; set; }
        public Material Material { get; set; }

        static string StringFor<T>(string name,T val)
        {
            return name + ": " + val;
        }
        public override string ToString()
        {
            return "{" +
                StringFor("TringleStrip", TringleStrip) + "," +
                StringFor("UvMapping", UvMapping) + "," +
                StringFor("HasColor", HasColor) + "," +
                StringFor("HasTangent", HasTangent) + "," +
                StringFor("ExtrusionAmount", ExtrusionAmount) + "," +
                StringFor("MaintainAngles", MaintainAngles) + "," +
                StringFor("DirectionTranform", DirectionTranform) + "," +
                StringFor("Material", Material) + "}";
        }
    }
}
