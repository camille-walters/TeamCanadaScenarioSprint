using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThetaList;

namespace DataVisualizer{
    interface IVertexArray : IVertexReciver
    {
        /// <summary>
        /// ensures the array has been created , if it wans't, then it is created
        /// </summary>
        void EnsureArray();
        /// <summary>
        /// discards the current array and frees all memory associated with it
        /// </summary>
        void DiscardArray();
        /// <summary>
        /// the vertex count of the vertex array
        /// </summary>
        int VertexCount { get; }
        /// <summary>
        /// the vertex capacity for this array object
        /// </summary>
        int VertexCapacity { get; }
        /// <summary>
        /// adds vertices to the array
        /// </summary>
        /// <param name="count"></param>
        void AddVertices(int count);
        /// <summary>
        /// removes vertices from the array
        /// </summary>
        /// <param name="count"></param>
        void RemoveVertices(int count);
        /// <summary>
        /// copies vertices within the array
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="count"></param>
        void CopyFromTo(int from, int to, int count);
        /// <summary>
        /// clears the array
        /// </summary>
        void Clear();
        /// <summary>
        /// controls where vertices are added when calling IVertexReciver.AppendFast
        /// </summary>
        int WriteBase {get; set;}

    }
}
