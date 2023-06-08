using DataVisualizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThetaList;
using UnityEngine;

namespace DataVisualizer{
    /// <summary>
    /// this is a base class for all axis data objects , it contains 
    /// </summary>
    public abstract class AxisDataGenerator : IChartVisualObject, IDataViewerNotifier, ILabeledObject
    { 
        public string ParentItemName { get; private set; }
        public string VisualFeatureName { get; private set; }

        bool mHasSettingError = false;
        bool mHasOriginalView = false, mHasLocalRect = false;
        protected bool HasView { get; private set; }
        DataSeriesBase mDataSeries;
        string mName;
        DataSeriesRefreshType mRefreshType = DataSeriesRefreshType.None;

        protected bool IsActive { get; private set; }

        protected DataSeriesBase DataSeries { get { return mDataSeries; } }

        public GameObject underlyingGameObject { get { return mDataSeries.underlyingGameObject; } }

        public ViewPortion CurrentView { get { return mDataSeries.CurrentView; } }

        public virtual int StackCount { get { return 1; } }

        public abstract int Count { get; }

        public abstract ChannelType CurrentChannels { get; }

        public string Name { get { return mName; } }

        public ViewPortion OriginalView { get; private set; }
        public ViewPortion LocalRect { get; private set; }

#pragma warning disable 0067
        public event Action<object> OnChannelCompositionChanged;
        public event Action<object, ChannelType> OnSetArray;
        public event Action<object, int> OnAppendEmpryArray;
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
        public event Action<object,ChannelType, int> OnAppendArray;
        public event Action<object> OnArrayLoaded;
        public event Action<object, int> OnRemoveFromStart;
        public event Action<object, bool> OnDataValidityChanged;
        public event Action<object, int> OnRemoveFromEnd;
        public event Action<ViewPortion> ParentViewSet;
#pragma warning restore 0067

        public IPrivateDataSeriesChart Parent { get; private set; }

        protected virtual void OnRefresh()
        {

        }

        public virtual void UniformUpdate()
        {
            ChartIntegrity.AssertMethodCalled((Action<DataSeriesChart>)SetParent);
            switch (mRefreshType)
            {
                case DataSeriesRefreshType.RecreateEntries:
                    OnRefresh();
                    mDataSeries.Refresh();
                    break;
                case DataSeriesRefreshType.FullRefresh:
                    OnRefresh();
                    mDataSeries.Refresh();
                    break;
                case DataSeriesRefreshType.InvalidateGraphic:
                    mDataSeries.Invalidate();
                    break;
                default:
                    break;
            }

            mDataSeries.UniformUpdate();
            mRefreshType = DataSeriesRefreshType.None;
        }

        /// <summary>
        /// returns true if the setting is different them the original one
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="member"></param>
        /// <param name="settings"></param>
        /// <param name="settingName"></param>
        /// <param name="defValue"></param>
        /// <param name="refreshOnChange"></param>
        /// <returns></returns>
        public bool UnboxSetting<T>(ref T member, IDataSeriesSettings settings, string settingName, T defValue, DataSeriesRefreshType refreshOnChange = DataSeriesRefreshType.InvalidateGraphic)
        {
            T unboxed = ChartCommon.UnboxObject<T>(settings.GetSetting(settingName), defValue);
            if (((unboxed == null) && (member == null)) || unboxed.Equals(member))
                return false;
            member = unboxed;
            if (refreshOnChange != DataSeriesRefreshType.None)
                mRefreshType |= refreshOnChange;
            return true;
        }

        void DisableOnError()
        {
            mHasSettingError = true;
            if (mDataSeries.IsActive)
            {
                ChartCommon.RuntimeWarning("visual property " + VisualFeatureName + " on item " + ParentItemName + " is disabled due to an error");
                mDataSeries.SetActive(false);
            }
        }

        protected virtual void OnSettingsAppliedSuccessfuly()
        {

        }

        public void ApplySettings(IDataSeriesSettings settings, string parentItemName, string visualFeatureName)
        {
            ChartIntegrity.AssertMethodCalled((Action<DataSeriesChart>)SetParent);
            ChartCommon.DevLog(LogOptions.Axis, GetType().Name, "Apply settings");

            ParentItemName = parentItemName;
            VisualFeatureName = visualFeatureName;

            bool active = ChartCommon.UnboxObject<bool>(settings.GetSetting("active"), true);
            if (active != IsActive)
                SetActive(active);

            string error;

            if (ValidateSettings(settings, out error) == false)
            {
                ChartCommon.RuntimeWarning("inscuffient settings : " + error);     // These mean your materials or parameters are not set correctly and the DataSeries can't be drawn
                DisableOnError();
                return;
            }

            mHasSettingError = false;
            VerifyActive();
            OnSettingsAppliedSuccessfuly();

        }

        void VerifyActive()
        {
            if (IsActive && mHasSettingError == false && mDataSeries.IsActive == false)
                mDataSeries.SetActive(true);
        }

