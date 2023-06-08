using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThetaList;

namespace Assets.Data_Visualizer.Core.Script.PreCompiled.Data.DataViews.GraphDataType
{
    partial class GraphDownSample
    {
        static SimpleList<int> mResampleList = new SimpleList<int>(true, 5);
        struct RemoveResult
        {
            public const int OpEmpty = 0;
            public const int OpRemoveOne = 1;
            public const int OpResample = 2;
            public int Operation;
            public int SegmentIndex;
            public int ViewIndex;
            public int PointIndex;
            public int A, B, C, D;
            public int Count;

            public RemoveResult(int segmentIndex,int pointIndex)
            {
                PointIndex = pointIndex;
                SegmentIndex = segmentIndex;
                ViewIndex = 0;
                A = B = C = D = 0;
                Count = 0;
                Operation = 0;
            }
            public RemoveResult(int op,int segIndex,int viewIndex,int pointIndex)
            {
                Operation = op;
                SegmentIndex = segIndex;
                ViewIndex = viewIndex;
                A = B = C = D = 0;
                Count = 0;
                PointIndex = pointIndex;
            }
            public RemoveResult(int segIndex,int pointIndex,int a,int b,int c,int d,int count)
            {
                SegmentIndex = segIndex;
                ViewIndex = 0;
                Operation = OpResample;
                A = a;
                B = b;
                C = c;
                D = d;
                Count = count;
                PointIndex = pointIndex;
            }
        }

        void PerformRemoveResult(RemoveResult res)
        {
            var seg = mSegments[res.SegmentIndex];
            switch (res.Operation)
            {
                case RemoveResult.OpRemoveOne:
                    RaiseOnBeforeRemove(res.ViewIndex);
                    mDownSampleIndices.RemoveAt(res.ViewIndex);
                    PushSegments(res.SegmentIndex + 1, -1);
                    RaiseOnRemove(res.ViewIndex);
                    break;
                case RemoveResult.OpResample:
                    int diff = seg.downsampleCount - res.Count;
                    if(diff > 0)
                    {
                        for (int i = 0; i < diff; i++)
                        {
                            RaiseOnBeforeRemove(seg.downsampleStart);
                            mDownSampleIndices.RemoveAt(seg.downsampleStart);
                            seg.downsampleCount--;
                            RaiseOnRemove(seg.downsampleStart);
                        }
                        PushSegments(res.SegmentIndex + 1, -diff);
                    }
                    mResampleList.Clear();
                    var arr = mResampleList.RawArray;
                    arr[0] = res.A;
                    arr[1] = res.B;
                    arr[2] = res.C;
                    arr[3] = res.D;
                    int setCount = 0;
                    for(int i=0; i<seg.downsampleCount; i++)
                    {
                        RaiseOnBeforeSet(seg.downsampleStart + setCount);
                        mDownSampleIndices[seg.downsampleStart + setCount] = arr[setCount];
                        RaiseOnSet(seg.downsampleStart + setCount);
                        setCount++;
                    }
                    if (diff < 0)
                    {
                        diff = -diff;
                        for (int i = 0; i < diff; i++)
                        {
                            RaiseOnBeforeInsert(seg.downsampleStart + setCount);
                            mDownSampleIndices.Insert(seg.downsampleStart + setCount, arr[setCount]);
                            seg.downsampleCount++;
                            RaiseOnInsert(seg.downsampleStart + setCount);
                            setCount++;
                        }
                        PushSegments(res.SegmentIndex + 1, diff);
                    }
                    break;
                default:
                    break;
            }
            ModifyCount(res.SegmentIndex, -1);
            ShiftSegments(res.SegmentIndex + 1, -1);
        }

        RemoveResult ResampleSegment(int segmentIndex,int pointIndex)
        {
            var seg = mSegments[segmentIndex];
            int index = seg.segmentFromIndex;
            int endIndex = index + seg.segmentCount;
            double maxY = double.NegativeInfinity;
            double minY = double.PositiveInfinity;
            if (pointIndex == index)
                index++;
            int from = index;
            int start = index;
            int min = index;
            int max = index;
            int end = index;
            index++;
            var positions = OffsetRawPositionArray();
            for (; index < endIndex; index++)
            {
                if (pointIndex == index)
                    continue;
                double current = positions[index].x;
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
            ResampleSegment(start, min, max, end, mResampleList);
            var rawArr = mResampleList.RawArray;
            return new RemoveResult(segmentIndex,pointIndex, rawArr[0], rawArr[1], rawArr[2], rawArr[3], mResampleList.Count);
        }
        RemoveResult RemoveFromSegment(int segmentIndex,int pointIndex)
        {
            var indices = mDownSampleIndices.RawArray;
            var seg = mSegments[segmentIndex];
            int start = seg.downsampleStart;
            if (seg.downsampleCount == 0)
                return new RemoveResult(segmentIndex,pointIndex);
            if (seg.downsampleCount == 4)
            {
                bool resample = false;
                for (int i = 0; i < 4 && resample == false; i++)
                {
                    if (mDownSampleIndices[seg.downsampleStart + i] == pointIndex)
                    {
                        resample = true;
                        break;
                    }
                }
                if (resample == false)
                    return new RemoveResult(segmentIndex, pointIndex);
                return ResampleSegment(segmentIndex,pointIndex);
            }

            if (seg.downsampleCount == 1)
                return new RemoveResult(RemoveResult.OpRemoveOne, segmentIndex, seg.downsampleStart, pointIndex);
            else if (seg.downsampleCount == 2)
            {
                if (mDownSampleIndices[seg.downsampleStart] == pointIndex)
                    return new RemoveResult(RemoveResult.OpRemoveOne, segmentIndex, seg.downsampleStart, pointIndex);
                return new RemoveResult(RemoveResult.OpRemoveOne, segmentIndex, seg.downsampleStart+1, pointIndex);
            }

            if (mDownSampleIndices[seg.downsampleStart] == pointIndex)
                return new RemoveResult(RemoveResult.OpRemoveOne, segmentIndex, seg.downsampleStart,pointIndex);
            if (mDownSampleIndices[seg.downsampleStart + 1] == pointIndex)
                return new RemoveResult(RemoveResult.OpRemoveOne, segmentIndex, seg.downsampleStart + 1, pointIndex);
            return new RemoveResult(RemoveResult.OpRemoveOne, segmentIndex, seg.downsampleStart + 2, pointIndex);
        }
    }
}
