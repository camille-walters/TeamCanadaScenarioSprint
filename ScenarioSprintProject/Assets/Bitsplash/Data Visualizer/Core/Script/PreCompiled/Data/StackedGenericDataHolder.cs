using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThetaList;
using UnityEngine;

namespace DataVisualizer{
    /// <summary>
    /// holds data for a chart and allows stacked values (stacked graphs and stacked bars etc).
    /// This object allows fast use of insert and remove ops while maintianing speed of an array list
    /// for more information check out https://github.com/amirbenarye/Theta-List
    /// </summary>
    public partial class StackedGenericDataHolder : IDataViewerNotifier
    {
        static OperationTree<int> EmptyTree = new OperationTree<int>();
        /// <summary>
        /// This is a threshold of when an operation tree is created . if the index + CreateTreeThreshold >= Count no tree is created
        /// </summary>
        const int CreateTreeThreshold = 50;
        /// <summary>
        /// the stacks of the data holder. It is maintained that all stacks have exacly the same item count and the same channel format. 
        /// </summary>
        List<StackDataHolder> mStacks = new List<StackDataHolder>();
        /// <summary>
        /// the channels enabled on this holder
        /// </summary>
        ChannelType mChannels = ChannelType.None;
        /// <summary>
        /// Cache of operations performed on the holder. they asre applied at once when there is a requirment to read the data fast. This allows fast use of insert and remove operations.
        /// see https://github.com/amirbenarye/Theta-List
        /// </summary>
        OperationTree<int> mOperations = new OperationTree<int>();
        /// <summary>
        /// used in various overloads in order to avoid the creation of an array
        /// </summary>
        SimpleList<GenericDataItem> mTmpStackItems = new SimpleList<GenericDataItem>(true);

        DataSeriesCategory mParentCategory;

        DoubleVector3[] mCachedAppendArray = new DoubleVector3[1000];
        int mCachedAppendCount = 0;

        public StackedGenericDataHolder(DataSeriesCategory parentCat)
        {
            mParentCategory = parentCat;
            AddStack();
        }

        bool mCachingOperations = false;
        public ChannelType Channels { get { return mChannels; } }
        /// <summary>
        /// the amount of items in each stack.
        /// </summary>
        public int Count { get; private set; }
        /// <summary>
        /// the amount of stacks in the holders
        /// </summary>
        public int StackCount { get { return mStacks.Count; } }

        public ChannelType CurrentChannels
        {
            get { return mChannels; }
        }

        public string Name { get; set; }

        public int SubArrayOffset
        {
            get { return 0; }
        }

        private event Action<object> InnerOnClear;
        private event Action<object, ChannelType> InnerOnSetArray;
        private event Action<object, int> InnerOnBeforeSet;
        private event Action<object, int> InnerOnSet;
        private event Action<object, int> InnerOnInsert;
        private event Action<object, int> InnerOnBeforeInsert;
        private event Action<object, int> InnerOnRemove;
#pragma warning disable 0067
        private event Action<object, OperationTree<int>> InnerOnCommit;
        private event Action<object, OperationTree<int>> InnerBeforeCommit;
#pragma warning restore 0067
        private event Action<object> InnerDataChanged;
        private event Action<object, ChannelType,int> InnerOnAppendArray;
        private event Action<object> InnerChannelCompositionChanged;
        private event Action<object, int> InnerOnBeforeRemove;
        private event Action<object, int> InnerOnRemoveFromStart;
        private event Action<object, int> InnerOnRemoveFromEnd;

        private void InvokeAppendArray(ChannelType channel,int count)
        {
            if (InnerOnAppendArray != null)
                InnerOnAppendArray(this,channel, count);
        }
        private void InvokeDataChanged()
        {
            if(InnerDataChanged != null)
                InnerDataChanged(this);
        }
        private void InvokeOnSetArray(ChannelType type)
        {
            if (InnerOnSetArray != null)
                InnerOnSetArray(this,type);
        }

        private void InvokeOnBeforeSet(int index)
        {
            if (InnerOnBeforeSet != null)
                InnerOnBeforeSet(this, index);
        }

        private void InvokeOnSet(int index)
        {
            if (InnerOnSet != null)
                InnerOnSet(this, index);
        }

