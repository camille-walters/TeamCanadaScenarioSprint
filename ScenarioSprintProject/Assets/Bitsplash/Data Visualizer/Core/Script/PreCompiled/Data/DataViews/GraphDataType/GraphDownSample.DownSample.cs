using DataVisualizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThetaList;
using UnityEngine;

namespace Assets.Data_Visualizer.Core.Script.PreCompiled.Data.DataViews.GraphDataType
{
    partial class GraphDownSample
    {
        public int mStartIndex = 0, mEndIndex = 0;
        public double mDownsampleFrom = 0;
        public double mDownsampleTo = 0;
        public double mSegmentSize = 0;

        SimpleList<SegmentInfo> mSegments = new SimpleList<SegmentInfo>(true);
        SimpleList<int> mDownSampleIndices = new SimpleList<int>(true);

        public bool IsEmpty { get { return mDownSampleIndices.Count == 0; } }
        public bool IsSingular { get { return mSegmentSize <= 0.0; } }

        class OffsetArray
        {
            int mOffset;
            DoubleVector3[] mArray;
            public OffsetArray(DoubleVector3[] arr,int offset)
            {
                mArray = arr;
                mOffset = offset;
            }
            public DoubleVector3 this[int index]
            {
                get { return mArray[index + mOffset]; }
            }
        }
        bool IsInSegment(int segmentIndex,int index)
        {
            if (segmentIndex < 0 || segmentIndex >= mSegments.Count)
                return false;
            var seg = mSegments[segmentIndex];
            if (seg.downsampleCount == 0)
                return false;
            if(seg.downsampleStart >= mDownSampleIndices.Count)
                return false;
            if (mDownSampleIndices[seg.downsampleStart] > index)
                return false;
            if (mDownSampleIndices[seg.downsampleStart + seg.downsampleCount - 1] < index)
                return false;
            return true;
        }
        public int FindSegmentByValue(DoubleVector3[] positions, double x)
        {
            if (IsSingular)
            {
                if (x == mDownsampleFrom)
                    return 0;
                return -1;
            }
            int segIndex = (int)((x - mDownsampleFrom) / mSegmentSize);
            if (segIndex < 0)
                return -1;
            return segIndex;
        }
        int FindSegment(OffsetArray positions,int index)
        {
            double x = positions[index].x;
            if (IsSingular)
            {
                if(x==mDownsampleFrom)
                    return 0;
                if (x > mDownsampleFrom)
                    return 1;
                return -1;
            }
            int segIndex = (int)((x - mDownsampleFrom) / mSegmentSize);
            if (segIndex < 0)
                return -1;
            if (IsInSegment(segIndex, index))
                return segIndex;
            if (IsInSegment(segIndex + 1, index))
                return segIndex + 1;
            if (IsInSegment(segIndex - 1, index))
                return segIndex - 1;
            return segIndex;
        }

        public void ModifyCount(int segmentIndex, int count)
        {
            var rawSegmentArry = mSegments.RawArray;
            rawSegmentArry[segmentIndex].segmentCount += count;
        }

        public void ShiftSegments(int segmentIndex, int count)
        {
            var rawSegmentArry = mSegments.RawArray;
            for (int i = segmentIndex; i < mSegments.Count; i++)
                rawSegmentArry[i].segmentFromIndex += count;
        }

        public void PushSegments(int segmentIndex, int count)
        {
            var rawSegmentArry = mSegments.RawArray;
            for (int i = segmentIndex; i < mSegments.Count; i++)
                rawSegmentArry[i].downsampleStart += count;

        }

        public void PushIndices(int fromIndex,int count)
        {
            int res = Array.BinarySearch(mDownSampleIndices.RawArray, 0, mDownSampleIndices.Count, fromIndex);
            if (res < 0)
                res = ~res;
            var indexArray = mDownSampleIndices.RawArray;
            for (int i = res; i < mDownSampleIndices.Count; i++)
                indexArray[i]+=count;
        }

        public void CleanLastSegments()
        {
            for(int i=mSegments.Count-1; i>=0; i--)
            {
                if (mSegments[i].downsampleCount > 0)
                    break;
                mSegments.RemoveFromEnd(1);
                mDownsampleTo -= mSegmentSize;
            }
            if (mSegments.Count ==0 || mSegments.Count ==1)
            {
                ClearWithEvents();
                return;
            }
            var last = mSegments[mSegments.Count - 1];
            mDownSampleIndices.RemoveFromEnd(last.downsampleCount);
            mSegments.RemoveFromEnd(1);
            if (mDownSampleIndices.Count ==0)
            {
                ClearWithEvents();
                return;
            }
            RaiseOnRemoveFromEnd(last.downsampleCount);
            mEndIndex = mDownSampleIndices[mDownSampleIndices.Count - 1]+1;
            mDownsampleTo = OffsetRawPositionArray()[mEndIndex].x;
        }

        public void Clear()
        {
            mSegments.Clear();
            mDownSampleIndices.Clear();
            mSegmentSize = 0;
            mDownsampleFrom = 0;
            mDownsampleTo = 0;
            mStartIndex = 0;
            mEndIndex = 0;
        }

        public void AddIndex(int index)
        {
            mDownSampleIndices.Add(index);
        }

