using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace DataVisualizer{
    public abstract class VisualFeatureHolder : MonoBehaviour, IDataSeriesSettings, ISerializationCallbackReceiver , IVisualFeatureHolderPrivate,IPrivateSetName
    {

        [SerializeField]
        [FormerlySerializedAs("Name")]
        string innerName;

        public string Name
        {
            get { return innerName; }
            private set { innerName = value; }
        }
         void IPrivateSetName.SetName(string name)
        {
            Name = name;
        }
        public static readonly string VisualFeatureString = "VisualFeature-";
        public event Action<VisualFeatureHolder, VisualFeatureBase, string> FeatureAdded;
        public event Action<VisualFeatureHolder, VisualFeatureBase, string> FeatureRemoved;

        Dictionary<string, VisualFeatureBase> IVisualFeatureHolderPrivate.Properties
        {
            get
            {
                return mProperties;
            }
        }

        protected virtual void OnFeatureAdded(VisualFeatureBase visualProp, string name)
        {

            if (FeatureAdded != null)
                FeatureAdded(this, visualProp, name);
        }

        /// <summary>
        /// renames an existing visual property. returns true on success. returns false if the new name already exists or the current name is not found
        /// </summary>
        /// <param name="fromName">the current name of the visual property</param>
        /// <param name="toName">the new name of the visual property</param>
        /// <returns></returns>
        public virtual bool RenameVisualFeature(string fromName, string toName)
        {
            VisualFeatureBase p;
            if (mProperties.ContainsKey(toName))
                return false;
            if (mProperties.TryGetValue(fromName, out p) == false)
                return false;
            mProperties.Remove(fromName);
            ((IPrivateSetName)p).SetName(toName);
            mProperties.Add(toName, p);
            DataChanged();
            return true;
        }


        public IEnumerable<string> FeatureNames { get { return mProperties.Keys; } }
        /// <summary>
        /// Gets a visual property by it's name. if the property does not exist or the type T is not a valid cast , null is returned
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public T GetVisualFeature<T>(string name) where T : VisualFeatureBase, new()
        {
            VisualFeatureBase p;

            if (mProperties.TryGetValue(name, out p) == false)
            {
                ChartCommon.RuntimeWarning("visual property named " + name + " does not exist");
                return null;
            }

            if (!(p is T))
            {
                ChartCommon.RuntimeWarning("visual property named " + name + " is not of type " + typeof(T).Name + ". It is of type " + p.GetType().Name);
                return null;
            }
            return (T)p;
        }

        protected virtual void OnFeatureRemoved(VisualFeatureBase visualProp, string name)
        {
            if (FeatureRemoved != null)
                FeatureRemoved(this, visualProp, name);
        }

        public void OnBeforeSerialize()
        {
            mSerielizedProperties = mProperties.Values.ToArray();
        }

        public void OnAfterDeserialize()
        {
            mInnerProperties = null;
        }

        void EnsurePropeties()
        {
            if (mInnerProperties == null)
            {
                mInnerProperties = new Dictionary<string, VisualFeatureBase>();
                if (mSerielizedProperties == null)
                    return;
                for (int i = 0; i < mSerielizedProperties.Length; i++)
                {
                    VisualFeatureBase item = mSerielizedProperties[i];
                    if (item == null || item.Name == null)
                        continue;
                    item.VisualFeatureDataChanged -= Prop_VisualFeatureDataChanged; ;
                    item.VisualFeatureViewOrderChaged -= Prop_VisualFeatureViewOrderChaged;
                    item.VisualFeatureDataChanged += Prop_VisualFeatureDataChanged; ;
                    item.VisualFeatureViewOrderChaged += Prop_VisualFeatureViewOrderChaged;

                    mInnerProperties.Add(item.Name, item);
                }
            }
        }

        /// <summary>
        /// called when the inner order of the visual properties have changed
        /// </summary>
        protected abstract void OrderChanged();
        /// <summary>
        /// called when the settings or data of one of the visual properties have changed
        /// </summary>
        protected abstract void DataChanged();

        private void Prop_VisualFeatureViewOrderChaged(VisualFeatureBase obj)
        {
            OrderChanged();
        }

        private void Prop_VisualFeatureDataChanged(VisualFeatureBase obj)
        {
            DataChanged();
        }

        /// <summary>
        /// Add a visual property to this category. All visual properties are listed under 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        protected void InnerAddVisualFeature(string name, DataSeriesVisualFeature prefab)
        {
            if (mProperties.ContainsKey(name))
            {
                ChartCommon.RuntimeWarning("visual property named " + name + " already exist");
                return;
            }

            var prop = GameObject.Instantiate<GameObject>(prefab.gameObject, gameObject.transform).GetComponent<DataSeriesVisualFeature>();
            prop.name = Name + "-" + name;
            ((IPrivateSetName)prop).SetName(name);
            prop.VisualFeatureDataChanged += Prop_VisualFeatureDataChanged;
            prop.VisualFeatureViewOrderChaged += Prop_VisualFeatureViewOrderChaged;
            mProperties.Add(name, prop);
            OnFeatureAdded(prop, name);
        }


        /// <summary>
        /// removes a visual property by it's name. returns true if the property was removed from the category , or false if it never existed in the first place
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool RemoveVisualFeature(string name)
        {
            VisualFeatureBase p;
            if (mProperties.TryGetValue(name, out p) == false)
                return false;

            p.VisualFeatureDataChanged -= Prop_VisualFeatureDataChanged;
            p.VisualFeatureViewOrderChaged -= Prop_VisualFeatureViewOrderChaged;

            var settingObj = transform.Find(Name + "-" + p.Name);
            if(settingObj != null)
                ChartCommon.SafeDestroy(settingObj.gameObject);
            bool res = mProperties.Remove(name);
            OnFeatureRemoved(p, name);
            return res;
        }


        public void ClearUnusedObjects()
        {
            foreach (Transform t in transform)
            {
                if (t == null)
                    continue;
                if (mProperties.Values.Select(x => x.gameObject).Contains(t.gameObject))
                    continue;
                ChartCommon.SafeDestroy(t.gameObject);
            }
        }

        public object GetSetting(string name)
        {
            if (name == "Data")
                return null;
            if (name.StartsWith(VisualFeatureString))
            {
                name = name.Substring(VisualFeatureString.Length);
                VisualFeatureBase prop;
                if (mProperties.TryGetValue(name, out prop) == false)
                    return null;
                return prop;
            }

            var field = GetType().GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.FlattenHierarchy);
            if (field == null)
                return null;

            return field.GetValue(this);
        }

        /// <summary>
        /// returns true if a visual property with the given name exists in the category
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool HasVisualFeature(string name)
        {
            return mProperties.ContainsKey(name);
        }

        Dictionary<string, VisualFeatureBase> mInnerProperties = new Dictionary<string, VisualFeatureBase>();

        Dictionary<string, VisualFeatureBase> mProperties
        {
            get
            {
                EnsurePropeties();
                return mInnerProperties;
            }
        }

        [SerializeField]
        VisualFeatureBase[] mSerielizedProperties;

    }
}
