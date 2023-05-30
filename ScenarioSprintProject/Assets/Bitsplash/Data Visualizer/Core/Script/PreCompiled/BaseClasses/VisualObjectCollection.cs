using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    public class VisualObjectCollection : MonoBehaviour, IChartVisualObject
    {
        List<string> mTmpRemoveList = new List<string>();

        public String Name { get; set; }
        protected bool IsActive { get; private set; }

        protected IPrivateDataSeriesChart Parent { get; private set; }
        bool mHasView = false, mHasFit = false;

        protected Dictionary<string, IChartVisualObject> mVisualObjects = new Dictionary<string, IChartVisualObject>();

        ViewPortion mLocalView;
        ViewPortion mChartSpaceView;

        public event Action<string> OnFeatureAdded;
        public event Action<string> OnFeatureRemoved;

        /// <summary>
        /// returns thep prev feature with the specified name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="feature"></param>
        /// <returns></returns>
        public IChartVisualObject AddFeature(string name, IChartVisualObject feature,IDataSeriesSettings settings)
        {
            ChartIntegrity.AssertMethodCalled((Action<DataSeriesChart>)SetParent);
            ChartCommon.DevLog(LogOptions.Axis, GetType().Name, "Validate Feature", name);
            IChartVisualObject res; 
            if (mVisualObjects.TryGetValue(name, out res) == false)
                res = null;
            feature.SetParent(Parent);
            mVisualObjects.Remove(name);
            mVisualObjects.Add(name, feature);
            if (feature != res)
            {
                ChartCommon.DevLog(LogOptions.Axis, GetType().Name, "Feature added", name);
                feature.OnInit();
                if (settings != null)
                    feature.ApplySettings(settings, Name, name);
                if (mHasFit)
                    feature.FitInto(mLocalView);
                if (mHasView)
                    feature.OnSetView(mChartSpaceView);

                if (OnFeatureAdded != null)
                    OnFeatureAdded(name);
                return res;
            }
            return null;
        }

        public IEnumerable<string> FeatureNames { get { return mVisualObjects.Keys; } }

        /// <summary>
        /// removes all items that are not contained in the supplied collection
        /// </summary>
        public void RemoveExcess(Dictionary<string,VisualFeatureBase> originalItems,Action<IChartVisualObject> onRemove)
        {
            mTmpRemoveList.Clear();
            foreach (string str in mVisualObjects.Keys)
            {
                if (originalItems.ContainsKey(str) == false)
                    mTmpRemoveList.Add(str);
            }
            for(int i=0; i<mTmpRemoveList.Count; i++)
            {
                string name = mTmpRemoveList[i];
                IChartVisualObject graphic;
                if(mVisualObjects.TryGetValue(name,out graphic))
                {
                    mVisualObjects.Remove(name);
                    if(OnFeatureRemoved != null)
                        OnFeatureRemoved(name);
                    onRemove(graphic);
                }
            }
        }
        /// <summary>
        /// returns the feature that was just removed
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IChartVisualObject RemoveVisualObject(string name)
        {
            ChartIntegrity.AssertMethodCalled((Action<DataSeriesChart>)SetParent);
            ChartCommon.DevLog(LogOptions.Axis, GetType().Name, "Remove Feature", name);
            IChartVisualObject res;
            if (mVisualObjects.TryGetValue(name, out res) == false)
                res = null;
            mVisualObjects.Remove(name);
            if (OnFeatureRemoved != null)
                OnFeatureRemoved(name);
            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IChartVisualObject GetVisualObject(string name)
        {
            IChartVisualObject res;
            if (mVisualObjects.TryGetValue(name, out res) == false)
                res = null;
            return res;
        }

        public GameObject underlyingGameObject { get { return gameObject; } }

        public ViewPortion CurrentView
        {
            get { return mChartSpaceView; }
        }

        public virtual void ApplySettings(IDataSeriesSettings settings, string parentItemName, string visualFeatureName)
        {
            ChartIntegrity.AssertMethodCalled((Action<DataSeriesChart>)SetParent);
            ChartCommon.DevLog(LogOptions.Axis, GetType().Name, "ApplySettings", visualFeatureName);
            bool active = ChartCommon.UnboxObject<bool>(settings.GetSetting("active"), true);
            if (active != IsActive)
                SetActive(active);
            foreach (var pair in mVisualObjects)
            {
                IDataSeriesSettings set = settings.GetSetting(DataSeriesCategory.VisualFeatureString + pair.Key) as IDataSeriesSettings;
                if (set != null)
                {
                    string nameStr = Name;
                    if (nameStr == null)
                        nameStr = "";
                    pair.Value.ApplySettings(set, nameStr, pair.Key);
                }
                else
                    ChartCommon.RuntimeWarning("setting item not found " + pair.Key);
            }
        }

        public void Destroy()
        {
            ChartCommon.DevLog(LogOptions.Axis, GetType().Name, "Destroy");
            foreach (IChartVisualObject s in mVisualObjects.Values)
                s.Destroy();
            mVisualObjects.Clear();
            ChartCommon.SafeDestroy(gameObject);
        }
        
        public void OnInit()
        {
            ChartIntegrity.NotifyMethodCall((Action)OnInit);
            ChartIntegrity.AssertMethodCalled((Action<DataSeriesChart>)SetParent);

            ChartCommon.DevLog(LogOptions.Axis, GetType().Name, "Init");
            foreach (IChartVisualObject s in mVisualObjects.Values)
                s.OnInit();
        }

        public void SetActive(bool active)
        {
            IsActive = active;
            gameObject.SetActive(active);
        }
        protected virtual void OnSetParent()
        {

        }
        public void SetParent(IPrivateDataSeriesChart parent)
        {
            ChartIntegrity.Assert(parent != null);
            ChartIntegrity.NotifyMethodCall((Action<DataSeriesChart>)SetParent);
            ChartCommon.DevLog(LogOptions.Axis, GetType().Name, "Set Parent");
            Parent = parent;
            foreach (IChartVisualObject s in mVisualObjects.Values)
                s.SetParent(parent);
            OnSetParent();
        }

        public void OnSetView(ViewPortion chartView)
        {
            ChartIntegrity.AssertMethodCalled((Action<DataSeriesChart>)SetParent);
            mHasView = true;
            mChartSpaceView = chartView;
            foreach (IChartVisualObject s in mVisualObjects.Values)
                s.OnSetView(mChartSpaceView);
        }

        public void FitInto(ViewPortion localView)
        {
            ChartIntegrity.AssertMethodCalled((Action<DataSeriesChart>)SetParent);
            mHasFit = true;
            mLocalView = localView;
            foreach (IChartVisualObject s in mVisualObjects.Values)
                s.FitInto(mLocalView);
        }

        public void UniformUpdate()
        {
            foreach (IChartVisualObject s in mVisualObjects.Values)
                s.UniformUpdate();
        }

        public void SetVisible(bool visible)
        {
            
        }
    }
}
