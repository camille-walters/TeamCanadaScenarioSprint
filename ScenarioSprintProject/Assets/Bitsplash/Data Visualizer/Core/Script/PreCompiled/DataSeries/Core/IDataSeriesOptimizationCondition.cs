using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataVisualizer{
    /// <summary>
    /// Implementing class are responsible for check conditions for a given optimzation. The instace is updated with any change in the series data , and maintains a boolean property DataCanBeOptimized that determines if the data is valid for the optimization represented by the instance
    /// example: This interface can be implemented to check if the data is sorted by x value for example, and then the data series can apply optimizations based on this.
    /// </summary>
    public interface IDataSeriesOptimizationCondition
    {
        /// <summary>
        /// the name of the condition
        /// </summary>
        string Name { get; }
        /// <summary>
        /// true if the data is valid for the optimization represented by this instance
        /// </summary>
        bool DataCanBeOptimized { get; }
        /// <summary>
        /// called when the data series is initiated
        /// </summary>
        void OnSet(GenericDataHolder holder);
        /// <summary>
        /// called when the data series is cleared
        /// </summary>
        void OnClear();
        /// <summary>
        /// called when a value is modified
        /// </summary>
        /// <param name="index"></param>
        /// <param name="prevValue"></param>
        void OnValueModified(int index);
        /// <summary>
        /// removes an item from the specified index
        /// </summary>
        /// <param name="index"></param>
        void OnRemove(int index);
        /// <summary>
        /// appends a data value to the end of the series
        /// </summary>
        /// <param name="data"></param>
        void OnAppend(int index);
        /// <summary>
        /// called when an item is inserted at the specified index
        /// </summary>
        /// <param name="index"></param>
        void OnInsert(int index);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        void OnAppendArray(int index);
        void OnSetArray(int index,int count);
    }
}
