using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThetaList;

namespace DataVisualizer{
    public partial class StackedGenericDataHolder
    {
        /// <summary>
        /// holds information of one stack item. it holds a cache for new operations that can be applied at once in an efficient way
        /// </summary>
        class StackDataHolder
        {
            DataBounds mBounds = new DataBounds();

            /// <summary>
            /// the parent of the instances
            /// </summary>
            StackedGenericDataHolder mParent;

            /// <summary>
            /// holds the data of the instance
            /// </summary>
            public GenericDataHolder Holder { get; private set; }

            /// <summary>
            /// holds new operations
            /// </summary>
            public GenericDataHolder NewOperations { get; private set; }

            /// <summary>
            /// optimization conditions used by data series in order to elevate performance
            /// </summary>
            List<IDataSeriesOptimizationCondition> mOptimizationConditions = new List<IDataSeriesOptimizationCondition>();

            /// <summary>
            /// creates a new stackDataholder with a parent
            /// </summary>
            /// <param name="parent"></param>
            public StackDataHolder(StackedGenericDataHolder parent)
            {
                mParent = parent;
                Holder = new GenericDataHolder();
                NewOperations = new GenericDataHolder();
            }

            /// <summary>
            /// applies a tree of cached operations on this object.
            /// </summary>
            /// <param name="tree"></param>
            public void ApplyOperationTree(OperationTree<int> tree)
            {
                Holder.ApplyOperationTree(tree, NewOperations);
                NewOperations.Clear();
            }

            /// <summary>
            /// gets a value from the data holders
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public GenericDataItem GetValue(int index)
            {
                mParent.ValidateIndex(index);
                int newOpIndex, currentIndex;
                if (mParent.mOperations.Find(index, out newOpIndex, out currentIndex))
                    return NewOperations.GetValue(newOpIndex);
                return Holder.GetValue(currentIndex);
            }

            //public void SetPositionArrayWithDownSampling(DoubleVector3[] array, int start, int count)
            //{
            //    Holder.ValidateChannel(ChannelType.Positions);
            //    if (Holder.Count < start + count)
            //        throw new InvalidOperationException("the array is larger then the data stored in the holder");
            //    if(array.Length < count)
            //         throw new InvalidOperationException("the array is smalled then the downsample data size");
            //    DoubleVector3[] holderArr = Holder.GetRawPositionArray();
            //    float pointsPerItem = array.Length / count;

            //    float fromPoint = 0f;
            //    float toPoint = pointsPerItem;

            //    for(float i=0; i<count; i++)
            //    {                    
            //        currentPoint += pointsPerItem;
            //    }
            //    mParent.InvokeOnSetArray(ChannelType.Positions);
            //}

            public void AppendPositionArray(DoubleVector3[] array, int count)
            {
                Holder.ValidateChannel(ChannelType.Positions);
                int prevCount = Holder.Count;
                Holder.AddEmpty(count);
                DoubleVector3[] holderArr = Holder.GetRawPositionArray();
                Array.Copy(array, 0, holderArr, prevCount, count);
            }

            public void SetPositionArray(DoubleVector3[] array,int start, int count)
            {
                Holder.ValidateChannel(ChannelType.Positions);
                if (Holder.Count < count)
                    Holder.AddEmpty(count - Holder.Count);
                else if (Holder.Count > count)
                    Holder.RemoveFromEnd(Holder.Count - count);
                DoubleVector3[] holderArr = Holder.GetRawPositionArray();
                Array.Copy(array, start, holderArr, 0, count);
            }

            /// <summary>
            /// sets a value for the stack at the specified index. All channel are replaced
            /// </summary>
            /// <param name="index"></param>
            /// <param name="item"></param>
            public void SetValue(int index, GenericDataItem item)
            {
                mParent.ValidateIndex(index);
                int newOpIndex, currentIndex;
                if (mParent.mOperations.Find(index, out newOpIndex, out currentIndex))
                    NewOperations.SetValue(newOpIndex, item);
                else
                {
                    mParent.InvokeOnBeforeSet(currentIndex);
                    Holder.SetValue(currentIndex, item);
                    mParent.InvokeOnSet(currentIndex);
                }
            }

