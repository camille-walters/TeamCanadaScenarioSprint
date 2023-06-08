using DataVisualizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ThetaList;

namespace Assets.Data_Visualizer.Core.Script.PreCompiled.Data.DataViews.GraphDataType
{
    partial class GraphDownSample
    {
        SimpleList<int> mMergeList = new SimpleList<int>(true);

        struct MergeResult
        {
            public const int OpEmpty = 0;
            public const int OpInsert = 1;
            public const int OpSet = 2;
            public const int OpMultiSet = 3;


            public int Operation;
            public int PointIndex;
            public int ViewIndex;
            public int SegmentIndex;
            public int A, B, C, D;

            public MergeResult(int op, int segIdx,int viewIdx, int pointIdx)
            {
                Operation = op;
                PointIndex = pointIdx;
                SegmentIndex = segIdx;
                ViewIndex = viewIdx;
                A = B = C = D = 0;
            }

            public MergeResult(int segIdx,int viewIdx,int a,int b,int c,int d,int pointIndex)
            {
                SegmentIndex = segIdx;
                ViewIndex = viewIdx;
                A = a;
                B = b;
                C = c;
                D = d;
                Operation = OpMultiSet;
                PointIndex = pointIndex;
            }
        }

        void SetDownsampleIndexWithEvents(int index,int value)
        {
            if (mDownSampleIndices[index] == value)
                return;
            RaiseOnBeforeSet(index);
            mDownSampleIndices[index] = value;
            RaiseOnSet(index);
        }

        void PerformMergeResult(MergeResult res)
        {
            switch (res.Operation)
            {
                case MergeResult.OpInsert:
                    PushSegments(res.SegmentIndex + 1, 1);
                    RaiseOnBeforeInsert(res.ViewIndex);
                    mDownSampleIndices.Insert(res.ViewIndex, res.PointIndex);
                    RaiseOnInsert(res.ViewIndex);
                    break;
                case MergeResult.OpSet:
                    RaiseOnBeforeSet(res.ViewIndex);
                    mDownSampleIndices[res.ViewIndex] = res.PointIndex;
                    RaiseOnSet(res.ViewIndex);
                    break;
                case MergeResult.OpMultiSet:
                    SetDownsampleIndexWithEvents(res.ViewIndex, res.A);
                    SetDownsampleIndexWithEvents(res.ViewIndex +1 , res.B);
                    SetDownsampleIndexWithEvents(res.ViewIndex+ 2, res.C);
                    SetDownsampleIndexWithEvents(res.ViewIndex+ 3, res.D);
                    break;
                default:
                    break;
            }
            ModifyCount(res.SegmentIndex, 1);
            ShiftSegments(res.SegmentIndex + 1, 1);
        }

        MergeResult MergeIntoSegment(OffsetArray positions, int segmentIndex, int pointIndex)
        {
            var seg = mSegments[segmentIndex];
            DoubleVector3 point = positions[pointIndex];
            var sampleIndices = mDownSampleIndices.RawArray;
            
            if (seg.downsampleCount == 0)
                return new MergeResult(MergeResult.OpInsert,segmentIndex,seg.downsampleStart,pointIndex);
            if (seg.downsampleCount == 1)
            {
                int start = sampleIndices[seg.downsampleStart];
                if (start < pointIndex)
                    return new MergeResult(MergeResult.OpInsert, segmentIndex, seg.downsampleStart +1, pointIndex);
                return new MergeResult(MergeResult.OpInsert, segmentIndex, seg.downsampleStart, pointIndex);
            }
            if (seg.downsampleCount == 2)
            {
                int start = sampleIndices[seg.downsampleStart];
                int end = sampleIndices[seg.downsampleStart + 1];
                if(pointIndex < start)
                    return new MergeResult(MergeResult.OpInsert, segmentIndex, seg.downsampleStart, pointIndex);
                if(pointIndex > end)
                    return new MergeResult(MergeResult.OpInsert, segmentIndex, seg.downsampleStart + 2, pointIndex);
                return new MergeResult(MergeResult.OpInsert, segmentIndex, seg.downsampleStart + 1, pointIndex);
            }
            if (seg.downsampleCount == 3)
            {
                int start = sampleIndices[seg.downsampleStart];
                int mid = sampleIndices[seg.downsampleStart + 1];
                int end = sampleIndices[seg.downsampleStart + 2];
                if (pointIndex < start)
                    return new MergeResult(MergeResult.OpInsert, segmentIndex, seg.downsampleStart, pointIndex);
                if (pointIndex > end)
                    return new MergeResult(MergeResult.OpInsert, segmentIndex, seg.downsampleStart + 3, pointIndex);
                if(pointIndex < mid)
                    return new MergeResult(MergeResult.OpInsert, segmentIndex, seg.downsampleStart + 1, pointIndex);
                return new MergeResult(MergeResult.OpInsert, segmentIndex, seg.downsampleStart + 2, pointIndex);
            }
            mMergeList.SetCount(4);
            mMergeList[0] = sampleIndices[seg.downsampleStart];
            mMergeList[1] = sampleIndices[seg.downsampleStart+1];
            mMergeList[2] = sampleIndices[seg.downsampleStart+2];
            mMergeList[3] = sampleIndices[seg.downsampleStart+3];
            int index = 0;
            for(;index<4; index++)
                if(mMergeList[index]>=pointIndex )
                    break;
            mMergeList.Insert(index, pointIndex);
            double aY = positions[mMergeList[1]].y;
            double bY = positions[mMergeList[2]].y;
            double cY = positions[mMergeList[3]].y;
            if ((aY <= bY && bY <= cY) || (aY >= bY && bY >= cY))
                mMergeList.RemoveAt(2);
            else if((bY <= aY && aY <= cY) || (bY >= aY && aY >= cY))
                mMergeList.RemoveAt(1);
            else
                mMergeList.RemoveAt(3);
            return new MergeResult(segmentIndex, seg.downsampleStart, mMergeList[0], mMergeList[1], mMergeList[2], mMergeList[3],pointIndex);
        }
    }
}
