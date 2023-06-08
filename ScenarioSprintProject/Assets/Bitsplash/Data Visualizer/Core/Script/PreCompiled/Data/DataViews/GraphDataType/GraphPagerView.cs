using DataVisualizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThetaList;
using UnityEngine;

namespace DataVisualizer
{
    class GraphPagerView : DataView
    {
        public const string PageZoomSetting = "PageZoom";
        ViewPortion mView;
        const double viewRatio = 0.5;
        int mFrom = 0, mTo = 0;
        double mFromX, mToX;
        double mViewWidth;
        double mPageZoomRatio = 1.2;

        public GraphPagerView(IDataViewerNotifier mainView) 
            : base(mainView)
        {

        }

        protected override void FirstLoadData()
        {
            CreateView(true);
        }

        bool ShouldRecreateView()
        {
            if (mFrom < 0 || mTo < 0 || mViewWidth == 0)
                return true;
            if (mTo == mFrom)
                return true;
            if (mView.Width * mPageZoomRatio < mViewWidth)
                return true;
            var positions = MainView.RawPositionArray(0);

            double viewMin = mView.Min.x - mView.Width * 0.05;
            double viewMax = mView.Max.x + mView.Width * 0.05;
            if (viewMin < mFromX || viewMax > mToX)
                return true;
            return false;
        }
        public override void ApplySettings(IDataSeriesSettings settings)
        {
            base.ApplySettings(settings);
            var obj = settings.GetSetting(DataSeriesCategory.OptimizationTypeName);
            if (obj != null)
                mPageZoomRatio = ((GraphOptimization)obj) == GraphOptimization.Accurate ? 2.0 : 1.2;
            else
                mPageZoomRatio = 1.2;
        }
        void CreateView(bool suspendEvents = false)
        {
            var positions = MainView.RawPositionArray(0);
            mFromX = mView.Min.x - mView.Width * viewRatio;
            mToX = mView.Max.x + mView.Width * viewRatio;
            mFrom = FindXValue(mFromX);
            MoveIndex(positions, ref mFrom, false);
            mTo = FindXValue(mToX);
            MoveIndex(positions, ref mTo, true);
            mFrom = Math.Max(0, mFrom - 1);
            mTo = Math.Min(mTo + 1, MainView.Count);
            mViewWidth = mView.Width;
            if (!suspendEvents)
                RaiseOnSetArray(ChannelType.Positions);
        }

        public override void SetView(ViewPortion view)
        {
            mView = view;
            if (ShouldRecreateView())
                CreateView();
        }

        public override int SubArrayOffset
        {
            get { return mFrom; }
        }

        public override int Count
        {
            get { return mTo - mFrom; }
        }

        int FindXValue(double value)
        {
            int count = MainView.Count;
            var positions = MainView.RawPositionArray(0);
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

        void MoveIndex(DoubleVector3[] positions, ref int index, bool isRight)
        {
            if (MainView.Count == 0)
                return;
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

        protected override void MainView_OnAfterCommit(object data, OperationTree<int> operations)
        {
            CreateView();
        }

        protected override void MainView_OnAppendArray(object data, ChannelType channel, int count)
        {
            int prevEnd = MainView.Count - count;
            if (mTo < prevEnd)
                return;
            if (ShouldRecreateView())
            {
                CreateView();
                return;
            }
            var positions = MainView.RawPositionArray(0);
            int newTo = FindXValue(mToX);
            MoveIndex(positions, ref newTo, true);
            newTo = Math.Min(MainView.Count, newTo + 1);

            int addedCount = newTo - mTo;
            if (addedCount <= 0)
                return;
            mTo = newTo;
            RaiseOnAppendArray(channel, addedCount);
        }

        protected override void MainView_OnBeforeInsert(object data, int index)
        {
            if (index <= mFrom && mFrom != 0)
                return;
            if(index > mTo && mTo!=MainView.Count-1)
                return;
            RaiseOnBeforeInsert(index - mFrom);
        }
        
        protected override void MainView_OnInsert(object data, int index)
        {
            int arrayStart = mFrom;
            int arrayEnd = mTo;
            if(index <= mTo)
            {
                mTo = Math.Max(mTo + 1, MainView.Count);
                if(index < mFrom)
                    mFrom = Math.Max(mFrom + 1, MainView.Count);
            }
            if (index <= arrayStart && arrayStart!=0)
                return;
            if (index > arrayEnd && arrayEnd != MainView.Count - 1)
                return;
            RaiseOnInsert(index - arrayStart);
        }

        protected override void MainView_OnBeforeSet(object data, int index)
        {
            if (index < mFrom || index > mTo)
                return;
            RaiseOnBeforeSet(index - mFrom);
        }

        protected override void MainView_OnSet(object data, int index)
        {
            if (index < mFrom || index > mTo)
                return;
            RaiseOnSet(index - mFrom);
        }

        protected override void MainView_OnBeforeRemove(object data, int index)
        {
            if (index < mFrom)
                return;
            if (index > mTo && mTo != MainView.Count - 1)
                return;
            if(index == mFrom)
            {
                if (mFrom == 0)
                    RaiseOnBeforeRemove(0);
                else
                    RaiseOnBeforeSet(0);
            }
            else if (index == mTo)
            {
                if (mTo == MainView.Count - 1)
                    RaiseOnBeforeRemove(mTo - mFrom);
                else
                    RaiseOnBeforeSet(mTo - mFrom);
            }
        }

        protected override void MainView_OnRemove(object data, int index)
        {
            //int arrayStart = mFrom;
            //int arrayEnd = mTo;
            //if (index <= mTo)
            //{
            //    mTo = Math.Max(mTo - 1, MainView.Count);
            //    if (index < mFrom)
            //        mFrom = Math.Max(mFrom - 1, MainView.Count);
            //}
        }

        protected override void MainView_OnRemoveFromEnd(object data, int count)
        {
            CreateView();
        }

        protected override void MainView_OnRemoveFromStart(object data, int count)
        {
            CreateView();
        }

        protected override void MainView_OnClear(object data)
        {
            mFrom = 0;
            mTo = 0;
            RaiseOnClear();
        }
        
        protected override void MainView_OnSetArray(object data, ChannelType channel)
        {
            CreateView();
        }
        
    }
}
