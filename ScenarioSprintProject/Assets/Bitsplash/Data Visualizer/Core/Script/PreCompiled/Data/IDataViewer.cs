using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    /// <summary>
    /// allows to view the data of a stacked data holder
    /// </summary>
    public interface IDataViewer
    {
        /// <summary>
        /// the raw position array of the data. This should not be modified
        /// </summary>
        /// <param name="stack">the stack index</param>
        /// <returns>the array</returns>
        DoubleVector3[] RawPositionArray(int stack);
        /// <summary>
        /// the raw end position array of the data. This should not be modified
        /// </summary>
        /// <param name="stack">the stack index</param>
        /// <returns>the array</returns>
        DoubleVector3[] RawEndPositionArray(int stack);
        /// <summary>
        /// the raw size array of the data. This should not be modified
        /// </summary>
        /// <param name="stack">the stack index</param>
        /// <returns>the array</returns>
        double[] RawSizeArray(int stack);
        /// <summary>
        /// the raw start/end array of the data. This should not be modified
        /// </summary>
        /// <param name="stack">the stack index</param>
        /// <returns>the array</returns>
        DoubleRange[] RawStartEndArray(int stack);
        /// <summary>
        /// the raw high/low array of the data. This should not be modified
        /// </summary>
        /// <param name="stack">the stack index</param>
        /// <returns>the array</returns>
        DoubleRange[] RawHighLowArray(int stack);
        /// <summary>
        /// the raw error range array of the data. This should not be modified
        /// </summary>
        /// <param name="stack">the stack index</param>
        /// <returns>the array</returns>
        DoubleRange[] RawErrorRangeArray(int stack);
        /// <summary>
        /// the raw bounding volume array of the data. This should not be modified
        /// </summary>
        /// <param name="stack">the stack index</param>
        /// <returns>the array</returns>
        DoubleRect?[] RawBoundingVolume(int stack);
        /// <summary>
        /// the raw color array of the data. This should not be modified
        /// </summary>
        /// <param name="stack">the stack index</param>
        /// <returns>the array</returns>
        Color32[] RawColorArray(int stack);
        /// <summary>
        /// the raw user data array of the data. This should not be modified
        /// </summary>
        /// <param name="stack">the stack index</param>
        /// <returns>the array</returns>
        object[] RawUserDataArray(int stack);
        /// <summary>
        /// the raw string/name array of the data. This should not be modified
        /// </summary>
        /// <param name="stack">the stack index</param>
        /// <returns>the array</returns>
        string[] RawNameArray(int stack);

        /// <summary>
        /// the offset of the sub array of this viewer.
        /// The sub array is a portion of the full array that is defined by an offset and a count
        /// </summary>
        int SubArrayOffset { get; }
        /// <summary>
        /// the view array for this data viewer
        /// </summary>
        /// <returns></returns>
        int[] RawViewArray();
        /// <summary>
        /// the amount of stacks
        /// </summary>
        int StackCount { get; }
        /// <summary>
        /// the amount of elements in each stack
        /// </summary>
        int Count { get; }
        /// <summary>
        /// the channel composition of the data holder
        /// </summary>
        ChannelType CurrentChannels { get; }
        /// <summary>
        /// the name of the underlaying object
        /// </summary>
        String Name { get; }
        /// <summary>
        /// the bounds of the data in the viewer
        /// </summary>
        DataBounds DataBounds(int stack);
    }
}
