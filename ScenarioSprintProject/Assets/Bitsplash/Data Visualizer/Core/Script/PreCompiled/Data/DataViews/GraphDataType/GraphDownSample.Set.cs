using DataVisualizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Data_Visualizer.Core.Script.PreCompiled.Data.DataViews.GraphDataType
{
    partial class GraphDownSample
    {
        struct SetResult
        {
            public const int OpEmpty = 0;
            public const int OpSet = 1;
            public const int OpMultipleSet =2 ;
            public const int OpResample = 3;
            public const int ResampleModeNone = 0;
            public const int ResampleModeB = 1;
            public const int ResampleModeC = 2;
            public int Operation;
            public int SegmentIndex;
            public int ViewIndex;
            public int PointIndex;
            public int A, B, C, D;
            public int ResampleMode;
            public SetResult(int op,int segIndex,int viewIndex,int pointIndex)
            {
                Operation = op;
                SegmentIndex = segIndex;
                ViewIndex = viewIndex;
                PointIndex = pointIndex;
                A = B = C = D = 0;
                ResampleMode = ResampleModeNone;
            }
            public SetResult(int op, int segIndex, int viewIndex, int pointIndex,int resampleMode)
            {
                Operation = op;
                SegmentIndex = segIndex;
                ViewIndex = viewIndex;
                PointIndex = pointIndex;
                A = B = C = D = 0;
                ResampleMode = resampleMode;
            }

            public SetResult(int segIndex,int viewIndex,int pointIndex,int a ,int b ,int c, int d)
            {
                SegmentIndex = segIndex;
                Operation = OpMultipleSet;
                ViewIndex = viewIndex;
                PointIndex = pointIndex;
                A = a;
                B = b;
                C = c;
                D = d;
                ResampleMode = ResampleModeNone;
            }
        }

        SetResult ResampleSegmentSet(int segmentIndex,int viewIndex,int pointIndex)
        {
            var seg = mSegments[segmentIndex];
            int index = seg.segmentFromIndex;
            int endIndex = index + seg.segmentCount;
            double maxY = double.NegativeInfinity;
            double minY = double.PositiveInfinity;
            int start = index;
            int min = index;
            int max = index;
            int end = index;
            index++;
            var positions = OffsetRawPositionArray();
            for (; index < endIndex; index++)
            {
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
            return new SetResult(segmentIndex, viewIndex, pointIndex, rawArr[0], rawArr[1], rawArr[2], rawArr[3]);
        }

        void PerformBeforeSetResult(SetResult res)
        {
            switch (res.Operation)
            {
                case SetResult.OpEmpty:
                    break;
                case SetResult.OpSet:
                    RaiseOnBeforeSet(res.ViewIndex);
                    break;
                case SetResult.OpMultipleSet:
                    break;
                case SetResult.OpResample:
                    switch(res.ResampleMode)
                    {
                        case SetResult.ResampleModeB:
                            RaiseOnBeforeSet(res.ViewIndex+1);
                            break;
                        case SetResult.ResampleModeC:
                            RaiseOnBeforeSet(res.ViewIndex + 2);
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        void PerformSetResult(SetResult res)
        {
            switch (res.Operation)
            {
                case SetResult.OpEmpty:
                    break;
                case SetResult.OpSet:
                    RaiseOnSet(res.ViewIndex);
                    break;
                case SetResult.OpMultipleSet:
                    break;
                case SetResult.OpResample:
                    var resamp = ResampleSegmentSet(res.SegmentIndex, res.ViewIndex, res.PointIndex);
                    switch (res.ResampleMode)
                    {
                        case SetResult.ResampleModeB:
                            mDownSampleIndices[res.ViewIndex + 1] = resamp.B;
                            RaiseOnSet(res.ViewIndex + 1);
                            SetDownsampleIndexWithEvents(res.ViewIndex, resamp.A);
                            SetDownsampleIndexWithEvents(res.ViewIndex+2, resamp.C);
                            SetDownsampleIndexWithEvents( res.ViewIndex+3, resamp.D);
                            break;
                        case SetResult.ResampleModeC:
                            mDownSampleIndices[res.ViewIndex + 2] = resamp.C;
                            RaiseOnSet(res.ViewIndex + 2);
                            SetDownsampleIndexWithEvents( res.ViewIndex, resamp.A);
                            SetDownsampleIndexWithEvents( res.ViewIndex + 1, resamp.B);
                            SetDownsampleIndexWithEvents( res.ViewIndex + 3, resamp.D);
                            break;
                        default:
                            SetDownsampleIndexWithEvents( res.ViewIndex, resamp.A);
                            SetDownsampleIndexWithEvents( res.ViewIndex + 1, resamp.B);
                            SetDownsampleIndexWithEvents( res.ViewIndex + 2, resamp.C);
                            SetDownsampleIndexWithEvents( res.ViewIndex + 3, resamp.D);
                            break;
                    }
                    break;
                default:
                    break;
            }
        }


        void SetDownsampleIndexWithEvents(int index, int value,bool raiseEvents)
        {
            if (mDownSampleIndices[index] == value)
                return;
            if(raiseEvents)
                RaiseOnBeforeSet(index);
            mDownSampleIndices[index] = value;
            if (raiseEvents)
                RaiseOnSet(index);
        }

        SetResult SetItem(OffsetArray positions, int segIndex, int pointIndex)
        {
            var seg = mSegments[segIndex];
            if (seg.downsampleCount == 0)
                return new SetResult();
            int start = mDownSampleIndices[seg.downsampleStart];
            if (pointIndex == start)
                return new SetResult(SetResult.OpSet, segIndex,seg.downsampleStart, pointIndex);
            int end = mDownSampleIndices[seg.downsampleStart + seg.downsampleCount - 1];
            if (pointIndex == end)
                return new SetResult(SetResult.OpSet, segIndex, seg.downsampleStart + seg.downsampleCount - 1, pointIndex);
            if(seg.downsampleCount == 3)
                return new SetResult(SetResult.OpSet, segIndex, seg.downsampleStart + 1, pointIndex);
            int mid1 = mDownSampleIndices[seg.downsampleStart + 1];
            int mid2 = mDownSampleIndices[seg.downsampleStart + 2];
            if (pointIndex == mid1)
                return new SetResult(SetResult.OpResample, segIndex, seg.downsampleStart,  pointIndex,SetResult.ResampleModeB);//ResampleSegmentSet(info, segIndex,seg.downsampleStart,pointIndex);
            if(pointIndex == mid2)
                return new SetResult(SetResult.OpResample, segIndex, seg.downsampleStart, pointIndex,SetResult.ResampleModeC);//ResampleSegmentSet(info, segIndex,seg.downsampleStart,pointIndex);
            double mid1Y = positions[mid1].y;
            double mid2Y = positions[mid2].y;
            double pointY = positions[pointIndex].y;
            if ((mid1Y < pointY && pointY < mid2Y) || (mid2Y < pointY && pointY < mid1Y))
                return new SetResult();
            return new SetResult(SetResult.OpResample, segIndex, seg.downsampleStart, pointIndex,SetResult.ResampleModeNone); //ResampleSegmentSet(info, segIndex, seg.downsampleStart, pointIndex);
        }
    }
}
