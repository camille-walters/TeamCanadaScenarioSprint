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
    public partial class GraphDataView : DataView
    {
        public const string SegmentCountSetting = "SegmentCount";
        static DoubleVector3[] mLoadArray = new DoubleVector3[512];
        
        ViewPortion mViewPortion;

        /// <summary>
        /// the amount of downsample segments in total. 
        /// </summary>
        const double EdgeSizeFactor = 0.5f;
        int mAvgPointsPerSegment = 100;
        GraphDownSample mDownSample;

        public GraphDataView(IDataViewerNotifier mainView)
            : base(mainView)
        {

        }

        /// <summary>
        /// applies the data in mSegments to the GraphDataView, so that the arrays are populated with the downsampled data
        /// </summary>
        void ApplyDownSample()
        {

        }



        public override void SetView(ViewPortion view)
        {
            mViewPortion = view;
            //if (mSegments.Count == 0)
            //{
            //    // add downsample
            //}
            //else
            //{
            //    //   ... add view modification
            //}
            base.SetView(view);
        }

        public override void ApplySettings(IDataSeriesSettings settings)
        {
            base.ApplySettings(settings);
            object obj = settings.GetSetting(DataSeriesCategory.OptimizationTypeName);
            int count = 600;
            if (obj != null)
                count = ((GraphOptimization)obj) == GraphOptimization.Accurate ? 2000 : 600;
            mDownSample.SetSegmentCount(count);
        }
        protected override void MainView_OnAfterCommit(object data, OperationTree<int> operations)
        {
            mDownSample.MainView_OnAfterCommit(data,operations);
        }

        protected override void MainView_OnAppendArray(object data, ChannelType channel, int count)
        {
            mDownSample.MainView_OnAppendArray(data, channel, count);
        }

        protected override void MainView_OnClear(object data)
        {
            mDownSample.MainView_OnClear(data);
        }
        protected override void MainView_OnBeforeInsert(object data, int index)
        {
            mDownSample.MainView_OnBeforeInsert(data, index);
        }
        protected override void MainView_OnInsert(object data, int index)
        {
            mDownSample.MainView_OnInsert(data, index);
        }
        protected override void MainView_OnRemoveFromEnd(object data, int count)
        {
            mDownSample.MainView_OnRemoveFromEnd(data, count);
        }
        protected override void MainView_OnBeforeRemove(object data, int index)
        {
            mDownSample.MainView_OnBeforeRemove(data, index);
        }

        protected override void MainView_OnRemove(object data, int index)
        {
            mDownSample.MainView_OnRemove(data, index);
        }
        protected override void MainView_OnSetArray(object data, ChannelType channel)
        {
            mDownSample.MainView_OnSetArray(data, channel);
        }
        protected override void MainView_OnBeforeSet(object data, int index)
        {
            mDownSample.MainView_OnBeforeSet(data, index);
        }
        protected override void MainView_OnSet(object data, int index)
        {
            mDownSample.MainView_OnSet(data, index);
        }
        protected override void MainView_OnRemoveFromStart(object data, int count)
        {
            mDownSample.MainView_OnRemoveFromStart(data, count);
        }

        void HookDownsample(GraphDownSample sample)
        {
            sample.OnAppendArray += Sample_OnAppendArray;
            sample.OnBeforeInsert += Sample_OnBeforeInsert;
            sample.OnBeforeRemove += Sample_OnBeforeRemove;
            sample.OnBeforeSet += Sample_OnBeforeSet;
            sample.OnClear += Sample_OnClear;
            sample.OnInsert += Sample_OnInsert;
            sample.OnRemove += Sample_OnRemove;
            sample.OnRemoveFromEnd += Sample_OnRemoveFromEnd;
            sample.OnRemoveFromStart += Sample_OnRemoveFromStart;
            sample.OnSet += Sample_OnSet;
            sample.OnSetArray += Sample_OnSetArray;
        }

        private void Sample_OnSetArray(object data, ChannelType channel)
        {
            RaiseOnSetArray(channel);
        }

        private void Sample_OnSet(object data, int index)
        {
            RaiseOnSet(index);
        }

        private void Sample_OnRemoveFromStart(object data, int count)
        {
            RaiseOnRemoveFromStart(count);
        }

        private void Sample_OnRemoveFromEnd(object data, int count)
        {
            RaiseOnRemoveFromEnd(count);
        }

        private void Sample_OnRemove(object data, int index)
        {
            RaiseOnRemove(index);
        }

        private void Sample_OnInsert(object data, int index)
        {
            RaiseOnInsert(index);
        }

        private void Sample_OnClear(object data)
        {
            RaiseOnClear();
        }

        private void Sample_OnBeforeSet(object data, int index)
        {
            RaiseOnBeforeSet(index);
        }

        private void Sample_OnBeforeRemove(object data, int index)
        {
            RaiseOnBeforeRemove(index);
        }

        private void Sample_OnBeforeInsert(object data, int index)
        {
            RaiseOnBeforeInsert(index);
        }

        private void Sample_OnAppendArray(object data, ChannelType channel, int count)
        {
            RaiseOnAppendArray(channel, count);
        }

        protected override void FirstLoadData()
        {
            
            mDownSample = new GraphDownSample(MainView, mAvgPointsPerSegment);
            mDownSample.DownSampleMainView();
            HookDownsample(mDownSample);
        }

        public override int Count
        {
            get
            {
                return mDownSample.Count;
            }
        }

        public override int[] RawViewArray()
        {
            return mDownSample.RawViewArray;
        }
    }
}