        event Action<object, ChannelType,int> IDataViewerNotifier.OnAppendArray
        {
            add
            {
                InnerOnAppendArray += value;
            }
            remove
            {
                InnerOnAppendArray -= value;
            }
        }

        event Action<object> IDataViewerNotifier.OnUncomittedData
        {
            add
            {
                InnerDataChanged += value;
            }
            remove
            {
                InnerDataChanged -= value;
            }
        }

        event Action<object> IDataViewerNotifier.OnClear
        {
            add
            {
                InnerOnClear += value;
            }
            remove
            {
                InnerOnClear -= value;
            }
        }
        event Action<object, int> IDataViewerNotifier.OnBeforeInsert
        {
            add
            {
                InnerOnBeforeInsert += value;
            }

            remove
            {
                InnerOnBeforeInsert -= value;
            }
        }
        event Action<object, int> IDataViewerNotifier.OnInsert
        {
            add
            {
                InnerOnInsert += value;
            }

            remove
            {
                InnerOnInsert -= value;
            }
        }

        event Action<object, int> IDataViewerNotifier.OnRemove
        {
            add
            {
                InnerOnRemove += value;
            }

            remove
            {
                InnerOnRemove -= value;
            }
        }

        event Action<object, int> IDataViewerNotifier.OnBeforeRemove
        {
            add
            {
                InnerOnBeforeRemove += value;
            }

            remove
            {
                InnerOnBeforeRemove -= value;
            }
        }

        event Action<object, int> IDataViewerNotifier.OnRemoveFromStart
        {
            add
            {
                InnerOnRemoveFromStart += value;
            }

            remove
            {
                InnerOnRemoveFromStart -= value;
            }
        }

        event Action<object, int> IDataViewerNotifier.OnRemoveFromEnd
        {
            add
            {
                InnerOnRemoveFromEnd += value;
            }

            remove
            {
                InnerOnRemoveFromEnd -= value;
            }
        }
        event Action<object> IDataViewerNotifier.OnChannelCompositionChanged
        {
            add
            {
                InnerChannelCompositionChanged += value;
            }
            remove
            {
                InnerChannelCompositionChanged -= value;
            }
        }


        event Action<object, ChannelType> IDataViewerNotifier.OnSetArray
        {
            add
            {
                InnerOnSetArray += value;
            }

            remove
            {
                InnerOnSetArray -= value;
            }
        }
        event Action<object, int> IDataViewerNotifier.OnBeforeSet
        {
            add
            {
                InnerOnBeforeSet += value;
            }

            remove
            {
                InnerOnBeforeSet -= value;
            }
        }
        event Action<object, int> IDataViewerNotifier.OnSet
        {
            add
            {
                InnerOnSet += value;
            }

            remove
            {
                InnerOnSet -= value;
            }
        }

        event Action<object, OperationTree<int>> IDataViewerNotifier.OnBeforeCommit
        {
            add
            {
                InnerBeforeCommit += value;
            }

            remove
            {
                InnerBeforeCommit -= value;
            }
        }
        event Action<object, OperationTree<int>> IDataViewerNotifier.OnAfterCommit
        {
            add
            {
                InnerOnCommit += value;
            }

            remove
            {
                InnerOnCommit -= value;
            }
        }

        /// <summary>
        /// maintains the count property after the underlying data has been changed
        /// </summary>
        private void MaintainCount()
        {
            if (mStacks.Count == 0)
                Count = 0;
            else
            {
                Count = mStacks[0].Holder.Count + mOperations.IndexBalance; // all stacks have the same count
            }
        }

        /// <summary>
        /// adds the following channels to the data holder
        /// </summary>
        /// <param name="t"></param>
        public void AddChannels(ChannelType t)
        {
            
            if ((mChannels & t) == t)
                return; // the channels are already added
            mChannels |= t;
            EnsureChannels();
            if (InnerChannelCompositionChanged != null)
                InnerChannelCompositionChanged(this);
        }

        /// <summary>
        /// checks that the stack count is exacly count. otherwise an exception is thrown
        /// </summary>
        /// <param name="count"></param>
        void ValidateStacks(int count)
        {
            if (mStacks.Count != count)
                throw new Exception("There are " + mStacks.Count + " stacks, but " + count + " values were specified");
        }

