using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataVisualizer{
    /// <summary>
    /// informs Graph And Chart for any optimizations that can be applied to a given material
    /// </summary>
    interface IMaterialOptimization
    {
        /// <summary>
        /// the name of the shader that can be optimized , must be uniqe for each implementation
        /// </summary>
        string ShaderName { get; }
        /// <summary>
        /// the name of the texture properties that should have their uv's streched using SetTextureScale (used for graph fill)
        /// </summary>
        IEnumerable<string> UvTextureNames { get; }
    }
}
