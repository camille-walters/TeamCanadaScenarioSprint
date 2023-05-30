using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThetaList;
using UnityEngine;

namespace DataVisualizer{
    /// <summary>
    /// holds generic data in a memory efficient way. allocating arrays only for channels that are required
    /// </summary>
    public class GenericDataHolder
    {
        /// <summary>
        /// the channels that are currently available in the list
        /// </summary>
        private ChannelType mCurrentChannels = ChannelType.None;
        SimpleList<string> mNames;
        SimpleList<DoubleVector3> mPositions;
        SimpleList<DoubleVector3> mEndPositions;
        SimpleList<DoubleRange> mStartEndRange;
        SimpleList<DoubleRange> mHighLowRange;
        SimpleList<DoubleRange> mErrorRange;
        SimpleList<double> mSizes;
        SimpleList<object> mUserData;
        SimpleList<DoubleRect?> mBoundingVolume;
        SimpleList<DoubleRect> mAlternativeBoundingVolume;
        SimpleList<Color32> mColors;

        public int Count { get; private set; }

        public void ApplyOperationTree(OperationTree<int> tree,GenericDataHolder newOps)
        {
            Count += tree.IndexBalance;
            if (newOps.mCurrentChannels != mCurrentChannels)
                throw new Exception(); // Illigal state
            if (mPositions != null)
                mPositions.ApplyOperations(tree, x => newOps.mPositions[x]);
            if (mEndPositions != null)
                mEndPositions.ApplyOperations(tree, x => newOps.mEndPositions[x]);
            if (mErrorRange != null)
                mErrorRange.ApplyOperations(tree, x => newOps.mErrorRange[x]);
            if (mHighLowRange != null)
                mHighLowRange.ApplyOperations(tree, x => newOps.mHighLowRange[x]);
            if (mStartEndRange != null)
                mStartEndRange.ApplyOperations(tree, x => newOps.mStartEndRange[x]);
            if (mUserData != null)
                mUserData.ApplyOperations(tree, x => newOps.mUserData[x]);
            if (mNames != null)
                mNames.ApplyOperations(tree, x => newOps.mNames[x]);
            if (mSizes != null)
                mSizes.ApplyOperations(tree, x => newOps.mSizes[x]);
            if (mColors != null)
                mColors.ApplyOperations(tree, x => newOps.mColors[x]);
        }
        public void RemoveFromEnd(int count)
        {
            if (count <= 0)
                return;
            Count -= Math.Min(count, Count);
            if (mPositions != null)
                mPositions.RemoveFromEnd(count);
            if (mEndPositions != null)
                mEndPositions.RemoveFromEnd(count);
            if (mErrorRange != null)
                mErrorRange.RemoveFromEnd(count);
            if (mHighLowRange != null)
                mHighLowRange.RemoveFromEnd(count);
            if (mStartEndRange != null)
                mStartEndRange.RemoveFromEnd(count);
            if (mUserData != null)
                mUserData.RemoveFromEnd(count);
            if (mNames != null)
                mNames.RemoveFromEnd(count);
            if (mSizes != null)
                mSizes.RemoveFromEnd(count);
            if (mColors != null)
                mColors.RemoveFromEnd(count);
        }
        public void RemoveFromStart(int count)
        {
            if (count <= 0)
                return;
            Count -=Math.Min(count,Count);
            if (mPositions != null)
                mPositions.RemoveFromStart(count);
            if (mEndPositions != null)
                mEndPositions.RemoveFromStart(count);
            if (mErrorRange != null)
                mErrorRange.RemoveFromStart(count);
            if (mHighLowRange != null)
                mHighLowRange.RemoveFromStart(count);
            if (mStartEndRange != null)
                mStartEndRange.RemoveFromStart(count);
            if (mUserData != null)
                mUserData.RemoveFromStart(count);
            if (mNames != null)
                mNames.RemoveFromStart(count);
            if (mSizes != null)
                mSizes.RemoveFromStart(count);
            if (mColors != null)
                mColors.RemoveFromStart(count);
        }

        public void Remove(int index)
        {
            Count--;
            if (mPositions != null)
                mPositions.RemoveAt(index);
            if (mEndPositions != null)
                mEndPositions.RemoveAt(index);
            if (mErrorRange != null)
                mErrorRange.RemoveAt(index);
            if (mHighLowRange != null)
                mHighLowRange.RemoveAt(index);
            if (mStartEndRange != null)
                mStartEndRange.RemoveAt(index);
            if (mUserData != null)
                mUserData.RemoveAt(index);
            if (mNames != null)
                mNames.RemoveAt(index);
            if (mSizes != null)
                mSizes.RemoveAt(index);
            if (mColors != null)
                mColors.RemoveAt(index);
        }