            /// <summary>
            /// sets the size for the stack item at the specified index
            /// </summary>
            /// <param name="index"></param>
            /// <param name="size"></param>
            public void SetSize(int index, double size)
            {
                mParent.ValidateIndex(index);
                int newOpIndex, currentIndex;
                if (mParent.mOperations.Find(index, out newOpIndex, out currentIndex))
                    NewOperations.SetSize(newOpIndex, size);
                else
                {
                    mParent.InvokeOnBeforeSet(currentIndex);
                    Holder.SetSize(currentIndex, size);
                    mParent.InvokeOnSet(currentIndex);
                }
            }
            /// <summary>
            /// sets the name for the stack item at the specified index
            /// </summary>
            /// <param name="index"></param>
            /// <param name="name"></param>
            public void SetName(int index, string name)
            {
                mParent.ValidateIndex(index);
                int newOpIndex, currentIndex;
                if (mParent.mOperations.Find(index, out newOpIndex, out currentIndex))
                    NewOperations.SetName(newOpIndex, name);
                else
                {
                    mParent.InvokeOnBeforeSet(currentIndex);
                    Holder.SetName(currentIndex, name);
                    mParent.InvokeOnSet(currentIndex);
                }
            }
            /// <summary>
            /// sets the user data for the stack item at the specified index
            /// </summary>
            /// <param name="index"></param>
            /// <param name="data"></param>
            public void SetUserData(int index, object data)
            {
                mParent.ValidateIndex(index);
                int newOpIndex, currentIndex;
                if (mParent.mOperations.Find(index, out newOpIndex, out currentIndex))
                    NewOperations.SetUserData(newOpIndex, data);
                else
                {
                    mParent.InvokeOnBeforeSet(currentIndex);
                    Holder.SetUserData(currentIndex, data);
                    mParent.InvokeOnSet(currentIndex);
                }
            }

            /// <summary>
            /// sets the error range for the stack item at the specified index
            /// </summary>
            /// <param name="index"></param>
            /// <param name="errorRange"></param>
            public void SetErrorRange(int index, DoubleRange errorRange)
            {
                mParent.ValidateIndex(index);
                int newOpIndex, currentIndex;
                if (mParent.mOperations.Find(index, out newOpIndex, out currentIndex))
                    NewOperations.SetErrorRange(newOpIndex, errorRange);
                else
                {
                    mParent.InvokeOnBeforeSet(currentIndex);
                    Holder.SetErrorRange(currentIndex, errorRange);
                    mParent.InvokeOnSet(currentIndex);
                }
            }

            /// <summary>
            /// sets the high low range for the stack item at the specified index
            /// </summary>
            /// <param name="index"></param>
            /// <param name="highLow"></param>
            public void SetHighLowRange(int index, DoubleRange highLow)
            {
                mParent.ValidateIndex(index);
                int newOpIndex, currentIndex;
                if (mParent.mOperations.Find(index, out newOpIndex, out currentIndex))
                    NewOperations.SetHighLow(newOpIndex, highLow);
                else
                {
                    mParent.InvokeOnBeforeSet(currentIndex);
                    Holder.SetHighLow(currentIndex, highLow);
                    mParent.InvokeOnSet(currentIndex);
                }
            }

            /// <summary>
            /// sets the start end range for the stack item at the specified index
            /// </summary>
            /// <param name="index"></param>
            /// <param name="startEnd"></param>
            public void SetStartEndRange(int index, DoubleRange startEnd)
            {
                mParent.ValidateIndex(index);
                int newOpIndex, currentIndex;
                if (mParent.mOperations.Find(index, out newOpIndex, out currentIndex))
                    NewOperations.SetStartEnd(newOpIndex, startEnd);
                else
                {
                    mParent.InvokeOnBeforeSet(currentIndex);
                    Holder.SetStartEnd(currentIndex, startEnd);
                    mParent.InvokeOnSet(currentIndex);
                }
            }

            /// <summary>
            /// sets the end position for the stack item at the specified index
            /// </summary>
            /// <param name="index"></param>
            /// <param name="endPosition"></param>
            public void SetEndPosition(int index, DoubleVector3 endPosition)
            {
                mParent.ValidateIndex(index);
                int newOpIndex, currentIndex;
                if (mParent.mOperations.Find(index, out newOpIndex, out currentIndex))
                    NewOperations.SetEndPosition(newOpIndex, endPosition);
                else
                {
                    mParent.InvokeOnBeforeSet(currentIndex);
                    Holder.SetEndPosition(currentIndex, endPosition);
                    mParent.InvokeOnSet(currentIndex);
                }
            }

            /// <summary>
            /// sets the position for the stack item at the specified index
            /// </summary>
            /// <param name="index"></param>
            /// <param name="position"></param>
            public void SetPosition(int index, DoubleVector3 position)
            {
                mParent.ValidateIndex(index);
                int newOpIndex, currentIndex;
                if (mParent.mOperations.Find(index, out newOpIndex, out currentIndex))
                    NewOperations.SetPosition(newOpIndex, position);
                else
                {
                    mParent.InvokeOnBeforeSet(currentIndex);
                    Holder.SetPosition(currentIndex, position);
                    mParent.InvokeOnSet(currentIndex);
                }
            }

            public void EnsureChannels(ChannelType channel)
            {
                Holder.EnsureChannels(channel);
                NewOperations.EnsureChannels(channel);
            }
        }
    }
}
