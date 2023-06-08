using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataVisualizer{
    class CanvasSolidMaterialOptimizer : IMaterialOptimization
    {
        public string ShaderName
        {
            get { return "DataVisualizer/Canvas/Solid"; }
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
