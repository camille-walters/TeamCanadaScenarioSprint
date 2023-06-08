using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThetaList;
using UnityEngine;

namespace DataVisualizer{
    [Serializable]
    public class ChartDataSource : IPrivateChartDataSource , ISerializationCallbackReceiver
    { 
        
        [SerializeField]
        private DataSeriesCategory[] mSerielizedCategories;

        private Dictionary<string, DataSeriesCategory> mCategories = new Dictionary<string, DataSeriesCategory>();
        private Dictionary<string, IDataSeries> mDataSeries = new Dictionary<string, IDataSeries>();
        public delegate GenericDataItem DataInterpolator(GenericDataItem from, GenericDataItem to, double t);
        double mMinX, mMinY, mMaxX, mMaxY, mXScroll, mYScroll, mXSize, mYSize, mXOut;
        ViewPortion mFitInto = new ViewPortion();
        bool mSupressSetEvent = false;
        IPrivateDataSeriesChart mParent;
        protected List<Slider> mSliders = new List<Slider>();
        
        public event Action<string,DataSeriesCategory> CategoryAdded;
        public event Action<string, DataSeriesCategory> CategoryRemoved;
        bool mCategoriesDirty = true;
        List<IDataSeriesOptimizationCondition> mOptimizationConditions = new List<IDataSeriesOptimizationCondition>();

        void RaiseCategoryAdded(string name, DataSeriesCategory cat)
        {
            if (CategoryAdded != null)
                CategoryAdded(name, cat);
        }

        void RaiseCategoryRemoved(string name, DataSeriesCategory cat)
        {
            if (CategoryRemoved != null)
                CategoryRemoved(name, cat);
        }

        Dictionary<string, DataSeriesCategory> IPrivateChartDataSource.Categories
        {
            get { return mCategories; }
        }

        Dictionary<string, IDataSeries> IPrivateChartDataSource.DataSeries
        {
            get { return mDataSeries; }
        }

        public void SetParent(IPrivateDataSeriesChart parent)
        {
            mParent = parent;
            GetSettingsObject();
        }

        public static GenericDataItem VectorInterpolator(GenericDataItem from, GenericDataItem to, double t)
        {
            GenericDataItem res = to;
            res.Position = DoubleVector3.LerpUnclamped(from.Position, to.Position, t);
            res.EndPosition = DoubleVector3.LerpUnclamped(from.EndPosition, to.EndPosition, t);
            return res;
        }

        public static GenericDataItem PositionInterpolator(GenericDataItem from, GenericDataItem to, double t)
        {
            GenericDataItem res = to;
            res.Position = DoubleVector3.LerpUnclamped(from.Position, to.Position, t);
            return res;
        }

        public class Slider
        {
            ChartDataSource mParent;
            public string category;
            public DataInterpolator mInterpolator;
            public int mFromIndex = -1;
            public int mIndex;
            public int mStackIndex;
            DoubleRect mBoundingRect;
            public GenericDataItem mFromValue;
            public GenericDataItem mToValue;
            public GenericDataItem mCurrent;

            /// <summary>
            /// Duration of the sliding action
            /// </summary>
            public double Duration { get; set; }

            /// <summary>
            /// Start time of the sliding action
            /// </summary>
            public double StartTime { get; set; }
            public Slider(ChartDataSource parent)
            {
                mParent = parent;
            }

            public DoubleVector2 Max
            {
                get
                {
                    return mBoundingRect.Max.ToDoubleVector2();
                }
            }

            public DoubleVector2 Min
            {
                get
                {
                    return mBoundingRect.Min.ToDoubleVector2();
                }
            }

            /// <summary>
            /// sets the slider so that it's value is mToValue. (the value the slider moves towards) 
            /// </summary>
            public void SetAsEndValue()
            {
                DataSeriesCategory data;
                if (mParent.mCategories.TryGetValue(category, out data) == false)
                    return;
                var items = ((IPrivateDataSeriesCategory)data).InnerData;
                if (mIndex < 0 || mIndex >= items.Count)
                    return;
                items.SetValue(mStackIndex,mIndex, mToValue);
                mBoundingRect = mToValue.BoundingVolume(items.Channels);
                ((IPrivateDataSeriesCategory)data).ModifyMinMax(mBoundingRect);
            }

