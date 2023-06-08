
using Assets.Data_Visualizer.Core.Script.PreCompiled.DataSeries.Core;
using Assets.Data_Visualizer.Script.DataSeries.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    class MultipleVisualFeaturesSeries : MonoBehaviour, IDataSeries, LoadingNotifier.ILoadingEventRaiser
    {
        protected bool IsActive { get; private set; }

        Dictionary<string, IDataSeries> mVisualFeatures = new Dictionary<string, IDataSeries>();
        HashSet<IDataSeries> mLoading = new HashSet<IDataSeries>();
        IDataViewerNotifier mViewer;
        ViewPortion mLocalView;
        ViewPortion mChartSpaceView;
        LoadingNotifier mLoadingNotifier;
        IPrivateDataSeriesChart mParent;
        bool mHasView = false, mHasFit = false;
        bool mVisible = true;


        public MultipleVisualFeaturesSeries()
        {
            IsActive = true;
            mLoadingNotifier = new LoadingNotifier(this);
        }

        public event Action<IDataSeries> DataStartLoading;
        public event Action<IDataSeries> DataDoneLoading;


        IEnumerable<IDataSeries> LoadingNotifier.ILoadingEventRaiser.CurrentItems
        {
            get
            {
                return mVisualFeatures.Values;
            }
        }

        void LoadingNotifier.ILoadingEventRaiser.RaiseStartLoading()
        {
            if (DataStartLoading != null)
                DataStartLoading(this);
        }

        void LoadingNotifier.ILoadingEventRaiser.RaiseDoneLoading()
        {
            if (DataDoneLoading != null)
                DataDoneLoading(this);
        }

        /// <summary>
        /// returns thep prev feature with the specified name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="feature"></param>
        /// <returns></returns>
        public IDataSeries AddFeature(string name,IDataSeries feature)
        {
            IDataSeries res;
            if(mVisualFeatures.TryGetValue(name, out res) == false)
                res = null;
            feature.SetParent(mParent);
            mVisualFeatures.Remove(name);
            mVisualFeatures.Add(name, feature);
            feature.DataStartLoading += Feature_DataStartLoading;
            feature.DataDoneLoading += Feature_DataDoneLoading;
            if (feature != res)
            {
                feature.OnInit();
                if(mHasFit)
                    feature.FitInto(mLocalView);
                if (mHasView)
                    feature.OnSetView(mChartSpaceView);
                if (mViewer != null)
                    feature.OnSet(mViewer);
                if (mParent != null)
                    feature.SetParent(mParent);
                feature.SetVisible(mVisible);
                return res;
            }
            return null;
        }

        void RemoveUnusedLoading()
        {
            mLoading.IntersectWith(mVisualFeatures.Values);
        }
        private void Feature_DataStartLoading(IDataSeries obj)
        {
            if(IsActive)
                mLoadingNotifier.NotifyStart(obj);
        }

        private void Feature_DataDoneLoading(IDataSeries obj)
        {
            mLoadingNotifier.NotifyDone(obj);
        }

        /// <summary>
        /// returns the feature that was just removed
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IDataSeries RemoveFeature(string name)
        {
            IDataSeries res;
            if (mVisualFeatures.TryGetValue(name, out res) == false)
                res = null;
            mVisualFeatures.Remove(name);
            if(res != null)
                mLoadingNotifier.NotifyDone(res);
            res.DataStartLoading -= Feature_DataStartLoading;
            res.DataDoneLoading -= Feature_DataDoneLoading;
            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IDataSeries GetFeature(string name)
        {
            IDataSeries res;
            if (mVisualFeatures.TryGetValue(name, out res) == false)
                res = null;
            return res;
        }

        public IEnumerable<string> FeatureNames { get { return mVisualFeatures.Keys; } }
        public int Count
        {
            get
            {
                if (mViewer == null)
                    return 0;
                return mViewer.Count;
            }
        }

        public ViewPortion CurrentView
        {
            get
            {
                return mChartSpaceView;
            }
        }

        public GameObject underlyingGameObject
        {
            get
            {
                return gameObject;
            }
        }

        public int CanvasViewOrder
        {
            get { return 0; }
        }



        public void ApplySettings(IDataSeriesSettings settings, string categoryName, string visualFeatureName)
        {
            bool active = ChartCommon.UnboxObject<bool>(settings.GetSetting("active"), true);
            if (active != IsActive)
                SetActive(active);
            foreach (var pair in mVisualFeatures)
            {
                IDataSeriesSettings set = settings.GetSetting(DataSeriesCategory.VisualFeatureString + pair.Key) as IDataSeriesSettings;
                if (set != null)
                {
                    set = new DataSeriesParentSettings(settings, set);
                    pair.Value.ApplySettings(set, categoryName, pair.Key);
                }
                else
                    ChartCommon.RuntimeWarning("setting item not found " + pair.Key);
            }
            SortObjects();
        }

        public void Destroy()
        {
            foreach (IDataSeries s in mVisualFeatures.Values)
                s.Destroy();
            mVisualFeatures.Clear();
            ChartCommon.SafeDestroy(gameObject);
        }

        public void FitInto(ViewPortion localView)
        {
            mHasFit = true;
            mLocalView = localView;
            foreach (IDataSeries s in mVisualFeatures.Values)
                s.FitInto(mLocalView);
        }

        public bool IsValidForOptimization(string name)
        {
            foreach (IDataSeries s in mVisualFeatures.Values)
            {
                if(s.IsValidForOptimization(name) == false)
                    return false;
            }
            return true;
        }

        public void OnInit()
        {
            foreach (IDataSeries s in mVisualFeatures.Values)
                s.OnInit();
        }

        public void OnSet(IDataViewerNotifier data)
        {
            mViewer = data;
            foreach (IDataSeries s in mVisualFeatures.Values)
                s.OnSet(data);
        }

        void SortObjects()
        {
            var arr = mVisualFeatures.Values.OrderBy(x => x.CanvasViewOrder).ToArray();
            for(int i=0; i<arr.Length; i++)
                arr[i].underlyingGameObject.transform.SetSiblingIndex(i);
        }

        public void OnSetView(ViewPortion chartView)
        {
            mHasView = true;
            mChartSpaceView = chartView;
            foreach (IDataSeries s in mVisualFeatures.Values)
                s.OnSetView(mChartSpaceView);
        }

        public void SetActive(bool active)
        {
            IsActive = active;
            gameObject.SetActive(active);
        }

        public void SetParent(IPrivateDataSeriesChart parnet)
        {
            mParent = parnet;
            foreach (IDataSeries s in mVisualFeatures.Values)
                s.SetParent(parnet);
        }

        public void UniformUpdate()
        {
            foreach (IDataSeries s in mVisualFeatures.Values)
                s.UniformUpdate();
        }

        public void SetVisible(bool visible)
        {
            if (mVisible != visible)
            {
                mVisible = visible;
                foreach (IDataSeries s in mVisualFeatures.Values)
                    s.SetVisible(visible);
            }
        }
    }
}