        public void ResampleSegment(int start,int min,int max,int end,SimpleList<int> resampledSegment)
        {
            int segmentCount = 1;
            resampledSegment.Clear();
            resampledSegment.Add(start);
            if (start != end)
            {
                int first = min;
                int second = max;
                if (min > max)
                {
                    first = max;
                    second = min;
                }
                if (start != first)
                {
                    segmentCount++;
                    resampledSegment.Add(first);
                }
                segmentCount++;
                resampledSegment.Add(second);
                if (second != end)
                {
                    segmentCount++;
                    resampledSegment.Add(end);
                }
            }
        }
        public void AddSegment(int start, int min, int max, int end,int from,int count)
        {
            int segmentCount = 1;
            AddIndex(start);
            if (start == end)
            {
                AddSegment(from, count, segmentCount);
                return;
            }

            int first = min;
            int second = max;

            if (min > max)
            {
                first = max;
                second = min;
            }
            if (start != first)
            {
                segmentCount++;
                AddIndex(first);
            }
            segmentCount++;
            AddIndex(second);
            if (second != end)
            {
                segmentCount++;
                AddIndex(end);
            }
            AddSegment(from, count, segmentCount);
        }

        public void AddSegment(int segFrom,int segCount,int downSampleCount)
        {
            mSegments.Add(new SegmentInfo(mDownSampleIndices.Count, downSampleCount, segFrom, segCount));
        }

        void MoveIndex(OffsetArray positions, ref int index, bool isRight)
        {
            double start = positions[index].x;
            if (isRight)
            {
                int end = MainView.Count - 1;
                while (index < end && positions[index + 1].x == start)
                    index++;
            }
            else
            {
                while (index > 0 && positions[index - 1].x == start)
                    index--;
            }
        }

        int AppendDownSample( double from, double to)
        {
            CleanLastSegments();
            if (IsEmpty)
            {
                DownSample( from, to);
                return mDownSampleIndices.Count;
            }
            return InnerAppendDownSample(from, to);
        }
        
        int InnerAppendDownSample(double from,double to)
        {
            var positions = OffsetRawPositionArray();
            double currentX = mDownsampleTo;
            mDownsampleTo = to;
            int currentIndex = mEndIndex;
            int startCount = mDownSampleIndices.Count;
            while (currentX <= mDownsampleTo && currentIndex < MainView.Count)
            {
                double segmentEnd = currentX + mSegmentSize;
                DownsampleSegment(positions,  ref currentIndex, segmentEnd);
                currentX = segmentEnd;
            }
            mEndIndex = Math.Min(MainView.Count-1,currentIndex);
            return mDownSampleIndices.Count - startCount;
        }
        
        void DownSample(double from, double to)
        {
            Clear();
            if (MainView.Count == 0)
                return;
            var positions = OffsetRawPositionArray();
            // find the first position between from and to
            int startIndex = FindXValue(from);
            MoveIndex(positions, ref mStartIndex, false); // in case of equal value , move to find the first instance of the value in array

            //find the last position between from and to
            int endIndex = FindXValue(to);
            MoveIndex(positions, ref endIndex, true); // in case of equal value , move to find the last instance of the value in array
            endIndex = Math.Min(MainView.Count, endIndex);

            int pointCount = endIndex - startIndex + 1; // the point count is an estimate and doesn't have to be accurate for the method to work
            double startX = positions[startIndex].x;
            double endX = positions[endIndex].x;
            

            double totalSpan = endX - startX;
            int totalSegments = mSegmentCount;// (int)Math.Ceiling(((double)pointCount) / ((double)mAvgPointsPerSegment));
            mSegmentSize = totalSpan / totalSegments; // calculate the segment size

            // start with an empty sample (size 0.0 , later we append the rest of it)
            mDownsampleFrom= startX;
            mDownsampleTo = startX;
            mStartIndex = startIndex;
            mEndIndex = startIndex;

            InnerAppendDownSample( mDownsampleFrom, endX); // append the data to the 0 sized sample
        }

        void DownsampleSegment(OffsetArray positions, ref int index, double endX)
        {
            double maxY = double.NegativeInfinity;
            double minY = double.PositiveInfinity;
            if (endX < positions[index].x)
            {
                AddSegment(index,0,0);
                return;
            }
            int from = index;
            int start = index;
            int min = index;
            int max = index;
            int end = index;
            index++;
            for (; index < MainView.Count; index++)
            {
                double current = positions[index].x;
                if (current > endX)
                    break;
                end = index;
                double y = positions[index].y;
                if (y < minY)
                {
                    minY = y;
                    min = index;
                }
                if (y > maxY)
                {
                    maxY = y;
                    max = index;
                }
            }
            AddSegment(start, min, max, end,from,index-1-from);
        }

        int FindXValue(double value)
        {
            int count = MainView.Count;
            var positions = OffsetRawPositionArray();
            int from = 0;
            int to = count;

            while (from < to)
            {
                int center = from + (to - from) / 2;
                double centerX = positions[center].x;
                if (centerX == value)
                    return center;
                if (value < centerX)
                    to = center - 1;
                else
                    from = center + 1;
            }
            return from;
        }

        struct SegmentInfo
        {
            public SegmentInfo(int start, int count,int segStart,int segCount)
            {
                downsampleStart = start;
                downsampleCount = count;
                segmentFromIndex = segStart;
                segmentCount = segCount;
            }
            public int segmentFromIndex;
            public int segmentCount;
            public int downsampleStart;
            public int downsampleCount;
        }
    }
}
