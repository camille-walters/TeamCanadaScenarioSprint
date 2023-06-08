using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThetaList;

namespace DataVisualizer{
    public class TextDataHolder
    {
        SimpleList<DoubleVector3> mPositions = new SimpleList<DoubleVector3>();
        SimpleList<double> mSizes = new SimpleList<double>();
        DataBounds mBounds = new DataBounds();
        public DoubleVector3[] RawPositions { get { return mPositions.RawArrayWithExtraLastItem; } }
        public double[] RawSizes { get { return mSizes.RawArrayWithExtraLastItem; } }

        public event Action<object, int> OnBeforeSet;
        public event Action<object, int> OnSet;
        public event Action<object, int> OnInsert;
        public event Action<object, int> OnBeforeInsert;
        public event Action<object, int> OnBeforeRemove;
        public event Action<object, int> OnRemove;
        public event Action<object> OnClear;

        public int Count { get { return mPositions.Count; } }
        public DataBounds Bounds { get { return mBounds; } }
        public AxisDimension Direction {get; set;}
        protected void RaiseOnInsert(int index)
        {
            if (OnInsert != null)
                OnInsert(this, index);
        }

        protected void RaiseOnClear()
        {
            if (OnClear != null)
                OnClear(this);
        }

        protected void RaiseOnBeforeInsert(int index)
        {
            if (OnBeforeInsert != null)
                OnBeforeInsert(this, index);
        }
        protected void RaiseOnBeforeSet(int index)
        {
            if (OnBeforeSet != null)
                OnBeforeSet(this, index);
        }

        protected void RaiseOnSet(int index)
        {
            if (OnSet != null)
                OnSet(this, index);
        }

        protected void RaiseOnBeforeRemove(int index)
        {
            if (OnBeforeRemove != null)
                OnBeforeRemove(this, index);
        }

        protected void RaiseOnRemove(int index)
        {
            if (OnRemove != null)
                OnRemove(this, index);
        }

        public void ClearTexts()
        {
            mBounds = new DataBounds();
            mPositions.Clear();
            mSizes.Clear();
            RaiseOnClear();
        }

        public void RemoveText(int index)
        {
            RaiseOnBeforeRemove(index);
            mPositions.RemoveAt(index);
            mSizes.RemoveAt(index);
            RaiseOnRemove(index);
        }

        public int AddText(DoubleVector3 position, double value)
        {
            mBounds.ModifyMinMax(position);
            int index = mPositions.Count;
            RaiseOnBeforeInsert(index);
            mPositions.Add(position);
            mSizes.Add(value);
            RaiseOnInsert(index);
            return index;
        }

        public void ModifyText(int index, double value, DoubleVector3 position)
        {
            RaiseOnBeforeSet(index);
            mPositions[index] = position;
            mSizes[index] = value;
            RaiseOnSet(index);
        }

    }
}