        /// <summary>
        /// checks that an index is in inclusive range of the item count of this object. if it is not an exception is thrown
        /// </summary>
        /// <param name="index"></param>
        void ValidateIndexInclusive(int index)
        {
            if (index < 0 || index > Count)
                throw new IndexOutOfRangeException();
        }

        /// <summary>
        /// checks that a stack index in in range of the current stack count.  if it is not an exception is thrown
        /// </summary>
        /// <param name="index"></param>
        void ValidateStackIndex(int index)
        {
            if (index < 0 || index >= mStacks.Count)
                throw new IndexOutOfRangeException("stack index out of range");
        }

        /// <summary>
        /// checkes that an index is in range of the item count of this object. if it is not an exception is thrown
        /// </summary>
        /// <param name="index"></param>
        void ValidateIndex(int index)
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException(String.Format("item index out of range with index {0} and count {1}",index,Count));
        }

        /// <summary>
        /// appends an item to the data holder. the data holder must have StackCount as 1 , otherwise an exception is thrown
        /// </summary>
        /// <param name="stack0"></param>
        public void Append(GenericDataItem stack0)
        {
            mTmpStackItems.Clear();
            GenericDataItem[] items = mTmpStackItems.AddEmpty(1);
            items[0] = stack0;
            Append(items, 1);
        }

        /// <summary>
        /// appends an item to the data holder. the data holder must have StackCount as 2 , otherwise an exception is thrown
        /// </summary>
        /// <param name="stack0"></param>
        /// <param name="stack1"></param>
        public void Append(GenericDataItem stack0, GenericDataItem stack1)
        {
            mTmpStackItems.Clear();
            GenericDataItem[] items = mTmpStackItems.AddEmpty(3);
            items[0] = stack0;
            items[1] = stack1;
            Append(items, 2);
        }

        /// <summary>
        ///appends an item to the data holder.the data holder must have StackCount as 3 , otherwise an exception is thrown
        /// </summary>
        /// <param name="stack0"></param>
        /// <param name="stack1"></param>
        /// <param name="stack2"></param>
        public void Append(GenericDataItem stack0, GenericDataItem stack1, GenericDataItem stack2)
        {
            mTmpStackItems.Clear();
            GenericDataItem[] items = mTmpStackItems.AddEmpty(3);
            items[0] = stack0;
            items[1] = stack1;
            items[2] = stack2;
            Append(items, 3);
        }

        /// <summary>
        /// set an item in the data holder.the data holder must have StackCount as 1 , otherwise an exception is thrown
        /// </summary>
        /// <param name="index"></param>
        /// <param name="stack0"></param>
        public void Set(int index, GenericDataItem stack0)
        {
            mTmpStackItems.Clear();
            GenericDataItem[] items = mTmpStackItems.AddEmpty(1);
            items[0] = stack0;
            Set(index, items, 1);
        }

        /// <summary>
        /// set an item in the data holder.the data holder must have StackCount as 2 , otherwise an exception is thrown
        /// </summary>
        /// <param name="index"></param>
        /// <param name="stack0"></param>
        /// <param name="stack1"></param>
        public void Set(int index, GenericDataItem stack0, GenericDataItem stack1)
        {
            mTmpStackItems.Clear();
            GenericDataItem[] items = mTmpStackItems.AddEmpty(2);
            items[0] = stack0;
            items[1] = stack1;
            Set(index, items, 2);
        }

        /// <summary>
        /// set an item in the data holder.the data holder must have StackCount as 3 , otherwise an exception is thrown
        /// </summary>
        /// <param name="index"></param>
        /// <param name="stack0"></param>
        /// <param name="stack1"></param>
        /// <param name="stack2"></param>
        public void Set(int index, GenericDataItem stack0, GenericDataItem stack1, GenericDataItem stack2)
        {
            mTmpStackItems.Clear();
            GenericDataItem[] items = mTmpStackItems.AddEmpty(3);
            items[0] = stack0;
            items[1] = stack1;
            items[2] = stack2;
            Set(index, items, 3);
        }

        /// <summary>
        /// insert an item to the data holder.the data holder must have StackCount as 1 , otherwise an exception is thrown
        /// </summary>
        /// <param name="index"></param>
        /// <param name="stack0"></param>
        public void Insert(int index, GenericDataItem stack0)
        {
            mTmpStackItems.Clear();
            GenericDataItem[] items = mTmpStackItems.AddEmpty(1);
            items[0] = stack0;
            Insert(index, items, 1);
        }

