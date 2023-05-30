using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    [ExecuteInEditMode]
    [Serializable]
    public abstract class DataSeriesChart : MonoBehaviour, IPrivateDataSeriesChart, ISerializationCallbackReceiver
    {
        static HashSet<string> mXParameters = new HashSet<string>();
        static HashSet<string> mYParameters = new HashSet<string>();
        bool mEnabled = false;
        static DataSeriesChart()
        {
            mXParameters.Clear();
            mYParameters.Clear();
            mXParameters.Add(StringFormatter.ParameterXValue);
            mYParameters.Add(StringFormatter.ParameterYValue);
        }
        List<string> mTmpStringList;
        HashSet<DataSeriesCategory> mInvalidatedCategories = new HashSet<DataSeriesCategory>();
        bool mInvalidated = false;
        [SerializeField]
        private AxisSystem mAxis;

        [SerializeField]
        private ChartDataSource mSeriesData;

        [SerializeField]
        private string defaultNumberFormat = "{0:0.##}";

        public string DefaultNumberFormat
        {
            get { return defaultNumberFormat; }
            set
            {
                defaultNumberFormat = value;
                ValidateChart();
            }
        }

        [SerializeField]
        private string defaultDateFormat = "{0:d}\\n{0:t}";
        public string DefaultDateFormat
        {
            get { return defaultDateFormat; }
            set
            {
                defaultDateFormat = value;
                ValidateChart();
            }
        }



        ViewPortion mLocalFitPortion;

        Transform mCategoryObject;

        HashSet<IDataSeries> mLoadedCategories = new HashSet<IDataSeries>();

        public ChartDataSource DataSource
        {
            get
            {
                return mSeriesData;
            }                                                                                                                             
        }

        public AxisSystem Axis
        {
            get
            {
                return mAxis;
            }
        }
        public AxisSystemView AxisView
        {
            get { return mAxis.View; }
        }
        protected IPrivateAxisSystem PrivateAxisSystem
        {
            get
            {
                return (IPrivateAxisSystem)mAxis;
            }
        }

        protected IPrivateChartDataSource PrivateSeriesData
        {
            get
            {
                return (IPrivateChartDataSource)mSeriesData;
            }
        }
        void InternalSetEnabled(bool enabled)
        {
            if (mEnabled == enabled)
                return;
            mEnabled = enabled;
            if (Axis != null)
                ((IPrivateAxisSystem)Axis).SetEnabled(enabled);

            if (enabled)
            {
                ValidateChart();
            }
            else
            {
                if (mLoadedCategories.Count > 0)
                {
                    mLoadedCategories.Clear();
                    DoneLoading();
                }
                IEnumerable<DataSeriesCategory> ordered = PrivateSeriesData.Categories.Values;

                foreach (DataSeriesCategory cat in ordered)
                {
                    IDataSeries series = PrivateSeriesData.GetSeries(cat.Name);
                    if (series != null)
                    {
                        series.Destroy();
                    }
                    PrivateSeriesData.RemoveSeries(cat.Name);
                }
            }
        }

        protected virtual void OnEnable() 
        {
            InternalSetEnabled(true);
        }

        protected virtual void OnDisable()
        {
            InternalSetEnabled(false);
        }

        protected virtual void OnDestroy()
        {
            InternalSetEnabled(false);
        }
        protected virtual void OnApplicationQuit()
        {
            InternalSetEnabled(false);
        }
        protected abstract GameObject CreateEmptyObject(Transform parent);

        void LoadAxis()
        {
            if (mAxis == null)  // this axis is already set
            {
                var t = transform.Find("~AxisSystem");
                GameObject obj = null;
                if (t != null)
                    obj = t.gameObject;
                if (obj != null)
                    mAxis = obj.GetComponent<AxisSystem>();
                else
                {
                    obj = CreateEmptyObject(transform);
                    obj.name = "~AxisSystem";
                    mAxis = obj.AddComponent<AxisSystem>();
                }
            }
            ChartCommon.HideObject(mAxis.gameObject);
        }

        protected virtual void Start()
        {
        }

        protected virtual void Awake()
        {
            LoadAxis();
            if (mSeriesData == null)
                mSeriesData = new ChartDataSource();
            mSeriesData.SetParent(this);
            PrivateAxisSystem.SetParent(this);
            var dataSource = PrivateSeriesData;
            if (dataSource != null)
                dataSource.Awake();

            ValidateChart();
        }

        private void MAxis_OnAxisViewChanged(AxisSystem axis)
        {
            if (mSeriesData != null)
                PrivateSeriesData.NotifyViewChanged(axis);
        }

        protected ViewPortion LocalFitPortion
        {
            get
            {
                return mLocalFitPortion;
            }
            set
            {
                mLocalFitPortion = value;
                OnDimentionsChanged();
            }
        }

        protected virtual void OnDimentionsChanged()
        {
            //dont invalidate the entire chart
            //base.OnDimentionsChanged();
            //instead just notify the exsiting series of the change
            DataSource.FitInto(mLocalFitPortion);
            PrivateAxisSystem.FitInto(mLocalFitPortion);
        }

        protected abstract GameObject FindChildObject(Transform parent,string name);

        protected abstract GameObject CreateChildObject(Transform parent);

        protected abstract IChartSeriesGraphic CreateEdgeGraphic(Transform parent);

        protected abstract void MakeClippableObject(GameObject obj,bool clippable);

        MultipleVisualFeaturesSeries GenerateCategorySeries(DataSeriesCategory cat,Transform parent)
        {

            var prevObject = FindChildObject(parent,cat.Name);
            if (prevObject != null)
                ChartCommon.SafeDestroy(prevObject);
            var obj = CreateChildObject(parent);
            
            obj.hideFlags = HideFlags.DontSave;
            obj.name = cat.Name;
            obj.transform.SetParent(parent);
            MultipleVisualFeaturesSeries series = obj.AddComponent<MultipleVisualFeaturesSeries>();
            var props = (IVisualFeatureHolderPrivate)cat;

            cat.CategoryDataChanged -= Cat_CategoryDataChanged;
            cat.CategoryDataChanged += Cat_CategoryDataChanged;
            cat.CategoryOrderChanged -= Cat_CategoryOrderChanged;
            cat.CategoryOrderChanged += Cat_CategoryOrderChanged;
            cat.CategoryOptimizationChanged -= Cat_CategoryOptimizationChanged;
            cat.CategoryOptimizationChanged += Cat_CategoryOptimizationChanged;
            cat.FeatureAdded -= Cat_FeatureAdded;
            cat.FeatureAdded += Cat_FeatureAdded;
            cat.FeatureRemoved -= Cat_FeatureRemoved;
            cat.FeatureRemoved += Cat_FeatureRemoved;
            series.SetParent(this);
            foreach (var pair in props.Properties.OrderByDescending(x=>x.Value.ViewOrder))
            {
                var visualFeature = (DataSeriesVisualFeature)pair.Value ;
                var featureObject = CreateEmptyObject(CategoryParent);
                var featureSeries = visualFeature.GenerateSeries(featureObject);
                featureSeries.underlyingGameObject.name = visualFeature.Name + "-" + visualFeature.VisualFeatureTypeName;
                featureSeries.underlyingGameObject.transform.SetParent(series.underlyingGameObject.transform,false);
                var prev = series.AddFeature(pair.Key, featureSeries);
                if (prev != null)
                    prev.Destroy();
            }

            return series;
        }

        private void Cat_CategoryOptimizationChanged(DataSeriesCategory obj)
        {
            ((IPrivateDataSeriesCategory)(obj.Data)).Validate();
        }

        public void Invalidate()
        {
            mInvalidated = true;
        }

        private void Cat_CategoryOrderChanged(DataSeriesCategory obj)
        {
            Invalidate();
        }

        public virtual void Update()
        {
            ValidateCategories();
            if (mInvalidated == true) 
                ValidateChart();
            ((IPrivateAxisSystem)mAxis).UniformUpdate();
            ((IPrivateChartDataSource)DataSource).UniformUpdate();
            foreach(IDataSeries series in PrivateSeriesData.DataSeries.Values)
                series.UniformUpdate();
        }

        private void Cat_FeatureRemoved(VisualFeatureHolder holder, VisualFeatureBase visualFeature, string name)
        {
            var cat = (DataSeriesCategory)holder;
            var series = PrivateSeriesData.GetSeries(cat.Name) as MultipleVisualFeaturesSeries;
            if (series == null)
                return;
            var removed = series.RemoveFeature(name);
            if (removed != null)
                removed.Destroy();
            ChartIntegrity.AsseetCollectionDistinct(series.FeatureNames, cat.FeatureNames);
        }
        
        private void Cat_FeatureAdded(VisualFeatureHolder holder, VisualFeatureBase visualFeature, string obj)
        {
            var cat = (DataSeriesCategory)holder;
            var series = PrivateSeriesData.GetSeries(cat.Name) as MultipleVisualFeaturesSeries;
            if (series == null)
                return;
            var featureObject = CreateEmptyObject(series.transform);
            featureObject.name = cat.Name + "-" + visualFeature.Name;
            var featureSeries = ((DataSeriesVisualFeature)visualFeature).GenerateSeries(featureObject);
            var prev = series.AddFeature(visualFeature.Name, featureSeries);
            if (prev != null)
                prev.Destroy();
            series.ApplySettings(cat, cat.Name, "");
            ChartIntegrity.AsseetCollectionDistinct(series.FeatureNames, cat.FeatureNames);
            
        }

        private void ValidateCategories()
        {
            foreach (DataSeriesCategory obj in mInvalidatedCategories)
            {
                if (obj == null)
                    continue;
                var series = PrivateSeriesData.GetSeries(obj.Name);
                if (series == null)
                    continue;
                series.ApplySettings(obj, obj.Name, "");
                ChartIntegrity.AsseetCollectionDistinct(PrivateSeriesData.Categories.Keys, PrivateSeriesData.DataSeries.Keys); // in the end of this call both should have the same names
            }
            mInvalidatedCategories.Clear();
        }
        private void Cat_CategoryDataChanged(DataSeriesCategory obj)
        {
            mInvalidatedCategories.Add(obj);
        }

        void EnsureEvents()
        {
            mSeriesData.CategoryAdded -= MSeriesData_CategoryAdded;
            mSeriesData.CategoryAdded += MSeriesData_CategoryAdded;
            mSeriesData.CategoryRemoved -= MSeriesData_CategoryRemoved;
            mSeriesData.CategoryRemoved += MSeriesData_CategoryRemoved;
            mAxis.OnAxisViewChanged -= MAxis_OnAxisViewChanged;
            mAxis.OnAxisViewChanged += MAxis_OnAxisViewChanged;
            mAxis.OnAxisTypeChanged -= MAxis_OnAxisTypeChanged;
            mAxis.OnAxisTypeChanged += MAxis_OnAxisTypeChanged;
        }

        private void MAxis_OnAxisTypeChanged(AxisSystem obj)
        {
            ValidateChart();
        }

        Transform CategoryParent
        {
            get
            {
                if (mCategoryObject == null)
                {
                    var obj = FindChildObject(transform, "DataSeries");
                    if (obj == null)
                    {
                        obj = CreateChildObject(transform);
                        obj.name = "DataSeries";
                    }
                    mCategoryObject = obj.transform;
                }
                ChartCommon.HideObject(mCategoryObject.gameObject);
                return mCategoryObject;
            }
        }
        protected IEnumerable<IDataSeries> DataSeriesObjects
        {
            get
            {
                foreach(DataSeriesCategory cat in PrivateSeriesData.Categories.Values)
                {
                    IDataSeries series = PrivateSeriesData.GetSeries(cat.Name);
                    if (series != null)
                        yield return series;
                }
            }
        }

        public void ValidateChart()
        {
            mInvalidated = false;
            EnsureEvents();
            DataSource.FitInto(mLocalFitPortion);
            PrivateAxisSystem.FitInto(mLocalFitPortion);
            PrivateAxisSystem.ValidateAxis();
            IEnumerable<DataSeriesCategory> ordered = PrivateSeriesData.Categories.Values.OrderBy(x => x.ViewOrder);

            foreach (DataSeriesCategory cat in ordered)
            {
                IDataSeries series =  PrivateSeriesData.GetSeries(cat.Name);
                if(series == null)
                {
                    series = GenerateCategorySeries(cat, CategoryParent);
                    CategoryAdded(series);
                    series.OnInit();
                    var obj = series.underlyingGameObject;
                    ChartCommon.ZeroLocalTransform(obj.transform);
                    series.ApplySettings(cat, cat.Name, "");
                    PrivateSeriesData.SetSeries(cat.Name, series);
                }
                else
                    series.ApplySettings(cat,cat.Name,"");
                
            }

            if (mTmpStringList == null)
                mTmpStringList = new List<string>();
            mTmpStringList.Clear();

            foreach(string name in PrivateSeriesData.DataSeries.Keys)    // remove excess objects
            {
                if (PrivateSeriesData.Categories.ContainsKey(name) == false)
                    mTmpStringList.Add(name);
            }

            for(int i=0; i<mTmpStringList.Count; i++)
            {
                string name = mTmpStringList[i];
                var removeSeries =  PrivateSeriesData.RemoveSeries(name);
                if(removeSeries != null)
                {
                    removeSeries.Destroy();
                }
            }

            ChartIntegrity.AsseetCollectionDistinct(PrivateSeriesData.Categories.Keys, PrivateSeriesData.DataSeries.Keys); // in the end of this call both should have the same names

        }

        void UnHookCategoryObject(IDataSeries series)
        {
            series.DataStartLoading -= Series_DataStartLoading;
            series.DataDoneLoading -= Series_DataDoneLoading;

        }
        void HookCategoryObject(IDataSeries series)
        {
            UnHookCategoryObject(series);
            series.DataStartLoading += Series_DataStartLoading;
            series.DataDoneLoading += Series_DataDoneLoading;

        }
        protected virtual void CategoryAdded(IDataSeries series)
        {
            HookCategoryObject(series);
        }

        protected virtual void CategoryRemoved(IDataSeries series)
        {
            Series_DataDoneLoading(series);
            UnHookCategoryObject(series);

        }

        private void Series_DataDoneLoading(IDataSeries series)
        {
            if (mLoadedCategories.Count == 0)
                return;
            mLoadedCategories.Remove(series);
            if (mLoadedCategories.Count == 0)
                DoneLoading();
        }

        private void Series_DataStartLoading(IDataSeries series)
        {
            if (mLoadedCategories.Count == 0)
                StartLoading();
            mLoadedCategories.Add(series);
        }

        protected virtual void StartLoading()
        {

        }

        protected virtual void DoneLoading()
        {

        }

        private void MSeriesData_CategoryRemoved(string name, DataSeriesCategory cat)
        {
            var removeSeries = PrivateSeriesData.RemoveSeries(name);
            if (removeSeries != null)
                removeSeries.Destroy();
            CategoryRemoved(removeSeries);
            ChartIntegrity.AsseetCollectionDistinct(PrivateSeriesData.Categories.Keys, PrivateSeriesData.DataSeries.Keys); // in the end of this call both should have the same names
        }

        private void MSeriesData_CategoryAdded(string name, DataSeriesCategory cat)
        {
            ((IPrivateDataSeriesCategory)cat).InnerData.SetView(Axis.ChartSpaceView);
            IDataSeries series = PrivateSeriesData.GetSeries(name);
            if (series == null)
            {
                series = GenerateCategorySeries(cat, CategoryParent);
                CategoryAdded(series);
                series.OnInit();
                var obj = series.underlyingGameObject;
                ChartCommon.ZeroLocalTransform(obj.transform);
            }
            else
                CategoryAdded(series);
            series.ApplySettings(cat, name, "");
            PrivateSeriesData.SetSeries(name, series);
            ChartIntegrity.AsseetCollectionDistinct(PrivateSeriesData.Categories.Keys, PrivateSeriesData.DataSeries.Keys); // in the end of this call both should have the same names
        }

        protected void OnValidate()
        {
            var dataSource = PrivateSeriesData;
            mSeriesData.SetParent(this);
            PrivateAxisSystem.SetParent(this);
            if (isActiveAndEnabled == false)
                return;
            if (dataSource != null)
                dataSource.OnValidate();

            if (mAxis != null)
                PrivateAxisSystem.OnValidate();
            Invalidate();
        }

        Transform IPrivateDataSeriesChart.getTransform()
        {
            return transform;
        }

        bool IPrivateDataSeriesChart.IsEnabled { get { return gameObject.activeSelf; } }

        IPrivateChartDataSource IPrivateDataSeriesChart.DataSource { get { return DataSource; } }
        GameObject IPrivateDataSeriesChart.CreateChildObject(Transform parent)
        {
            return CreateChildObject(parent);
        }

        IChartSeriesGraphic IPrivateDataSeriesChart.CreateEdgeGraphic(Transform parent)
        {
            return CreateEdgeGraphic(parent);
        }

        IPrivateAxisSystem IPrivateDataSeriesChart.Axis { get { return Axis; } }

        string IPrivateDataSeriesChart.DefaultDateFormat { get { return defaultDateFormat; } }

        string IPrivateDataSeriesChart.DefaultNumberFormat { get { return defaultNumberFormat; } }

        void IPrivateDataSeriesChart.MakeClippingObject(GameObject obj,bool clipping)
        {
            MakeClippableObject(obj, clipping);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {

        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {

        }

        GameObject IPrivateDataSeriesChart.FindChildObject(Transform parent, string name)
        {
            return FindChildObject(parent, name);
        }

        bool IPrivateDataSeriesChart.IsParameterDateType(string parameter)
        {
            if(mXParameters.Contains(parameter))
                return Axis.View.HorizontalIsDateTime;
            if (mYParameters.Contains(parameter))
                return Axis.View.VerticalIsDateTime;
            return false;
        }
    }
}