            public bool Update()
            {
                DataSeriesCategory data;
                if (mInterpolator == null)
                    return true;
                if (mParent.mCategories.TryGetValue(category, out data) == false)
                    return true;

                var items =  ((IPrivateDataSeriesCategory)data).InnerData;
                if (mFromIndex >= 0)
                {
                    if (mFromIndex >= items.Count)
                        return true;
                    mFromValue = items.GetValue(mStackIndex,mFromIndex);
                }

                if (mIndex < 0 || mIndex >= items.Count)
                    return true;

                double time = Time.time;
                time -= StartTime;

                if (Duration <= 0.0001f)
                    time = 1f;
                else
                {
                    time /= Duration;
                    time = Math.Max(0.0, Math.Min(time, 1.0));
                }

                GenericDataItem v = mInterpolator(mFromValue, mToValue, time);
                mCurrent = v;
                mBoundingRect = mCurrent.BoundingVolume(items.Channels);
                mParent.mSupressSetEvent = true;
                items.SetValue(mStackIndex, mIndex, v);
                mParent.mSupressSetEvent = false;
                if (time >= 1f)
                {
                    items.SetValue(mStackIndex, mIndex, mToValue);
                    ((IPrivateDataSeriesCategory)data).ModifyMinMax(mBoundingRect);
                    return true;
                }

                return false;
            }
        }

        protected IDataSeries GetSeries(string category)
        {
            IDataSeries series;
            if (mDataSeries.TryGetValue(category, out series) == false)
                return null;
            return series;
        }

        public void FitInto(ViewPortion portion)
        {
            mFitInto = portion;
            foreach (IDataSeries series in mDataSeries.Values) 
                series.FitInto(portion);
        }
        
        public void Clear()
        {
            foreach (IDataSeries series in mDataSeries.Values)
                series.Destroy();
            mDataSeries.Clear();
            mSliders.Clear();
        }

        protected virtual void SetSeries(string category, IDataSeries series)
        {
            DataSeriesCategory cat;
            if (mCategories.TryGetValue(category, out cat) == false)
            { 
           //     Debug.LogWarningFormat("category {0} not found", category);
                return; 
            } 
            mDataSeries[category] = series;
            SetSeriesView(series);
            series.FitInto(mFitInto);
            series.OnSet(((IPrivateDataSeriesCategory)cat).InnerData);
        }

        protected virtual IDataSeries RemoveSeries(string category)
        {
            if (mDataSeries.ContainsKey(category) == false)
                return null;
            IDataSeries res = mDataSeries[category];
            mDataSeries.Remove(category);
            return res;
        }
        public bool HasCategory(string name)
        {
            EnsureCategories();
            return mCategories.ContainsKey(name);
        }
        
        public DataSeriesCategory GetCategory(string name)
        {
            EnsureCategories();
            DataSeriesCategory cat;
            
            if (mCategories.TryGetValue(name, out cat) == false)
                throw new Exception("no category named " + name);
            return cat;
        }

        public IEnumerable<DataSeriesCategory> Categories
        {
            get
            {
                EnsureCategories();
                return mCategories.Values;
            }
        }
        void UnhookCategoryEvents(DataSeriesCategory cat)
        {
            var holders = cat.EventHandlers as DataSeriesCategory.EventHandlersHolder;
            if (holders == null)
                return;
            var data = (IDataViewerNotifier)(((IPrivateDataSeriesCategory)cat).InnerData);
            
            data.OnAfterCommit -= holders.OnAfterCommit;
            data.OnBeforeCommit -= holders.OnBeforeCommit;
            data.OnBeforeRemove -= holders.OnBeforeRemove;
            data.OnRemove -= holders.OnRemove;
            data.OnSet -= holders.OnSet;
            data.OnSetArray -= holders.OnSetArray;
            data.OnInsert -= holders.OnInsert;
            data.OnAppendArray -= holders.OnAppendArray;
        }