        /// <summary>
        ///  insert an item to the data holder.the data holder must have StackCount as 2 , otherwise an exception is thrown
        /// </summary>
        /// <param name="index"></param>
        /// <param name="stack0"></param>
        /// <param name="stack1"></param>
        public void Insert(int index, GenericDataItem stack0, GenericDataItem stack1)
        {
            mTmpStackItems.Clear();
            GenericDataItem[] items = mTmpStackItems.AddEmpty(2);
            items[0] = stack0;
            items[1] = stack1;
            Insert(index, items, 2);
        }

        /// <summary>
        ///  insert an item to the data holder.the data holder must have StackCount as 3 , otherwise an exception is thrown
        /// </summary>
        /// <param name="index"></param>
        /// <param name="stack0"></param>
        /// <param name="stack1"></param>
        /// <param name="stack2"></param>
        public void Insert(int index, GenericDataItem stack0, GenericDataItem stack1, GenericDataItem stack2)
        {
            mTmpStackItems.Clear();
            GenericDataItem[] items = mTmpStackItems.AddEmpty(3);
            items[0] = stack0;
            items[1] = stack1;
            items[2] = stack2;
            Insert(index,items, 3);
        }

        /// <summary>
        // inserts an item to the data holder.the params array must have exacly StackCount elements, otherwise an exception is thrown
        /// </summary>
        /// <param name="index"></param>
        /// <param name="items"></param>
        public void Insert(int index, params GenericDataItem[] items)
        {
            Insert(index, items, items.Length);
        }

        /// <summary>
        /// appends an item to the data holder. the params array must have exacly StackCount elements , otherwise an exception is thrown
        /// </summary>
        /// <param name="items"></param>
        public void Append(params GenericDataItem[] items)
        {
            Append(items, items.Length);
        }

        public DoubleRect BoundPositions(int stackIndex)
        {
            ValidateStackIndex(stackIndex);
            CommitChanges(); // commit all changes first         
            var stack = mStacks[stackIndex];
            DoubleRect bound = DoubleRect.CreateNan();
            for (int i=0; i<stack.Holder.Count; i++)
                bound.UnionVector(stack.Holder.GetPositionUncheked(i));
            return bound;
        }

        public DoubleRect BoundLastPositions(int count)
        {
            CommitChanges(); // commit all changes first        
            DoubleRect bound = DoubleRect.CreateNan();
            for (int stackIndex = 0; stackIndex < mStacks.Count; stackIndex++)
            {
                var stack = mStacks[stackIndex];
                for (int i = stack.Holder.Count - count; i < stack.Holder.Count; i++)
                    bound.UnionVector(stack.Holder.GetPositionUncheked(i));
            }
            return bound;
        }
        public DoubleRect BoundPositions()
        {
            CommitChanges(); // commit all changes first        
            DoubleRect bound = DoubleRect.CreateNan();
            for (int stackIndex = 0; stackIndex < mStacks.Count; stackIndex++)
            {
                var stack = mStacks[stackIndex];            
                for (int i = 0; i < stack.Holder.Count; i++)
                    bound.UnionVector(stack.Holder.GetPositionUncheked(i));
            }
            return bound;
        }
        public GenericDataItem GetValue(int stackIndex,int index)
        {
            ValidateStackIndex(stackIndex);
            return mStacks[stackIndex].GetValue(index);
        }

        //public void AppendPositionArray(int stackIndex, DoubleVector3[] array,int count)
        //{
        //    ValidateStackIndex(stackIndex);
        //    CommitChanges(); // commit all changes first
        //    mStacks[stackIndex].AppendPositionArray(array, count);
        //}

        bool CheckArraySorted(DoubleVector3[] array,int start,int count)
        {
            int end = start + count;
            double prev = array[start].x;
            for (int i = start + 1; i < end; i++)
            {
                double x = array[i].x;
                if (prev > x)
                    return false;
                prev = x;
            }
            return true;
        }

