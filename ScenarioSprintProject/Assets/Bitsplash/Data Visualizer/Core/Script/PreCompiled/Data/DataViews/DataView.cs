using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThetaList;
using UnityEngine;

namespace DataVisualizer{
    public abstract class DataView : IDataView
    {
        protected IDataViewerNotifier MainView { get; private set; }
        public virtual int StackCount { get { return MainView.StackCount; } }

        public virtual int Count
        {
            get { return MainView.Count; }
        }

        public ChannelType CurrentChannels { get { return MainView.CurrentChannels; } }

        public string Name
        {
            get { return MainView.Name; }
        }

        public virtual int SubArrayOffset { get { return MainView.SubArrayOffset; } }

        public DataView(IDataViewerNotifier mainView)
        {
            MainView = mainView;
            mainView.ParentViewSet += MainView_ParentViewSet;
            FirstLoadData();
            HookMainView();
        }

        private void MainView_ParentViewSet(ViewPortion view)
        {
            SetView(view);
            RaiseParentViewSet(view);
        }

        public event Action<object, bool> OnDataValidityChanged;
        public event Action<object> OnChannelCompositionChanged;
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
        public event Action<object, OperationTree<int>> OnAfterCommit;
        public event Action<object, OperationTree<int>> OnBeforeCommit;
        public event Action<object> OnClear;
        public event Action<object> OnUncomittedData;

        public event Action<ViewPortion> ParentViewSet;

        public void RaiseParentViewSet(ViewPortion view)
        {
            if (ParentViewSet != null)
                ParentViewSet(view);
        }

        public void RaiseOnUncomittedData()
        {
            if (OnUncomittedData != null)
                OnUncomittedData(this);
        }

        public void RaiseOnClear()
        {
            if (OnClear != null)
                OnClear(this);
        }

        public void RaiseOnBeforeCommit(OperationTree<int> op)
        {
            if (OnBeforeCommit != null)
                OnBeforeCommit(this, op);
        }
        public void RaiseOnAfterCommit(OperationTree<int> op)
        {
            if (OnAfterCommit != null)
                OnAfterCommit(this, op);
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
        public void RaiseOnAppendArray(ChannelType type,int count)
        {
            if (OnAppendArray != null)
                OnAppendArray(this, type, count);
        }

        public void RaiseOnDataValidityChanged(bool isValid)
        {
            if (OnDataValidityChanged != null)
                OnDataValidityChanged(this,isValid);
        }

        public void RaiseOnChannelCompositionChanged()
        {
            if (OnChannelCompositionChanged != null)
                OnChannelCompositionChanged(this);
        }

        public void RaiseOnSetArray(ChannelType type)
        {
            if (OnSetArray != null)
                OnSetArray(this,type);
        }

        protected abstract void FirstLoadData();

        void HookMainView()
        {
            MainView.OnChannelCompositionChanged += MainView_OnChannelCompositionChanged;
            MainView.OnUncomittedData += MainView_OnUncomittedData;
            MainView.OnClear += MainView_OnClear;
            MainView.OnBeforeSet += MainView_OnBeforeSet;
            MainView.OnSet += MainView_OnSet;
            MainView.OnSetArray += MainView_OnSetArray;
            MainView.OnAppendArray += MainView_OnAppendArray;
            MainView.OnRemove += MainView_OnRemove;
            MainView.OnInsert += MainView_OnInsert;
            MainView.OnBeforeInsert += MainView_OnBeforeInsert;
            MainView.OnAfterCommit += MainView_OnAfterCommit;
            MainView.OnBeforeRemove += MainView_OnBeforeRemove;
            MainView.OnRemoveFromStart += MainView_OnRemoveFromStart;
            MainView.OnRemoveFromEnd += MainView_OnRemoveFromEnd;
        }

        protected virtual void MainView_OnRemoveFromEnd(object data, int count)
        {

        }

        protected virtual void MainView_OnRemoveFromStart(object data, int count)
        {
            
        }

        protected virtual void MainView_OnBeforeRemove(object data, int index)
        {
        }

        protected virtual void MainView_OnAfterCommit(object data, OperationTree<int> operations)
        {

        }

        protected virtual void MainView_OnBeforeInsert(object data, int index)
        {
        }

        protected virtual void MainView_OnInsert(object data, int index)
        {
        }

        protected virtual void MainView_OnRemove(object data, int index)
        {
        }

        protected virtual void MainView_OnAppendArray(object data, ChannelType channel, int count)
        {
        }

        protected virtual void MainView_OnSetArray(object data, ChannelType channel)
        {

        }

        protected virtual void MainView_OnSet(object data, int index)
        {

        }

        protected virtual void MainView_OnBeforeSet(object data, int index)
        {

        }

        protected virtual void MainView_OnClear(object data)
        {

        }

        protected virtual void MainView_OnUncomittedData(object data)
        {

        }

        protected virtual void MainView_OnChannelCompositionChanged(object data)
        {
            RaiseOnChannelCompositionChanged();
        }

        public void CommitChanges()
        {
            MainView.CommitChanges();
        }

        public virtual void SetView(ViewPortion view)
        {

        }

        public virtual DoubleVector3[] RawPositionArray(int stack)
        {
            return MainView.RawPositionArray(stack);
        }

        public virtual DoubleVector3[] RawEndPositionArray(int stack)
        {
            return MainView.RawEndPositionArray(stack);
        }

        public virtual double[] RawSizeArray(int stack)
        {
            return MainView.RawSizeArray(stack);
        }

        public virtual DoubleRange[] RawStartEndArray(int stack)
        {
            return MainView.RawStartEndArray(stack);
        }

        public virtual DoubleRange[] RawHighLowArray(int stack)
        {
            return MainView.RawHighLowArray(stack);
        }

        public virtual DoubleRange[] RawErrorRangeArray(int stack)
        {
            return MainView.RawErrorRangeArray(stack);
        }

        public virtual DoubleRect?[] RawBoundingVolume(int stack)
        {
            return MainView.RawBoundingVolume(stack);
        }

        public virtual Color32[] RawColorArray(int stack)
        {
            return MainView.RawColorArray(stack);
        }

        public virtual object[] RawUserDataArray(int stack)
        {
            return MainView.RawUserDataArray(stack);
        }

        public virtual string[] RawNameArray(int stack)
        {
            return MainView.RawNameArray(stack);
        }

        public virtual int[] RawViewArray()
        {
            return null;
        }

        public virtual DataBounds DataBounds(int stack)
        {
            return MainView.DataBounds(stack);
        }

        public IDataViewerNotifier GetDataView(string name)
        {
            throw new NotImplementedException();
        }

        public virtual void ApplySettings(IDataSeriesSettings settings)
        {
            var dataView = MainView as IDataView;
            if (dataView != null)
                dataView.ApplySettings(settings);
        }
    }
}
