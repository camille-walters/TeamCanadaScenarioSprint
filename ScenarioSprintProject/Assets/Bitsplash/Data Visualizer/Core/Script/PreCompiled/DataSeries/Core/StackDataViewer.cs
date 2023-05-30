using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    public class StackDataViewer
    {
        private IDataViewer mViewer;
        DataBounds mBounds;
        public IDataArray<DoubleVector3> RawPositionArray;    // public field. For faster execution regardless of mono optimiztions. should not be changed
        public IDataArray<DoubleVector3> RawEndPositionArray;   // public field. For faster execution regardless of mono optimiztions. should not be changed
        public IDataArray<double> RawSizeArray;   // public field. For faster execution regardless of mono optimiztions. should not be changed
        public IDataArray<DoubleRange> RawStartEndArray;  // public field. For faster execution regardless of mono optimiztions. should not be changed
        public IDataArray<DoubleRange> RawHighLowArray;  // public field. For faster execution regardless of mono optimiztions. should not be changed
        public IDataArray<DoubleRange> RawErrorRangeArray;  // public field. For faster execution regardless of mono optimiztions. should not be changed
        public IDataArray<DoubleRect?> RawBoundingVolume;  // public field. For faster execution regardless of mono optimiztions. should not be changed
        public IDataArray<object> RawUserDataArray;  // public field. For faster execution regardless of mono optimiztions. should not be changed
        public IDataArray<string> RawNameArray;  // public field. For faster execution regardless of mono optimiztions. should not be changed
        public IDataArray<Color32> RawColorArray;
        public int[] RawViewArray;
        public int SubArrayStart;
        public int Count { get { return mViewer.Count; } }
        public ChannelType CurrentChannels { get { return mViewer.CurrentChannels; } }
        bool? mHasView = null;
        protected StackDataViewer()
        {
            mViewer = null;
        } 
        
        public DataBounds Bounds
        {
            get
            {
                return mBounds;
            }
        }

        public StackDataViewer(IDataViewer viewer)
        {
            mViewer = viewer;
        }

        protected void CopyFrom(StackDataViewer stackViewer)
        {
            mViewer = stackViewer.mViewer;
            RawPositionArray = stackViewer.RawPositionArray;
            RawEndPositionArray = stackViewer.RawEndPositionArray;
            RawSizeArray = stackViewer.RawSizeArray;
            RawStartEndArray = stackViewer.RawStartEndArray;
            RawHighLowArray = stackViewer.RawHighLowArray;
            RawErrorRangeArray = stackViewer.RawErrorRangeArray;
            RawBoundingVolume = stackViewer.RawBoundingVolume;
            RawUserDataArray = stackViewer.RawUserDataArray;
            RawNameArray = stackViewer.RawNameArray;
            RawColorArray = stackViewer.RawColorArray;
            SubArrayStart = stackViewer.SubArrayStart;

        }
        
        public String Name
        {
            get
            {
                if (mViewer == null)
                    return "";
                return mViewer.Name;
            }
        }

        public void SetStack(int stack)
        {
            mBounds = mViewer.DataBounds(stack);
            var viewArray = mViewer.RawViewArray();
            SubArrayStart = mViewer.SubArrayOffset;
            RawViewArray = viewArray;
            if (mHasView.HasValue == false || (mHasView.Value == true && viewArray == null) || (mHasView==false && viewArray!=null))
            {
                if (viewArray == null)
                {
                    mHasView = false;
                    RawPositionArray = new SimpleDataArray<DoubleVector3>(this, mViewer.RawPositionArray(stack));
                    RawColorArray = new SimpleDataArray<Color32>(this, mViewer.RawColorArray(stack));
                    RawEndPositionArray = new SimpleDataArray<DoubleVector3>(this, mViewer.RawEndPositionArray(stack));
                    RawSizeArray = new SimpleDataArray<double>(this, mViewer.RawSizeArray(stack));
                    RawStartEndArray = new SimpleDataArray<DoubleRange>(this, mViewer.RawStartEndArray(stack));
                    RawHighLowArray = new SimpleDataArray<DoubleRange>(this, mViewer.RawHighLowArray(stack));
                    RawErrorRangeArray = new SimpleDataArray<DoubleRange>(this, mViewer.RawErrorRangeArray(stack));
                    RawBoundingVolume = new SimpleDataArray<DoubleRect?>(this, mViewer.RawBoundingVolume(stack));
                    RawUserDataArray = new SimpleDataArray<object>(this, mViewer.RawUserDataArray(stack));
                    RawNameArray = new SimpleDataArray<string>(this, mViewer.RawNameArray(stack));
                }
                else
                {
                    mHasView = true;
                    RawPositionArray = new ViewableDataArray<DoubleVector3>(this, mViewer.RawPositionArray(stack));
                    RawColorArray = new ViewableDataArray<Color32>(this, mViewer.RawColorArray(stack));
                    RawEndPositionArray = new ViewableDataArray<DoubleVector3>(this, mViewer.RawEndPositionArray(stack));
                    RawSizeArray = new ViewableDataArray<double>(this, mViewer.RawSizeArray(stack));
                    RawStartEndArray = new ViewableDataArray<DoubleRange>(this, mViewer.RawStartEndArray(stack));
                    RawHighLowArray = new ViewableDataArray<DoubleRange>(this, mViewer.RawHighLowArray(stack));
                    RawErrorRangeArray = new ViewableDataArray<DoubleRange>(this, mViewer.RawErrorRangeArray(stack));
                    RawBoundingVolume = new ViewableDataArray<DoubleRect?>(this, mViewer.RawBoundingVolume(stack));
                    RawUserDataArray = new ViewableDataArray<object>(this, mViewer.RawUserDataArray(stack));
                    RawNameArray = new ViewableDataArray<string>(this, mViewer.RawNameArray(stack));
                }
            }
            else
            {
                RawPositionArray.Set(mViewer.RawPositionArray(stack));
                RawColorArray.Set(mViewer.RawColorArray(stack));
                RawEndPositionArray.Set(mViewer.RawEndPositionArray(stack));
                RawSizeArray.Set(mViewer.RawSizeArray(stack));
                RawStartEndArray.Set(mViewer.RawStartEndArray(stack));
                RawHighLowArray.Set(mViewer.RawHighLowArray(stack));
                RawErrorRangeArray.Set(mViewer.RawErrorRangeArray(stack));
                RawBoundingVolume.Set(mViewer.RawBoundingVolume(stack));
                RawUserDataArray.Set(mViewer.RawUserDataArray(stack));
                RawNameArray.Set(mViewer.RawNameArray(stack));
            }
        }

        public abstract class IDataArray<T>
        {
            protected T[] mArray;
            protected StackDataViewer mParent;
            public IDataArray(StackDataViewer parent,T[] array)
            {
                mArray = array;
                mParent = parent;
            }
            public bool IsNull { get { return mArray == null; } }
            public void Set(T[] array)
            {
                mArray = array;
            }
            public abstract T Get(int index);
        }

        public class ViewableDataArray<T> : IDataArray<T>
        {
            public ViewableDataArray(StackDataViewer parent, T[] array)
                : base(parent, array)
            {
            }


            public override T Get(int index)
            {
                return mArray[mParent.SubArrayStart + mParent.RawViewArray[index]];
            }
        }
        public class SimpleDataArray<T> : IDataArray<T>
        {
            public SimpleDataArray(StackDataViewer parent, T[] array)
                : base(parent, array)
            {

            }

            public override T Get(int index)
            {
                return mArray[mParent.SubArrayStart + index];
            }
        }
    }
}