        public void SetPositionArray(int stackIndex, DoubleVector3[] array, int start, int count)
        {
            if (start < 0 || array.Length < start + count)
                throw new IndexOutOfRangeException("SetPositionArray is called with invalid start or count");
            if (CheckArraySorted(array, start, count) == false)
                throw new ArgumentException("SetPositionArray - Array not sorted along the x axis");
            ValidateStackIndex(stackIndex);
            CommitChanges(); // commit all changes first
            mStacks[stackIndex].SetPositionArray(array, start, count);
            MaintainCount();
            InvokeOnSetArray(ChannelType.Positions);
        }
        public IEnumerator LoadAsync(int stackIndex, DoubleVector3[] array, int count, int LoadEachFrame = 50000)
        {
            var tmpArr = new DoubleVector3[LoadEachFrame];
            int tmpArrCount = 0;
            for (int i = 0; i < count; i++)
            {

                tmpArr[tmpArrCount++] = array[i];
                if (tmpArrCount >= tmpArr.Length)
                {
                    AppendPositionArray(0, tmpArr, tmpArrCount);
                    tmpArrCount = 0;
                    yield return 0;
                }
            }
            if (tmpArrCount > 0)
                AppendPositionArray(0, tmpArr, tmpArrCount);
        }

        public void CachedAppendPosition(int stackIndex,DoubleVector3 v)
        {
            double x = double.NegativeInfinity;
            if(mCachedAppendCount == 0)
            {
                var holder = mStacks[0].Holder;
                if (holder.Count > 0)
                    x = holder.RawPositionArray()[holder.Count - 1].x;
            }
            else
                x = mCachedAppendArray[mCachedAppendCount - 1].x;

            if (x > v.x)
            {
                ChartCommon.RuntimeWarning("Graph data must be sorted along the x axis, append opetation is ignored");
                return;
            }

            //Count++;
            mCachedAppendArray[mCachedAppendCount++] = v;
            if (mCachedAppendCount == mCachedAppendArray.Length)
                CommitCachedArray();
        }

        void CommitCachedArray()
        {
            if (mCachedAppendCount > 0)
            {
                int count = mCachedAppendCount;
                mCachedAppendCount = 0;
                AppendPositionArray(0, mCachedAppendArray, count);
                
            }
        }

        public void AppendPositionArray(int stackIndex,DoubleVector3[] array,int count)
        {
            ValidateStackIndex(stackIndex);
            //this line is marked as comment to avoid stackoverflow when calling CommitCachedArray(). this should normally be enabled
            //CommitChanges(); // commit all changes first
            mStacks[stackIndex].AppendPositionArray(array, count);
            MaintainCount();
            InvokeAppendArray(ChannelType.Positions,count);
        }

        public void SetValue(int stackIndex, int index, GenericDataItem item)
        {
            ValidateStackIndex(stackIndex);
            mStacks[stackIndex].SetValue(index, item);
        }

        public void SetPosition(int stackIndex,int index,DoubleVector3 position)
        {
            ValidateStackIndex(stackIndex);
            mStacks[stackIndex].SetPosition(index, position);
        }

        public void SetEndPosition(int stackIndex, int index, DoubleVector3 endPosition)
        {
            ValidateStackIndex(stackIndex);
            mStacks[stackIndex].SetEndPosition(index, endPosition);
        }

        public void SetStartEndRange(int stackIndex, int index, DoubleRange startEnd)
        {
            ValidateStackIndex(stackIndex);
            mStacks[stackIndex].SetStartEndRange(index, startEnd);
        }

        public void SetHighLowRange(int stackIndex,int index,DoubleRange highLow)
        {
            ValidateStackIndex(stackIndex);
            mStacks[stackIndex].SetHighLowRange(index, highLow);
        }

        public void SetErrorRange(int stackIndex, int index, DoubleRange error)
        {
            ValidateStackIndex(stackIndex);
            mStacks[stackIndex].SetErrorRange(index, error);
        }

        public void SetName(int stackIndex, int index, string name)
        {
            ValidateStackIndex(stackIndex);
            mStacks[stackIndex].SetName(index, name);
        }

        public void SetUserData(int stackIndex,int index, object userData)
        {
            ValidateStackIndex(stackIndex);
            mStacks[stackIndex].SetUserData(index, userData);
        }

        public void SetSize(int stackIndex, int index, double size)
        {
            ValidateStackIndex(stackIndex);
            mStacks[stackIndex].SetSize(index, size);
        }

        public void Set(int index, params GenericDataItem[] items)
        {
            Set(index, items, items.Length);
        }