        private void Data_OnAppendArray(DataSeriesCategory category, object arg1, ChannelType channel,int count)
        {
            var items = ((IPrivateDataSeriesCategory)category).InnerData;
            if (channel == ChannelType.Positions)
            {
                DoubleRect r = items.BoundLastPositions(count);
                if (r.IsNan == false) 
                    ((IPrivateDataSeriesCategory)category).ModifyMinMax(r);
            }
            else
            {
                for (int i = 0; i < items.StackCount; i++)
                {
                    for (int j = 0; j < items.Count; j++)
                    {
                        DoubleRect r = items.GetValue(i, j).BoundingVolume(items.Channels); // reapply min max from all stacks
                        ((IPrivateDataSeriesCategory)category).ModifyMinMax(r);
                    }
                }
            }
            mSliders.Clear();
        }

        void HookCategoryEvents(DataSeriesCategory cat)
        {
            UnhookCategoryEvents(cat);
            var data = (IDataViewerNotifier)(((IPrivateDataSeriesCategory)cat).InnerData);
            var holders = new DataSeriesCategory.EventHandlersHolder();
            cat.EventHandlers = holders;

            holders.OnAfterCommit = (object o, OperationTree<int> tree) => { Data_OnAfterCommit(cat, o, tree); };
            holders.OnBeforeCommit = (object o, OperationTree<int> tree) => { Data_OnBeforeCommit(cat, o, tree); };
            holders.OnInsert = (object o, int index) => { Data_OnInsert(cat, o, index); };
            holders.OnSet = (object o, int index) => { Data_OnSet(cat, o, index); };
            holders.OnSetArray = (object o, ChannelType channel) => { Data_OnSetArray(cat, o, channel); };
            holders.OnBeforeRemove = (object o, int index) => { Data_OnBeforeRemove(cat, o, index); };
            holders.OnRemove = (object o, int index) => { Data_OnRemove(cat, o, index); };
            holders.OnClear = (object o) => { Data_OnClear(cat, o); };
            holders.OnAppendArray = (object o, ChannelType channel,int count) => { Data_OnAppendArray(cat, o,channel, count); };
            data.OnAfterCommit += holders.OnAfterCommit;
            data.OnBeforeCommit += holders.OnBeforeCommit;
            data.OnBeforeRemove += holders.OnBeforeRemove;
            data.OnRemove += holders.OnRemove;
            data.OnSet += holders.OnSet;
            data.OnInsert += holders.OnInsert;
            data.OnClear += holders.OnClear;
            data.OnAppendArray += holders.OnAppendArray;
            data.OnSetArray += holders.OnSetArray;
        }

        private void Data_OnSetArray(DataSeriesCategory category, object obj, ChannelType channel)
        {
            if (mSupressSetEvent)
                return;
            var items = ((IPrivateDataSeriesCategory)category).InnerData;
            if (channel == ChannelType.Positions)
            {
                DoubleRect r = items.BoundPositions();
                if (r.IsNan == false)
                    ((IPrivateDataSeriesCategory)category).ModifyMinMax(r);
            }
            else
            {
                for (int i = 0; i < items.StackCount; i++)
                {
                    for (int j = 0; j < items.Count; j++)
                    {
                        DoubleRect r = items.GetValue(i, j).BoundingVolume(items.Channels); // reapply min max from all stacks
                        ((IPrivateDataSeriesCategory)category).ModifyMinMax(r);
                    }
                }
            }
            mSliders.Clear();
        }

        private void Data_OnClear(DataSeriesCategory category,object obj)
        {
            ClearSlidersFromCategory(category.name);
        }

        private void Data_OnInsert(DataSeriesCategory category, object arg1, int index)
        {
            var items = ((IPrivateDataSeriesCategory)category).InnerData;
            for (int i = 0; i < items.StackCount; i++)
            {
                DoubleRect r = items.GetValue(i, index).BoundingVolume(items.Channels); // reapply min max from all stacks
                ((IPrivateDataSeriesCategory)category).ModifyMinMax(r);
            }
            FixSliders(category.Name, index, 1);
        }