        public abstract bool ValidateSettings(IDataSeriesSettings settings,out string error);

        public virtual TextDataHolder LabelData { get { return null; } }

        protected void RaiseOnInsert(int index)
        {
            if (OnInsert != null)
                OnInsert(this, index);
        }

        protected void RaiseOnClear()
        {
            if (OnClear != null)
                OnClear(this);
        }

        protected void RaiseOnBeforeInsert(int index)
        {
            if (OnBeforeInsert != null)
                OnBeforeInsert(this, index);
        }
        protected void RaiseOnBeforeSet(int index)
        {
            if (OnBeforeSet != null)
                OnBeforeSet(this, index);
        }

        protected void RaiseOnSet(int index)
        {
            if (OnSet != null)
                OnSet(this, index);
        }

        protected void RaiseOnBeforeRemove(int index)
        {
            if (OnBeforeRemove != null)
                OnBeforeRemove(this,index);
        }

        protected void RaiseOnRemove(int index)
        {
            if (OnRemove != null)
                OnRemove(this, index);
        }

        public AxisDataGenerator(string name, GameObject obj)
        {
            mName = name;
            mDataSeries = GenerateSeries(obj);
        }

        protected abstract DataSeriesBase GenerateSeries(GameObject obj);

        public virtual void Destroy()
        {
            mDataSeries.Destroy();
        }

        public virtual void FitInto(ViewPortion localRect)
        {
            ChartIntegrity.AssertMethodCalled((Action<DataSeriesChart>)SetParent);
            ChartCommon.DevLog(LogOptions.Axis, GetType().Name, "Fit Into",localRect);
            mHasLocalRect = true;
            LocalRect = localRect;
            mDataSeries.FitInto(localRect);
            VerifyView();
        }

        public virtual void OnInit()
        {
            ChartIntegrity.AssertMethodCalled((Action<DataSeriesChart>)SetParent);
            ChartCommon.DevLog(LogOptions.Axis, GetType().Name, "Init");
            mDataSeries.OnInit();
            ChartCommon.DevLog(LogOptions.Axis, GetType().Name, "On set");
        }

        protected virtual ViewPortion DataSeriesView
        {
            get
            {
                return OriginalView;
            }
        }

        public int SubArrayOffset
        {
            get { return 0; }
        }

        protected virtual void OnHasView()
        {
            mDataSeries.OnSet(this);
        }

        void VerifyView()
        {
            if((HasView == false) &&  mHasOriginalView && mHasLocalRect)
            {
                HasView = true;
                OnHasView();
            }
        }

        public virtual void OnSetView(ViewPortion view)
        {
            ChartIntegrity.AssertMethodCalled((Action<DataSeriesChart>)SetParent);
            ChartCommon.DevLog(LogOptions.Axis, GetType().Name, "Set View",view);
            mHasOriginalView = true;
            OriginalView = view;
            ChartCommon.DevLog(LogOptions.Axis, GetType().Name, "Set data series view", DataSeriesView);
            mDataSeries.OnSetView(DataSeriesView);
            VerifyView();
        }

        public virtual DoubleRect?[] RawBoundingVolume(int stack)
        {
            return null;
        }

        public virtual Color32[] RawColorArray(int stack)
        {
            return null;
        }

        public virtual DoubleVector3[] RawEndPositionArray(int stack)
        {
            return null;
        }

        public virtual DoubleRange[] RawErrorRangeArray(int stack)
        {
            return null;
        }

        public virtual DoubleRange[] RawHighLowArray(int stack)
        {
            return null;
        }

        public virtual string[] RawNameArray(int stack)
        {
            return null;
        }

        public virtual DoubleVector3[] RawPositionArray(int stack)
        {
            return null;
        }

        public virtual double[] RawSizeArray(int stack)
        {
            return null;
        }

        public virtual DoubleRange[] RawStartEndArray(int stack)
        {
            return null;
        }
        
        public virtual object[] RawUserDataArray(int stack)
        {
            return null;
        }

        public void SetActive(bool active)
        {
            ChartCommon.DevLog(LogOptions.Axis, GetType().Name, "Set Active",active);
            IsActive = active;
            if(mHasSettingError == false)
                mDataSeries.SetActive(active);
        }

        public void SetParent(IPrivateDataSeriesChart parent)
        {
            ChartIntegrity.Assert(parent != null);
            ChartIntegrity.NotifyMethodCall((Action <DataSeriesChart>) SetParent);
            ChartCommon.DevLog(LogOptions.Axis, GetType().Name, "Set Parent");
            Parent = parent;
            mDataSeries.SetParent(parent);
        }

        public void CommitChanges()
        {
            
        }

        public abstract DataBounds DataBounds(int stack);

        public void SetVisible(bool visible)
        {
            mDataSeries.SetVisible(visible);
        }

        public void SetView(ViewPortion view)
        {

        }

        public int[] RawViewArray()
        {
            return null;
        }

        public IDataViewerNotifier GetDataView(string name)
        {
            return this;
        }
    }
}