        private void Set(int index, GenericDataItem[] items, int count)
        {
            ValidateIndex(index);
            ValidateStacks(count);

            if (mOperations.NodeCount > 0 || mCachingOperations)
            {
                int newIndex = mStacks[0].NewOperations.Count;
                for (int i = 0; i < count; i++)
                    mStacks[i].NewOperations.AppendValue(items[i]);
                mOperations.SetOp(index, newIndex);
                MaintainCount();
                InvokeDataChanged();
            }
            else
            {
                if (InnerOnBeforeSet != null)
                    InnerOnBeforeSet(this, index);
                for (int i = 0; i < count; i++)
                    mStacks[i].Holder.SetValue(index, items[i]);
                MaintainCount();
                if (InnerOnSet != null)
                    InnerOnSet(this, index);
            }
           
        }
        public void CacheOperations()
        {
            mCachingOperations = true;
        }

        private void Insert(int index,GenericDataItem[] items,int count)
        {
            ValidateIndexInclusive(index);
            ValidateStacks(count);
            bool createOpTree = index  + CreateTreeThreshold  < Count;  // if the index is too far from the end of the list create an op tree
            if (mOperations.NodeCount > 0 || createOpTree || mCachingOperations)
            {
                int newIndex = mStacks[0].NewOperations.Count;
                for (int i = 0; i < count; i++)
                    mStacks[i].NewOperations.AppendValue(items[i]);
                mOperations.InsertOp(index, newIndex);
                MaintainCount();
                InvokeDataChanged();
            }
            else
            {
                if (InnerOnBeforeInsert != null)
                    InnerOnBeforeInsert(this, index);
                for (int i = 0; i < count; i++)
                    mStacks[i].Holder.InsertValue(index,items[i]);
                MaintainCount();
                if (InnerOnInsert != null)
                    InnerOnInsert(this, index);
            }
            
        }

        protected void Append(GenericDataItem[] items,int count)
        {
            Insert(Count, items, count);
            //ValidateStacks(count);
            //if (mOperations.NodeCount > 0)
            //{
            //    int index = mStacks[0].NewOperations.Count;
            //    for(int i=0; i< count; i++)
            //        mStacks[i].NewOperations.AppendValue(items[i]);
            //    mOperations.InsertOp(Count, index);
            //    MaintainCount();
            //    InvokeDataChanged();
            //}
            //else
            //{
            //    int preCount = Count;
            //    if (InnerOnBeforeInsert != null)
            //        InnerOnBeforeInsert(this, preCount);
            //    for (int i = 0; i < count; i++)
            //        mStacks[i].Holder.AppendValue(items[i]);
            //    MaintainCount();
            //    if (InnerOnAppend != null)
            //        InnerOnAppend(this, preCount);
            //}
        }

