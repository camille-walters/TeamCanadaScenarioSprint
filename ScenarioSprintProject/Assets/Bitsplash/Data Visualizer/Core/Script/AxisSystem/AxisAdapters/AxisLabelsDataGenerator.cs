using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThetaList;
using UnityEngine;

namespace DataVisualizer{
    /// <summary>
    /// this class extends AxisDataGenerator and adds label marking functionallity to it
    /// </summary>
    public abstract class AxisLabelsDataGenerator : AxisDataGenerator
    {

        TextDataHolder mHolder;
        protected TextDataHolder Holder {get {return mHolder;} }
        public AxisLabelsDataGenerator(string name,GameObject obj, TextDataHolder holder) 
            : base(name,obj)
        {
            mHolder = holder;
            HookHolder();
        }

        void UnhookHolder()
        {
            mHolder.OnBeforeInsert -= MHolder_OnBeforeInsert;
            mHolder.OnBeforeRemove -= MHolder_OnBeforeRemove;
            mHolder.OnBeforeSet -= MHolder_OnBeforeSet;
            mHolder.OnInsert -= MHolder_OnInsert;
            mHolder.OnSet -= MHolder_OnSet;
            mHolder.OnRemove -= MHolder_OnRemove;
            mHolder.OnClear -= MHolder_OnClear;
        }
        public override DataBounds DataBounds(int stack)
        {
            return mHolder.Bounds;
        }
        public override void Destroy()
        {
            UnhookHolder();
            base.Destroy();
        }

        void HookHolder()
        {
            UnhookHolder();
            mHolder.OnBeforeInsert += MHolder_OnBeforeInsert;
            mHolder.OnBeforeRemove += MHolder_OnBeforeRemove;
            mHolder.OnBeforeSet += MHolder_OnBeforeSet;
            mHolder.OnInsert += MHolder_OnInsert;
            mHolder.OnSet += MHolder_OnSet;
            mHolder.OnRemove += MHolder_OnRemove;
            mHolder.OnClear += MHolder_OnClear;
        }
        
        private void MHolder_OnRemove(object arg1, int index)
        {
            RaiseOnRemove(index);
        }

        private void MHolder_OnClear(object obj)
        {
            RaiseOnClear();
        }

        private void MHolder_OnSet(object arg1, int index)
        {
            RaiseOnSet(index);
        }

        private void MHolder_OnInsert(object arg1, int index)
        {
            RaiseOnInsert(index);
        }

        private void MHolder_OnBeforeSet(object arg1, int index)
        {
            RaiseOnBeforeSet(index);
        }

        private void MHolder_OnBeforeRemove(object arg1, int index)
        {
            RaiseOnBeforeRemove(index);
        }

        private void MHolder_OnBeforeInsert(object arg1, int index)
        {
            RaiseOnBeforeInsert(index);
        }
        
        public override int Count { get { return mHolder.Count; } }

        public override ChannelType CurrentChannels { get { return ChannelType.Positions | ChannelType.Sizes; } }

        public override DoubleVector3[] RawPositionArray(int stack)
        {
            return mHolder.RawPositions;
        }

        public override double[] RawSizeArray(int stack)
        {
            return mHolder.RawSizes;
        }
    }
}
