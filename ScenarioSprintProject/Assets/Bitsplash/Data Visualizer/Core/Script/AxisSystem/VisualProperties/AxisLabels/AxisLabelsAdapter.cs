using Assets.Data_Visualizer.Script.DataSeries.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer
{
    class AxisLabelsAdapter : VisualObjectCollection
    {
        [ThreadStatic]
        static List<string> mInnerVisualProperties = new List<string>();

        static List<string> mVisualProperties
        {
            get
            {
                if (mInnerVisualProperties == null)
                    mInnerVisualProperties = new List<string>();
                return mInnerVisualProperties;
            }
        }

        string mCurrentItems = "";
        VisualObjectCollection mAxisObjects;
        Func<string, GameObject, TextDataHolder, AxisLabelsDataGenerator> mCreator;
        IDataSeriesSettings mSettings = null;

        public override void ApplySettings(IDataSeriesSettings  settings, string parentItemName, string visualFeatureName)
        {
            mSettings = settings;
            string items = (string)settings.GetSetting("ApplyToProperties");
            ChartCommon.DevLog(LogOptions.Axis, GetType().Name, "apply settings","items:",items);
            if(items != mCurrentItems)
            {
                mCurrentItems = items; 
                if (mCurrentItems == null)
                    mCurrentItems = "";                       
                VerifyItems();
            }
            foreach (var pair in mVisualObjects)
            {
                string nameStr = Name;
                if (nameStr == null)
                    nameStr = "";
                var labeled = mAxisObjects.GetVisualObject(pair.Key) as ILabeledObject;
                var overrideSettings = mSettings;
                if(labeled != null)
                    overrideSettings = new DataSeriesOverrideSettings(mSettings,ItemLabelsDataSeries.TextAxisDirectionSetting, labeled.LabelData.Direction);                
                pair.Value.ApplySettings(overrideSettings, nameStr, pair.Key);
            }
        //     base.ApplySettings(settings, parentItemName, visualFeatureName);
        }

        public void SetCreationDelegate(Func<string, GameObject, TextDataHolder, AxisLabelsDataGenerator> creator)
        {
            mCreator = creator;
        }

        protected override void OnSetParent()
        {
            base.OnSetParent();
            ChartCommon.DevLog(LogOptions.Axis, GetType().Name, "set parent");
            ((IPrivateAxisSystem)Parent.Axis).VisualObjectsChanged += AxisLabelsAdapter_VisualObjectsChanged;
            VerifyVisualObjects();
            VerifyItems();
        }
        
        void HookAxisObject()
        {
            UnhookAxisObject();
            if (mAxisObjects != null)
            {
                mAxisObjects.OnFeatureAdded += MAxisObjects_OnFeatureAdded;
                mAxisObjects.OnFeatureRemoved += MAxisObjects_OnFeatureRemoved;
            }
        }

        void UnhookAxisObject()
        {
            if(mAxisObjects != null)
            {
                mAxisObjects.OnFeatureAdded -= MAxisObjects_OnFeatureAdded;
                mAxisObjects.OnFeatureRemoved -= MAxisObjects_OnFeatureRemoved;
            }
        }

        private void MAxisObjects_OnFeatureRemoved(string name)
        {
            OnRemoveVisualObject(name);
        }

        private void MAxisObjects_OnFeatureAdded(string name)
        {
            OnAddVisualObject(name);
        }

        GameObject obtainGameObject(string name)
        {
            var obj = ((IPrivateDataSeriesChart)Parent).FindChildObject(transform, name);
            if (obj != null)
            {
                ChartCommon.DevLog(LogOptions.Axis, GetType().Name, "destorying old object", name);
                ChartCommon.SafeDestroy(obj);
            }
            ChartCommon.DevLog(LogOptions.Axis, GetType().Name, "creating game object", name);
            obj = ((IPrivateDataSeriesChart)Parent).CreateChildObject(transform);
            obj.name = name;
            return obj;
        }

        void OnRemoveVisualObject(string name)
        {
            ChartCommon.DevLog(LogOptions.Axis, GetType().Name, "removed visual object", name);
            var removed = RemoveVisualObject(name);
            if(removed != null)
                removed.Destroy();
        }

        void OnAddVisualObject(string name)
        {
            if (mAxisObjects == null)
            {
                ChartCommon.DevLog(LogOptions.Axis, GetType().Name, "add visual object","axis object null");
                return;
            }
            if (mCurrentItems.Split('|').Distinct().Contains(name) == false)
                return;
            
            ChartCommon.DevLog(LogOptions.Axis, GetType().Name, "add visual object", name);
            var obj = mAxisObjects.GetVisualObject(name) as ILabeledObject;
            if(obj != null)
            {
                GameObject newObj = obtainGameObject(name);
                var labelData = obj.LabelData;
                if (labelData != null)
                {
                    var axisLabelsDataGenerator = mCreator(name, newObj, labelData);
                    var overrideSettings = new DataSeriesOverrideSettings(mSettings, ItemLabelsDataSeries.TextAxisDirectionSetting, obj.LabelData.Direction);
                    AddFeature(name, axisLabelsDataGenerator, overrideSettings);                     
                }
            }
        }
        
        void VerifyVisualObjects()
        {
            ChartCommon.DevLog(LogOptions.Axis, GetType().Name, "verify visual objects");
            UnhookAxisObject();
            mAxisObjects = ((IPrivateAxisSystem)Parent.Axis).GetVisualObjects();
            ChartCommon.DevLog(LogOptions.Axis, GetType().Name, "obtained axis object", mAxisObjects == null ? "null" : " has value");
            HookAxisObject();
        }

        private void AxisLabelsAdapter_VisualObjectsChanged()
        {
            ChartCommon.DevLog(LogOptions.Axis, GetType().Name, "axis visual objects changed");
            VerifyVisualObjects();
            VerifyItems();
        }

        void VerifyItems()
        {
            if (mCurrentItems == null)
                mCurrentItems = "";
            ChartCommon.DevLog(LogOptions.Axis, GetType().Name, "verify items", "items:", mCurrentItems);
            string[] split = mCurrentItems.Split('|').Distinct().ToArray();
            for (int i=0; i<split.Length; i++)
            {
                string trim = split[i].Trim();
                split[i] = trim;
                var obj = GetVisualObject(trim);
                if (obj == null)
                {
                    ChartCommon.DevLog(LogOptions.Axis, GetType().Name, "verify items", "creating item:", trim);
                    OnAddVisualObject(trim);
                }
            }

            string[] toRemove = FeatureNames.Except(split).ToArray();
            for (int i = 0; i < toRemove.Length; i++)
            {
                ChartCommon.DevLog(LogOptions.Axis, GetType().Name, "verify items", "removing item:", toRemove[i]);
                OnRemoveVisualObject(toRemove[i]);
            }
        }
    }
}