        public void AddEmpty(int count)
        {
            Count += count;
            if (mPositions != null)
                mPositions.AddEmpty(count);
            if (mEndPositions != null)
                mEndPositions.AddEmpty(count);
            if (mErrorRange != null)
                mErrorRange.AddEmpty(count);
            if (mHighLowRange != null)
                mHighLowRange.AddEmpty(count);
            if (mStartEndRange != null)
                mStartEndRange.AddEmpty(count);
            if (mUserData != null)
                mUserData.AddEmpty(count);
            if (mNames != null)
                mNames.AddEmpty(count);
            if (mSizes != null)
                mSizes.AddEmpty(count);
            if (mColors != null)
                mColors.AddEmpty(count);
        }

        
        public DoubleVector3 GetPositionUncheked(int index)
        {
            return mPositions[index];
        }

        public GenericDataItem GetValue(int index)
        {
            GenericDataItem res = new GenericDataItem();
            if (mPositions != null)
                res.Position = mPositions[index];
            if (mEndPositions != null)
                res.EndPosition = mEndPositions[index];
            if (mErrorRange != null)
                res.ErrorRange = mErrorRange[index];
            if (mHighLowRange != null)
                res.HighLow = mHighLowRange[index];
            if (mStartEndRange != null)
                res.StartEnd = mStartEndRange[index];
            if (mUserData != null)
                res.userData = mUserData[index];
            if (mNames != null)
                res.Name = mNames[index];
            if (mSizes != null)
                res.Size = mSizes[index];
            if (mColors != null)
                res.Color = mColors[index];
            return res;
        }

        public DoubleVector3[] GetRawPositionArray()
        {
            return mPositions.RawArrayWithExtraLastItem;
        }

        public void SetValue(int index, GenericDataItem val)
        {
            if (mPositions != null)
                mPositions[index] = val.Position;
            if (mColors != null)
                mColors[index] = val.Color;
            if (mEndPositions != null)
                mEndPositions[index]= val.EndPosition;
            if (mErrorRange != null)
                mErrorRange[index]= val.ErrorRange;
            if (mHighLowRange != null)
                mHighLowRange[index]= val.HighLow;
            if (mStartEndRange != null)
                mStartEndRange[index]= val.StartEnd;
            if (mUserData != null)
                mUserData[index]= val.userData;
            if (mNames != null)
                mNames[index]= val.Name;
            if (mSizes != null)
                mSizes[index]= val.Size;
        }

        public void InsertValue(int index,GenericDataItem val)
        {
            Count++;
            if (mPositions != null)
                mPositions.Insert(index,val.Position);
            if (mColors != null)
                mColors.Insert(index, val.Color);
            if (mEndPositions != null)
                mEndPositions.Insert(index,val.EndPosition);
            if (mErrorRange != null)
                mErrorRange.Insert(index,val.ErrorRange);
            if (mHighLowRange != null)
                mHighLowRange.Insert(index,val.HighLow);
            if (mStartEndRange != null)
                mStartEndRange.Insert(index,val.StartEnd);
            if (mUserData != null)
                mUserData.Insert(index,val.userData);
            if (mNames != null)
                mNames.Insert(index,val.Name);
            if (mSizes != null)
                mSizes.Insert(index,val.Size);
        }

        public void AppendValue(GenericDataItem val)
        {
            Count++;
            if (mPositions!=null)
                mPositions.Add(val.Position);
            if (mColors != null)
                mColors.Add(val.Color);
            if (mEndPositions !=null)
                mEndPositions.Add(val.EndPosition);
            if (mErrorRange!=null)
                mErrorRange.Add(val.ErrorRange);
            if (mHighLowRange !=null)
                mHighLowRange.Add(val.HighLow);
            if (mStartEndRange!= null)
                mStartEndRange.Add(val.StartEnd);
            if (mUserData != null)
                mUserData.Add(val.userData);
            if (mNames != null)
                mNames.Add(val.Name);
            if (mSizes != null)
                mSizes.Add(val.Size);
        }

        public void ValidateChannel(ChannelType channel)
        {
            if (HasChannel(channel) == false)
                throw new Exception("channel does not exsit in category");
        }

        private bool HasChannel(ChannelType channel)
        {
            return (mCurrentChannels & channel) != 0;
        }

        public void SetUserData(int index,object userData)
        {
            ValidateChannel(ChannelType.UserData);
            mUserData[index] = userData;
        }

        public void SetName(int index,string name)
        {
            ValidateChannel(ChannelType.Name);
            mNames[index] = name;
        }

        public void SetErrorRange(int index,DoubleRange range)
        {
            ValidateChannel(ChannelType.HighLow);
            mStartEndRange[index] = range;
        }