        private void Data_OnSet(DataSeriesCategory category, object arg1, int index)
        {
            if (mSupressSetEvent)
                return;
            var items = ((IPrivateDataSeriesCategory)category).InnerData;
            for (int i = 0; i < items.StackCount; i++)
            {
                DoubleRect r = items.GetValue(i, index).BoundingVolume(items.Channels); // reapply min max from all stacks
                ((IPrivateDataSeriesCategory)category).ModifyMinMax(r);  
            }
            CommitCaseSet(category.name, index);
        }

        private void Data_OnRemove(DataSeriesCategory category, object arg1, int index)
        {
            var items = ((IPrivateDataSeriesCategory)category).InnerData;
            if (index < 0 || index >= items.Count)
            {
                Debug.LogWarning("Invalid index , call is ignored");
                return;
            }
            FixSliders(category.Name,index,-1);
        }
        
        private void Data_OnBeforeRemove(DataSeriesCategory category, object arg1, int arg2)
        {

        }


        private void Data_OnBeforeCommit(DataSeriesCategory category, object arg1, ThetaList.OperationTree<int> tree)
        {
        }

        void CommitCaseInsert(string category,OperationTree<int>.OperationNode op)
        {
            for (int i = 0; i < mSliders.Count; i++)
            {
                var slider = mSliders[i] as Slider;
                if (slider == null)
                    continue;
                if (slider.category != category)
                    continue;
                if (slider.mIndex >= op.Index)
                    slider.mIndex++;
                if (slider.mFromIndex >= 0 && slider.mFromIndex >= op.Index)
                    slider.mFromIndex++;
            }
        }

        void CommitCaseSet(string category, int index)
        {
            mSliders.RemoveAll(x =>
            {
                var s = x as Slider;
                if (s == null)
                    return false;
                if (s.category != category)
                    return false;
                if (s.mIndex == index)
                    return true;
                return false;
            });
        }

        void CommitCaseRemove(string category,OperationTree<int>.OperationNode op)
        {
            for (int count = 0; count < op.Count; count++)
            {
                mSliders.RemoveAll(x =>
                {
                    var s = x as Slider;
                    if (s == null)
                        return false;
                    if (s.category != category)
                        return false;
                    if (s.mIndex == op.Index)
                        return true;
                    if (s.mIndex > op.Index)
                        s.mIndex--;                
                    if (s.mFromIndex>= 0 && s.mFromIndex >= op.Index)
                    {
                        s.mFromIndex--;
                        if (s.mFromIndex < 0)
                            return true;
                    }
                    return false;
                });
            }
        }

        void FixSlidersOnCommit(DataSeriesCategory category, ThetaList.OperationTree<int> tree)
        {
            string name = category.name;
            foreach (var op in tree.OperationData())
            {
                switch (op.Operation)
                {
                    case OperationTree<int>.OperationType.Insert:
                        CommitCaseInsert(name, op);
                        break;
                    case OperationTree<int>.OperationType.Remove:
                        CommitCaseRemove(name, op);
                        break;
                    case OperationTree<int>.OperationType.Set:
                        CommitCaseSet(name, op.Index);
                        break;
                }
            }
        }

        private void Data_OnAfterCommit(DataSeriesCategory category, object arg1, ThetaList.OperationTree<int> tree)
        {
            var items = ((IPrivateDataSeriesCategory)category).InnerData;
            for (int i = 0; i < items.StackCount; i++)
            {
                for (int j = 0; j < items.Count; j++)
                {
                    DoubleRect volume = items.GetValue(i, j).BoundingVolume(items.Channels);
                    ((IPrivateDataSeriesCategory)category).ModifyMinMax(volume);
                }
            }
            FixSlidersOnCommit(category, tree);
        }

        GameObject GetSettingsObject()
        {
            GameObject obj;
            GameObject parent = mParent.getTransform().gameObject;
            Transform t = parent.transform.Find("Settings");
            if (t != null)
                obj = t.gameObject;
            else
            {
                //      Debug.LogError("Settings object is null");
                obj = new GameObject("Settings");
                obj.transform.SetParent(parent.transform);
            }
            ChartCommon.HideObject(obj);
            return obj;
        }

