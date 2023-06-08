using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataVisualizer{
    /// <summary>
    /// interface for data that feeds the dataseriesgraphic. It is used mainly to hide private methods via explicit interface implementation
    /// </summary>
    public interface IDataSeriesGraphicLink
    {
        /// <summary>
        /// clips the visible objects into view
        /// </summary>
        void PrepareVertices();
        /// <summary>
        /// gets the count of items that are viewable given the current view settings
        /// </summary>
        int ViewableIndexCount { get; }
        /// <summary>
        /// gets the index of the next viewable object, when index is between 0 and ViewableIndexCount
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        int GetViewableIndex(int index);
        /// <summary>
        /// the count of indices in the data link
        /// </summary>
        int Count { get; }
        /// <summary>
        /// the count of inner indices for a specified index
        /// </summary>
        /// <param name="index1"></param>
        /// <returns></returns>
        int InnerCount(int index1);
        /// <summary>
        /// get the following graphic in the data link
        /// </summary>
        /// <param name="index1"></param>
        /// <param name="index2"></param>
        /// <returns></returns>
        SeriesObject this[int index1, int index2] { get; }
    }
}