        public void SetHighLow(int index, DoubleRange range)
        {
            ValidateChannel(ChannelType.HighLow);
            mStartEndRange[index] = range;
        }

        public void SetStartEnd(int index, DoubleRange range)
        {
            ValidateChannel(ChannelType.StartEnd);
            mStartEndRange[index] = range;
        }

        public void SetSize(int index, double size)
        {
            ValidateChannel(ChannelType.Sizes);
            mSizes[index] = size;
        }

        public void SetColor(int index,Color32 color)
        {
            ValidateChannel(ChannelType.Color);
            mColors[index] = color;
        }

        public void SetEndPosition(int index, DoubleVector3 position)
        {
            ValidateChannel(ChannelType.EndPositions);
            mEndPositions[index] = position;
        }

        public void SetPosition(int index,DoubleVector3 position)
        {
            ValidateChannel(ChannelType.Positions);
            mPositions[index] = position;
        }

        public void Clear()
        {
            Count = 0;
            if (mNames != null)
                mNames.Clear();

            if (mPositions != null)
                mPositions.Clear();
            if (mColors != null)
                mColors.Clear();
            if (mEndPositions != null)
                mEndPositions.Clear();

            if (mStartEndRange != null)
                mStartEndRange.Clear();

            if (mHighLowRange != null)
                mHighLowRange.Clear();

            if (mSizes != null)
                mSizes.Clear();

            if (mUserData != null)
                mUserData.Clear();

            if (mBoundingVolume != null)
                mBoundingVolume.Clear();

            if (mAlternativeBoundingVolume != null)
                mAlternativeBoundingVolume.Clear();
        }

        /// <summary>
        /// Not using generic type t to make sure we do not violate any mono generic limitations
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="list"></param>
        private SimpleList<DoubleRect> EnsureDoubleRectList(ChannelType channel, SimpleList<DoubleRect> list)
        {
            if (HasChannel(channel))
            {
                if (list == null)
                {
                    list = new SimpleList<DoubleRect>(Count);
                    list.ClearWithoutRelease = true;
                    list.AddEmpty(Count);
                }
            }
            else
                list = null;
            return list;
        }
        
        /// <summary>
        /// Not using generic type t to make sure we do not violate any mono generic limitations
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="list"></param>
        private SimpleList<string> EnsureStringList(ChannelType channel,  SimpleList<string> list)
        {
            if (HasChannel(channel))
            {
                if (list == null)
                {
                    list = new SimpleList<string>(Count);
                    list.AddEmpty(Count);
                }
            }
            else
                list = null;
            return list;
        }

        /// <summary>
        /// Not using generic type t to make sure we do not violate any mono generic limitations
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="list"></param>
        private SimpleList<object> EnsureObjectList(ChannelType channel,  SimpleList<object> list)
        {
            if (HasChannel(channel))
            {
                if (list == null)
                {
                    list = new SimpleList<object>(Count);
                    list.AddEmpty(Count);
                }
            }
            else
                list = null;
            return list;
        }

        /// <summary>
        /// Not using generic type t to make sure we do not violate any mono generic limitations
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="list"></param>
        private SimpleList<DoubleRect?> EnsureNullDoubleRectList(ChannelType channel,  SimpleList<DoubleRect?> list)
        {
            
            if (channel == ChannelType.None || HasChannel(channel))    // channel.None used to make sure the list exists in anyway.
            {
                if (list == null)
                {
                    list = new SimpleList<DoubleRect?>(Count);
                    list.ClearWithoutRelease = true;
                    list.AddEmpty(Count);
                }
            }
            else
                list = null;
            return list;
        }

        /// <summary>
        /// Not using generic type t to make sure we do not violate any mono generic limitations
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="list"></param>
        private SimpleList<double> EnsureDoubleList(ChannelType channel, SimpleList<double> list)
        {
            if (HasChannel(channel))
            {
                if (list == null)
                {
                    list = new SimpleList<double>(Count);
                    list.ClearWithoutRelease = true;
                    list.AddEmpty(Count);
                }
            }
            else
                list = null;
            return list;
        }

        /// <summary>
        /// Not using generic type t to make sure we do not violate any mono generic limitations
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="list"></param>
        private SimpleList<DoubleRange> EnsureDoubleRangeList(ChannelType channel, SimpleList<DoubleRange> list)
        {
            if (HasChannel(channel))
            {
                if (list == null)
                {
                    list = new SimpleList<DoubleRange>(Count);
                    list.ClearWithoutRelease = true;
                    list.AddEmpty(Count);
                }
            }
            else
                list = null;
            return list;
        }

