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
        int mSegmentCount = 800;
        SetResult mTempSetResult;
        protected IDataViewerNotifier MainView { get; private set; }
        public GraphDownSample(IDataViewerNotifier mainView, int avgPointsPerSegment)
        {
            MainView = mainView;
        }

        public event Action<object, ChannelType> OnSetArray;
        public event Action<object, ChannelType, int> OnAppendArray;
        public event Action<object, int> OnRemoveFromStart;
        public event Action<object, int> OnRemoveFromEnd;
        public event Action<object, int> OnBeforeSet;
        public event Action<object, int> OnSet;
        public event Action<object, int> OnInsert;
        public event Action<object, int> OnBeforeInsert;
        public event Action<object, int> OnBeforeRemove;
        public event Action<object, int> OnRemove;
        public event Action<object> OnClear;

        public int Count
        {
            get { return mDownSampleIndices.Count; }
        }

        public int[] RawViewArray
        {
            get { return mDownSampleIndices.RawArrayWithExtraLastItem; }
        }
        
        OffsetArray OffsetRawPositionArray()
        {
            return new OffsetArray(MainView.RawPositionArray(0), MainView.SubArrayOffset);
        }

        public void RaiseOnClear()
        {
            if (OnClear != null)
                OnClear(this);
        }

        public void RaiseOnRemove(int index)
        {
            if (OnRemove != null)
                OnRemove(this, index);
        }

        public void RaiseOnBeforeRemove(int index)
        {
            if (OnBeforeRemove != null)
                OnBeforeRemove(this, index);
        }

        public void RaiseOnBeforeInsert(int index)
        {
            if (OnBeforeInsert != null)
                OnBeforeInsert(this, index);
        }
        public void RaiseOnInsert(int index)
        {
            if (OnInsert != null)
                OnInsert(this, index);
        }
        public void RaiseOnSet(int index)
        {
            if (OnSet != null)
                OnSet(this, index);
        }
        public void RaiseOnBeforeSet(int index)
        {
            if (OnBeforeSet != null)
                OnBeforeSet(this, index);
        }
        public void RaiseOnRemoveFromEnd(int count)
        {
            if (OnRemoveFromEnd != null)
                OnRemoveFromEnd(this, count);
        }
        public void RaiseOnRemoveFromStart(int count)
        {
            if (OnRemoveFromStart != null)
                OnRemoveFromStart(this, count);
        }
        public void RaiseOnAppendArray(ChannelType type, int count)
        {
            if (OnAppendArray != null)
                OnAppendArray(this, type, count);
        }

        public void RaiseOnSetArray(ChannelType type)
        {
            if (OnSetArray != null)
                OnSetArray(this, type);
        }
        public void MainView_OnAfterCommit(object data, OperationTree<int> operations)
        {
            DownSampleWithEvents();
        }

        public void MainView_OnRemoveFromEnd(object data, int count)
        {
            DownSampleWithEvents();
        }

        public void MainView_OnRemoveFromStart(object data, int count)
        {
            DownSampleWithEvents();
        }

       
        public void MainView_OnBeforeInsert(object data, int index)
        {

        }

        public void MainView_OnInsert(object data, int index)
        {
            if (DownsampleSampleIfEmpty())
                return;
            PushIndices(index, 1);
            var positions = OffsetRawPositionArray();
            int segment = FindSegment(positions, index);
            ChartIntegrity.Assert(segment >= 0);
            if (segment < 0)
                return;
            if (segment >= mSegments.Count)
            {
                AppendData(ChannelType.Positions, 1);
                return;
            }
            var res = MergeIntoSegment(positions, segment, index);
            PerformMergeResult(res);
        }

        public void MainView_OnBeforeRemove(object data, int index)
        {
            if (IsEmpty)
                return;
            var positions = OffsetRawPositionArray();
            int segment = FindSegment(positions, index);
            ChartIntegrity.Assert(segment >= 0);
            ChartIntegrity.Assert(segment < mSegments.Count);
            if (segment < 0 || segment >= mSegments.Count)
                return;
            var res = RemoveFromSegment(segment, index);
            PerformRemoveResult(res);
        }

        public void MainView_OnRemove(object data, int index)
        {
            if (DownsampleSampleIfEmpty())
                return;
            PushIndices(index, -1);
        }

        public void MainView_OnAppendArray(object data, ChannelType channel, int count)
        {
            AppendData(channel, count);
        }

        void AppendData(ChannelType channel, int count)
        {
            if (MainView.Count > 0)
            {
                if (mSegments.Count > (mSegmentCount * 2))
                {
                    DownSampleWithEvents();
                }
                else
                {
                    var positions = OffsetRawPositionArray();
                    double from = positions[0].x;
                    double to = positions[MainView.Count - 1].x;
                    int downSampledCount = AppendDownSample(from, to);
                    RaiseOnAppendArray(channel, downSampledCount);
                }
            }
        }
        public void MainView_OnSetArray(object data, ChannelType channel)
        {
            DownSampleWithEvents();
        }

        public void MainView_OnBeforeSet(object data, int index)
        {
            if (IsEmpty)
                return;
            
            //var positions = OffsetRawPositionArray();
            //int segment = FindSegment(positions, index);
            //if (segment == -1 || mSegments.Count==0 || mDownSampleIndices.Count ==0) // point is not in view
            //    return;
            //int end = mDownSampleIndices[mDownSampleIndices.Count-1];
            //Debug.Log("before " + end);
            //RaiseOnBeforeSet(end);
            //ChartIntegrity.Assert(segment >= 0);
            //ChartIntegrity.Assert(segment < mSegments.Count);
            //mTempSetResult = SetItem( positions, segment, index);
            //PerformBeforeSetResult(mTempSetResult);
        }
        

        public void MainView_OnSet(object data, int index)
        {
            if (DownsampleSampleIfEmpty())
                return;
            if (mDownSampleIndices.Count > 0)
            {
                RaiseOnBeforeSet(mDownSampleIndices.Count - 1);
                RaiseOnSet(mDownSampleIndices.Count - 1);
            }
        }

        bool DownsampleSampleIfEmpty()
        {
            if(IsEmpty)
            {
                DownSampleWithEvents();
                return true;
            }
            return false;
        }
        public void MainView_OnClear(object data)
        {
            ClearWithEvents();
        }

        void ClearWithEvents()
        {
            Clear();
            RaiseOnClear();
        }
        public void SetSegmentCount(int count)
        {
            mSegmentCount = count;
            DownSampleWithEvents();
        }
        public void DownSampleMainView()
        {
            if (MainView.Count > 0)
            {
                var positions = OffsetRawPositionArray();
                double from = positions[0].x;
                double to = positions[MainView.Count - 1].x;
                DownSample(from, to);
            }
            else
                Clear();
        }
        void DownSampleWithEvents()
        {    
                                
            if (MainView.Count > 0)
            {
                var positions = OffsetRawPositionArray();
                double from = positions[0].x;
                double to = positions[MainView.Count - 1].x;
                DownSample(from, to);
                RaiseOnSetArray(ChannelType.Positions);
            }
            else
                ClearWithEvents();
        }

    }
}
