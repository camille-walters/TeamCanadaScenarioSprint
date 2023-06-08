using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    public abstract class VisualFeatureBase : MonoBehaviour, IDataSeriesSettings, IPrivateSetName
    {
        [HideInInspector]
        [SerializeField]
        private string mName;

        public string Name
        {
            get { return mName; }
        }

        [HideInInspector]
        [SerializeField]
        private int mViewOrder;

        /// <summary>
        /// used internally to dispaly visual properties in the inspector
        /// </summary>
        public int ViewOrder
        {
            get { return mViewOrder; }
            set
            {
                mViewOrder = value;
                ViewOrderChanged();
            }
        }

        [SerializeField]
        [HideInInspector]
        protected int canvasSortOrder;

        /// <summary>
        /// the sort order for canavas visual properties. This sort order is applied to the canvas that holds the visual property
        /// </summary>
        public int CanvasSortOrder
        {
            get { return canvasSortOrder; }
            set
            {
                canvasSortOrder = value;
                ViewOrderChanged();
            }
        }

        [SerializeField]
        /// true if the category is active , false if the category is disabled and should not be displayed
        protected bool active = true;

        private event Action<VisualFeatureBase> InnerVisualFeatureDataChanged;
        /// <summary>
        /// invoked when the underlying data of this instance has changed. This notifies the parent classes that the chart should redraw some parts of itself
        /// </summary>
        public event Action<VisualFeatureBase> VisualFeatureDataChanged
        {
            add
            {
                OnHookDataChanged();
                InnerVisualFeatureDataChanged += value;
            }
            remove
            {
                InnerVisualFeatureDataChanged -= value;
                OnUnhookDataChanged();
            }
        }
        public event Action<VisualFeatureBase> VisualFeatureViewOrderChaged;

        protected virtual void OnHookDataChanged()
        {

        }
        protected virtual void OnUnhookDataChanged()
        {

        }
        /// <summary>
        /// called when the view order has changed
        /// </summary>
        protected void ViewOrderChanged()
        {
            if (VisualFeatureViewOrderChaged != null)
                VisualFeatureViewOrderChaged(this);
        }
        /// <summary>
        /// called when the underlying data has changed. This invokes the VisualFeatureDataChanged event
        /// </summary>
        protected void DataChanged()
        {
            if (InnerVisualFeatureDataChanged != null)
                InnerVisualFeatureDataChanged(this);
        }


        public bool Active
        {
            get { return active; }
            set
            {
                active = value;
                DataChanged();
            }
        }

        public abstract string VisualFeatureTypeName
        {
            get;
        }

        /// <summary>
        /// gets the setting with the specified name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object GetSetting(string name)
        {
            var field = GetType().GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.FlattenHierarchy);
            if (field == null)
                return null;
            return field.GetValue(this);
        }

        void IPrivateSetName.SetName(string name)
        {
            mName = name;
        }
    }
}
