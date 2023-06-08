using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThetaList;
using UnityEngine;

namespace DataVisualizer{
    public abstract class AxisLineDataGenerator : AxisDataGenerator
    {
        SimpleList<DoubleVector3> mPositions = new SimpleList<DoubleVector3>();
        SimpleList<DoubleVector3> mEndPositions = new SimpleList<DoubleVector3>();
        AxisDimension mDirection;

        DataBounds mBounds = new DataBounds();

        protected AxisDimension Direction
        {
            get { return mDirection; }
        }

        public AxisLineDataGenerator(string name,GameObject obj) : base(name,obj)
        {

        }

        public override int Count { get { return mPositions.Count; } }

        public override ChannelType CurrentChannels { get { return ChannelType.Positions | ChannelType.EndPositions; } }

        public override DoubleVector3[] RawPositionArray(int stack)
        {
            return mPositions.RawArrayWithExtraLastItem;
        }

        public override DoubleVector3[] RawEndPositionArray(int stack)
        {
            return mEndPositions.RawArrayWithExtraLastItem;
        }

        public override bool ValidateSettings(IDataSeriesSettings settings, out string error)
        {
            error = null;
            UnboxSetting<AxisDimension>(ref mDirection, settings, "dimension", AxisDimension.X, DataSeriesRefreshType.FullRefresh);
            return true;
        }

        public void ModifyLine(int index, DoubleVector3 startPosition, DoubleVector3 endPosition)
        {
            RaiseOnBeforeSet(index);
            mPositions[index] = startPosition;
            mEndPositions[index] = endPosition;
            RaiseOnSet(index);

        }

        public void ClearLines()
        {
            mBounds = new DataBounds();
            mPositions.Clear();
            mEndPositions.Clear();
            RaiseOnClear();
        }

        public int AddLine(DoubleVector3 startPosition,DoubleVector3 endPosition)
        {
            int index = mPositions.Count;
            RaiseOnBeforeInsert(index);
            mPositions.Add(startPosition);
            mEndPositions.Add(endPosition);
            RaiseOnInsert(index);
            mBounds.ModifyMinMax(startPosition);
            mBounds.ModifyMinMax(endPosition);
            return index;
        }

        public override DataBounds DataBounds(int stack)
        {
            return mBounds;
        }
        public void RemoveLine(int index)
        {
            RaiseOnBeforeRemove(index);
            mPositions.RemoveAt(index);
            mEndPositions.RemoveAt(index);
            RaiseOnRemove(index);
        }

    }
}