        /// <summary>
        /// Adds a new category based on a BaseScrollableCategoryData object
        /// </summary>
        /// <param name="category"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual bool AddCategoryFromPrefab(string category,DataSeriesCategory prefab)
        {
            EnsureCategories();
            if (mCategories.ContainsKey(category))
                return false;
            GameObject settings = GetSettingsObject();
            DataSeriesCategory data = GameObject.Instantiate<GameObject>(prefab.gameObject, settings.transform).GetComponent<DataSeriesCategory>();
            ChartIntegrity.NotifyAddCollection(this, "categories", category);
            data.gameObject.name = category;
            ((IPrivateSetParent<ChartDataSource>)data.Data).SetParent(this);
            ((IPrivateSetName)data.Data).SetName(category);
            ((IPrivateSetName)data).SetName(category);
            HookCategoryEvents(data);
            mCategories.Add(category, data);
            RaiseCategoryAdded(category, data);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="category"></param>
        public void RemoveCategory(string category)
        {
            EnsureCategories();
            ChartIntegrity.NotifyRemoveCollection(this, "categories", category);
            DataSeriesCategory cat;
            if (mCategories.TryGetValue(category, out cat))
            {
                if (cat != null)
                    UnhookCategoryEvents(cat);
                mCategories.Remove(category);
                RaiseCategoryRemoved(category, cat);
                GameObject obj = GetSettingsObject();
                var catSetting = obj.transform.Find(cat.Name);
                if (catSetting != null)
                    ChartCommon.SafeDestroy(catSetting.gameObject);
            }
        }

        private void ClearSlidersFromCategory(string category)
        {
            mSliders.RemoveAll(x =>
            {
                var s = x as Slider;
                if (s == null)
                    return false;
                if (s.category != category)
                    return false;
                return true;
            });
        }

        /// <summary>
        /// fixes slider positioning after removal of an item
        /// </summary>
        /// <param name="removedIndex"></param>
        private void FixSliders(string category, int operationIndex,int weight)
        {
            mSliders.RemoveAll(x =>
            {
                var s = x as Slider;
                if (s == null)
                    return false;
                if (s.category != category)
                    return false;
                if (s.mIndex == operationIndex && weight == -1)
                    return true;
                if (s.mIndex > operationIndex)
                   s.mIndex += weight;
                if (s.mFromIndex >= 0 &&  s.mFromIndex >= operationIndex)
                {
                    s.mFromIndex += weight;
                    if (s.mFromIndex < 0)
                    {
                        s.SetAsEndValue();
                        return true;
                    }
                }
                return false;
            });
        }

        void IPrivateChartDataSource.UniformUpdate()
        {
            mSliders.RemoveAll(x =>
            {
                return x.Update();
            });
        }

        void IPrivateChartDataSource.NotifyViewChanged(IPrivateAxisSystem axis)
        {
            if (mParent == null)
                return;
            var viewPortion = mParent.Axis.ChartSpaceView;
            foreach (DataSeriesCategory cat in mCategories.Values)
                ((IPrivateDataSeriesCategory)cat).InnerData.SetView(viewPortion);
            
            foreach (IDataSeries series in mDataSeries.Values)
                series.OnSetView(viewPortion);
        }

        void SetSeriesView(IDataSeries series)
        {
            series.OnSetView(mParent.Axis.ChartSpaceView);
        }

        //public void AppendSlideValue(CategoryDataHolder holder, GenericDataItem item, GenericDataItem initial, double slideTime = 0.0, DataInterpolator interp = null)
        //{
        //    int index = holder.Count;
        //    if (slideTime <= 0.0 || holder.Advanced.Count == 0 || interp == null)
        //    {
        //        holder.Advanced.Append(item);
        //        return;
        //    }

        //    Slider s = new Slider(this);
        //    s.category = holder.Name;
        //    s.StartTime = Time.time;
        //    s.Duration = slideTime;
        //    s.mFromIndex = - 1;
        //    s.mFromValue = initial;
        //    s.mIndex = index;
        //    s.mInterpolator = interp;
        //    s.mStackIndex = 0;
        //    s.mToValue = item;

        //    holder.Advanced.Append(initial);
        //    mSliders.Add(s);
        //}
        //public GenericDataItem GetAbsoluteValue(CategoryDataHolder holder, int index)
        //{
        //    var slider = mSliders.Find(x => x.mIndex == index);
        //    if (slider != null)
        //        return slider.mToValue;

        //    return holder.Advanced.GetValue(0, index); 
        //}

        //public void InsertSlideValue(CategoryDataHolder holder, int index,GenericDataItem item, double slideTime = 0.0, DataInterpolator interp = null)
        //{
        //    if (slideTime <= 0.0 || holder.Advanced.Count == 0 || interp == null)
        //    {
        //        holder.Advanced.Insert(index, item);
        //        return;
        //    }

        //    Slider s = new Slider(this);
        //    s.category = holder.Name;
        //    s.StartTime = Time.time;
        //    s.Duration = slideTime;
        //    s.mFromIndex = index - 1;
        //    s.mIndex = index;
        //    s.mInterpolator = interp;
        //    s.mStackIndex = 0;
        //    s.mToValue = item;
        //    var from = holder.Advanced.GetValue(0, index);
        //    holder.Advanced.Insert(index,from);
        //    mSliders.Add(s);

        //}

        //public void AppendSlideValue(CategoryDataHolder holder, GenericDataItem item,double slideTime = 0.0,DataInterpolator interp = null)
        //{
        //    int index = holder.Advanced.Count;
        //    if (slideTime <= 0.0 || holder.Advanced.Count == 0 || interp == null)
        //    {
        //        holder.Advanced.Append(item);
        //        return;
        //    }

        //    Slider s = new Slider(this);
        //    s.category = holder.Name ;
        //    s.StartTime = Time.time;
        //    s.Duration = slideTime;
        //    s.mFromIndex = index - 1;
        //    s.mIndex = index;
        //    s.mInterpolator = interp;
        //    s.mStackIndex = 0;
        //    s.mToValue = item;
        //    var from = holder.Advanced.GetValue(0, s.mFromIndex);
        //    holder.Advanced.Append(from);
        //    mSliders.Add(s);
        //}

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            EnsureCategories();
            mSerielizedCategories = mCategories.Values.ToArray();
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            mCategoriesDirty = true;    // categories are dirty
//            mCategories = LoadData(); 
        }  

