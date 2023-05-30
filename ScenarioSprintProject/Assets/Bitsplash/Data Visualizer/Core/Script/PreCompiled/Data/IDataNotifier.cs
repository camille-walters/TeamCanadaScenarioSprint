using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThetaList;

namespace DataVisualizer{
    /// <summary>
    /// notifies about changes its the data of a stacked data holder
    /// </summary>
    public interface IDataViewerNotifier : IDataViewer
    {

        /// <summary> 
        /// commits all changes made into the data holder. After this call it is safe to query the RawArray properties
        /// </summary>
        void CommitChanges();

        /// <summary>
        /// called when the data composition of the data holder is changed
        /// </summary>
        event Action<object> OnChannelCompositionChanged;
        /// <summary>
        /// called when the user has set a value (but not when the operation has been applied to an operation tree)
        /// </summary>
        event Action<object, ChannelType> OnSetArray;
        /// <summary>
        /// called when an empry array of data is appended to the data holder
        /// </summary>
        event Action<object, ChannelType,int> OnAppendArray;

        ///// <summary>
        ///// called when data is removed from the begining of the dataviewer
        ///// </summary>
        event Action<object, int> OnRemoveFromStart;

        ///// <summary>
        ///// called when data is removed from the end of the viewer
        ///// </summary>
        event Action<object, int> OnRemoveFromEnd;

        /// <summary>
        /// called when the user has set a value (but not when the operation has been applied to an operation tree)
        /// </summary>
        event Action<object, int> OnBeforeSet;
        /// <summary>
        /// called when the user has set a value (but not when the operation has been applied to an operation tree)
        /// </summary>
        event Action<object, int> OnSet;
        /// <summary>
        /// called when the user has inserted a value (but not when the operation has been applied to an operation tree)
        /// </summary>
        event Action<object, int> OnInsert;
        /// <summary>
        /// called when the user has inserted a value (but not when the operation has been applied to an operation tree)
        /// </summary>
        event Action<object, int> OnBeforeInsert;
        /// <summary>
        /// called when the user has removed a value (but not when the operation has been applied to an operation tree)
        /// </summary>
        event Action<object, int> OnBeforeRemove;
        /// <summary>
        /// called when the user has removed a value (but not when the operation has been applied to an operation tree)
        /// </summary>
        event Action<object, int> OnRemove;
        /// <summary>
        /// called right after an operation tree has benn applied to the data. 
        /// </summary>
        event Action<object, OperationTree<int>> OnAfterCommit;
        /// <summary>
        /// called right before an operation tree is applied to the data
        /// </summary>
        event Action<object, OperationTree<int>> OnBeforeCommit;
        /// <summary>
        /// called when the holder is cleared
        /// </summary>
        event Action<object> OnClear;

        /// <summary>
        /// called with any operation to notify that the data of this object has been changed and should be reflected on the data series
        /// </summary>
        event Action<object> OnUncomittedData;

        /// <summary>
        /// sets the view portion associated with the chart
        /// </summary>
        /// <param name="view"></param>
        void SetView(ViewPortion view);

        /// <summary>
        /// gets a view on this data viewer notifier. If the view doesn't exist , it is created
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IDataViewerNotifier GetDataView(string name);

        /// <summary>
        /// invoked when the parent view portion is set
        /// </summary>
        event Action<ViewPortion> ParentViewSet;
    }
}