        private class BinarySearchComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                double xx = ((DoubleVector3)x).x;
                double yx = ((DoubleVector3)y).x;
                if (xx < yx)
                    return -1;
                if (yx < xx)
                    return 1;
                return 0;
            }
        }

        public DoubleVector3 GetPointAt(int index)
        {
            CommitChanges();
            return RawPositionArray(0)[index];
        }

        public void RemoveAllBefore(double x)
        {
            CommitChanges();
            var holder =  mStacks[0].Holder;
            var positions = holder.RawPositionArray();
            int index = Array.BinarySearch(positions, 0, holder.Count, new DoubleVector3(x, 0, 0), new BinarySearchComparer());
            if (index < 0)
                index = ~index;
            if (index > 0)
                index--;
            RemoveFromStart(index); // remove everything before the index
        }

        public void RemoveFromStart(int count)
        {
            ValidateIndex(count);
            CommitChanges();
            for (int i = 0; i < mStacks.Count; i++)
                mStacks[i].Holder.RemoveFromStart(count);
            MaintainCount();
            if(InnerOnRemoveFromStart != null)
                InnerOnRemoveFromStart(this, count);
        }

        public void RemoveFromEnd(int count)
        {
            ValidateIndex(count);
            CommitChanges();
            for (int i = 0; i < mStacks.Count; i++)
                mStacks[i].Holder.RemoveFromEnd(count);
            MaintainCount();
            if (InnerOnRemoveFromEnd != null)
                InnerOnRemoveFromEnd(this, count);
        }

        public void Remove(int index)
        {
            ValidateIndex(index);
            bool createOpTree = index + CreateTreeThreshold < Count;  // if the index is too far from the end of the list create an op tree
            if (mOperations.NodeCount > 0 || createOpTree || mCachingOperations)
            {
                mOperations.RemoveOp(index);
                MaintainCount();
                InvokeDataChanged();
            }
            else
            {
                if (InnerOnBeforeRemove != null)
                    InnerOnBeforeRemove(this, index);
                for (int i = 0; i < mStacks.Count; i++)
                    mStacks[i].Holder.Remove(index);
                MaintainCount();
                if (InnerOnRemove != null)
                    InnerOnRemove(this, index);
            }        
        }

        void EnsureChannels()
        {
            for (int i = 0; i < mStacks.Count; i++)
                mStacks[i].EnsureChannels(mChannels);
        }

        public void CommitChanges()
        {
            CommitCachedArray();
            //mCachingOperations = false;
            //if (mOperations.NodeCount <= 0) // the tree is empty
            //    return;
            //if (InnerBeforeCommit != null)
            //    InnerBeforeCommit(this, mOperations);
            //for (int i=0; i<mStacks.Count; i++)
            //{
            //    mStacks[i].ApplyOperationTree(mOperations);
            //}
            //var opTree = mOperations;   // store the operation tree so we can pass it to the event
            //mOperations = EmptyTree;    // set the current operation tree to the empty one when calling the events. this will make access to the stackGenericDataHolder work without issues.
            //MaintainCount();        
            //if (InnerOnCommit != null)
            //    InnerOnCommit(this, opTree);
            //mOperations = opTree; // set operations back to the original tree
            //mOperations.Clear();    // clear the operations

        }

        StackDataHolder CreateNewStackItem()
        {
            CommitChanges();
            StackDataHolder stack = new StackDataHolder(this);
            stack.EnsureChannels(mChannels);
            stack.Holder.AddEmpty(Count);   // set the new stack to the same size of the current stack
            return stack;
        }

        public void ClearStacks()
        {
            Clear();         
            mStacks.Clear();
            AddStack();
        }

        /// <summary>
        /// clears all data of this stack data holder
        /// </summary>
        public void Clear()
        {
            mOperations.Clear();
            for(int i=0; i<mStacks.Count; i++)
            {
                mStacks[i].Holder.Clear();
                mStacks[i].NewOperations.Clear();
            }
            MaintainCount();
            if (InnerOnClear != null)
                InnerOnClear(this);
        }

        public void InsertStack(int index)
        {
            StackDataHolder stack = CreateNewStackItem();
            mStacks.Insert(index,stack);
        }

        public void RemoveStack(int index)
        {
            mStacks.RemoveAt(index);
        }

        public void AddStack()
        {
            StackDataHolder stack = CreateNewStackItem();
            mStacks.Add(stack);
        }


        public DoubleVector3[] RawPositionArray(int stack)
        {
            return mStacks[stack].Holder.RawPositionArray();
        }

        public DoubleVector3[] RawEndPositionArray(int stack)
        {
            return mStacks[stack].Holder.RawEndPositionArray();
        }

        public double[] RawSizeArray(int stack)
        {
            return mStacks[stack].Holder.RawSizeArray();
        }

        public DoubleRange[] RawStartEndArray(int stack)
        {
            return mStacks[stack].Holder.RawStartEndArray();
        }

        public DoubleRange[] RawHighLowArray(int stack)
        {
            return mStacks[stack].Holder.RawHighLowArray();
        }

        public DoubleRange[] RawErrorRangeArray(int stack)
        {
            return mStacks[stack].Holder.RawErrorRangeArray();
        }

        public DoubleRect?[] RawBoundingVolume(int stack)
        {
            return mStacks[stack].Holder.RawBoundingVolume();
        }

        public object[] RawUserDataArray(int stack)
        {
            return mStacks[stack].Holder.RawUserDataArray();
        }

        public string[] RawNameArray(int stack)
        {
            return mStacks[stack].Holder.RawNameArray();
        }

        public Color32[] RawColorArray(int stack)
        {
            return mStacks[stack].Holder.RawColorArray();
        }

        public DataBounds DataBounds(int stack)
        {
            return ((IPrivateDataSeriesCategory)mParentCategory).Bounds;
        }



        public virtual int[] RawViewArray()
        {
            return null;
        }
    }
}