        protected Dictionary<string, DataSeriesCategory> LoadData()
        {
            var dic = new Dictionary<string, DataSeriesCategory>();
            if (mSerielizedCategories != null)
            { 
                for(int i=0; i< mSerielizedCategories.Length; i++)
                {
                    var cat = mSerielizedCategories[i];
                    dic.Add(cat.Name, cat);
                    ((IPrivateSetName)cat.Data).SetName(cat.Name);
                    ((IPrivateSetParent<ChartDataSource>)cat.Data).SetParent(this);
                    HookCategoryEvents(cat);
                }
            }
            return dic;
        }

        IDataSeries IPrivateChartDataSource.GetSeries(string category)
        {
            return GetSeries(category);
        }

        IDataSeries IPrivateChartDataSource.RemoveSeries(string category)
        {
            return RemoveSeries(category);
        }

        void IPrivateChartDataSource.SetSeries(string category, IDataSeries series)
        {
            SetSeries(category, series);
        }

        void LoadCategories()
        {
            mCategories = LoadData();
            mCategoriesDirty = false;
        }
        void EnsureCategories()
        {
            if (mCategoriesDirty == true)
                LoadCategories();
        }
        void IPrivateChartDataSource.OnValidate()
        {
            EnsureCategories();
            foreach (DataSeriesCategory cat in mCategories.Values)
                ((IPrivateDataSeriesCategory)cat).Validate();
        }

        void IPrivateChartDataSource.Awake()
        {
            EnsureCategories();
        }
    }
}
