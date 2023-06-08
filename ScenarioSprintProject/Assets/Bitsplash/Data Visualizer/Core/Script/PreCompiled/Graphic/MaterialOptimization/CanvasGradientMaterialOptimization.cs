using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataVisualizer{
    public class CanvasGradientMaterialOptimization : IMaterialOptimization
    {
        public string ShaderName
        {
            get { return "Chart/Canvas/Gradient"; }
        }

        public IEnumerable<string> UvTextureNames
        {
            get
            {
                yield return "_MainTex";
            }
        }
    }
}