        private SimpleList<Color32> EnsureColorList(ChannelType channel, SimpleList<Color32> list)
        {
            if (HasChannel(channel))
            {
                if (list == null)
                {

                    list = new SimpleList<Color32>(Count);
                    list.ClearWithoutRelease = true;
                    list.AddEmpty(Count);
                    ChartCommon.DevLog("channels", "list " + (list != null));
                }
            }
            else
            {
                list = null;
                ChartCommon.DevLog("channels", "list " + (list != null));
            }
            return list;
        }
        /// <summary>
        /// Not using generic type t to make sure we do not violate any mono generic limitations
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="list"></param>
        private SimpleList<DoubleVector3> EnsureDoubleVector3List(ChannelType channel, SimpleList<DoubleVector3> list)
        {

            if (HasChannel(channel))
            {
                if (list == null)
                {

                    list = new SimpleList<DoubleVector3>(Count);
                    list.ClearWithoutRelease = true;
                    list.AddEmpty(Count);
                    ChartCommon.DevLog("channels", "list " + (list != null));
                }
            }
            else
            {
                list = null;
                ChartCommon.DevLog("channels", "list " + (list != null));
            }
            return list;
        }
        
        /// <summary>
        /// Ensure all the channels are allocated for this instance
        /// </summary>
        /// <param name="channels"></param>
        public void EnsureChannels(ChannelType channels)
        {
            if (mCurrentChannels == channels)
                return;
            
            mCurrentChannels = channels;
            mPositions = EnsureDoubleVector3List(ChannelType.Positions, mPositions);
            mColors = EnsureColorList(ChannelType.Color, mColors);
            mEndPositions = EnsureDoubleVector3List(ChannelType.EndPositions, mEndPositions);
            mStartEndRange = EnsureDoubleRangeList(ChannelType.StartEnd, mStartEndRange);
            mHighLowRange = EnsureDoubleRangeList(ChannelType.HighLow,  mHighLowRange);
            mErrorRange = EnsureDoubleRangeList(ChannelType.ErrorRange,  mErrorRange);
            mBoundingVolume = EnsureNullDoubleRectList(ChannelType.None,  mBoundingVolume);
            mAlternativeBoundingVolume = EnsureDoubleRectList(ChannelType.AlternativeBoundingVolume,  mAlternativeBoundingVolume);
            mUserData = EnsureObjectList(ChannelType.UserData,  mUserData);
            mSizes = EnsureDoubleList(ChannelType.Sizes, mSizes);
            mNames = EnsureStringList(ChannelType.Name,  mNames);

            if (mPositions != null)
                ChartCommon.DevLog("channel", "positions");
            if(mColors != null)
                ChartCommon.DevLog("channel", "color");
            if (mEndPositions != null)
                ChartCommon.DevLog("channel", "end positions");
            if (mErrorRange != null)
                ChartCommon.DevLog("channel", "error");
            if (mHighLowRange != null)
                ChartCommon.DevLog("channel", "high low");
            if (mStartEndRange != null)
                ChartCommon.DevLog("channel", "start end");
            if (mUserData != null)
                ChartCommon.DevLog("channel", "user data");
            if (mNames != null)
                ChartCommon.DevLog("channel", "names");
            if (mSizes != null)
                ChartCommon.DevLog("channel", "sizes");

        }

        public DoubleVector3[] RawPositionArray()
        {
            if (mPositions == null)
                return null;
            return mPositions.RawArrayWithExtraLastItem;
        }

        public Color32[] RawColorArray()
        {
            if (mColors == null)
                return null;
            return mColors.RawArrayWithExtraLastItem;
        }

        public DoubleVector3[] RawEndPositionArray()
        {
            if (mEndPositions == null)
                return null;
            return mEndPositions.RawArrayWithExtraLastItem;
        }

        public double[] RawSizeArray()
        {
            if (mSizes == null)
                return null;
            return mSizes.RawArrayWithExtraLastItem;
        }

        public DoubleRange[] RawStartEndArray()
        {
            if (mStartEndRange == null)
                return null;
            return mStartEndRange.RawArrayWithExtraLastItem;
        }

        public DoubleRange[] RawHighLowArray()
        {
            if (mHighLowRange == null)
                return null;
            return mHighLowRange.RawArrayWithExtraLastItem;
        }

        public DoubleRange[] RawErrorRangeArray()
        {
            if (mErrorRange == null)
                return null;
            return mErrorRange.RawArrayWithExtraLastItem;
        }

        public DoubleRect?[] RawBoundingVolume()
        {
            if (mBoundingVolume == null)
                return null;
            return mBoundingVolume.RawArrayWithExtraLastItem;
        }

        public object[] RawUserDataArray()
        {
            if (mUserData == null)
                return null;
            return mUserData.RawArrayWithExtraLastItem;
        }

        public string[] RawNameArray()
        {
            if (mNames == null)
                return null;
            return mNames.RawArrayWithExtraLastItem;
        }

    }
}
