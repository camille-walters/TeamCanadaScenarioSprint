using Assets.Data_Visualizer.Core.Script.PreCompiled.Data;
using Assets.Data_Visualizer.Core.Script.PreCompiled.Data.DataViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThetaList;
using UnityEngine;
using UnityEngine.Serialization;

namespace DataVisualizer{ 
    [Serializable]
    public class DataSeriesCategory : VisualFeatureHolder , IPrivateDataSeriesCategory
    {
        public const string OptimizationTypeName = "optimization";

        DataBounds mDataBounds = new DataBounds();
        CategoryDataHolder mData;

        public object UserData { get; set; }

        public CategoryDataHolder Data
        {
            get
            {
                if (mData == null)
                    mData = new CategoryDataHolder(this);
                return mData;
            }
        }
        StackedGenericDataHolder IPrivateDataSeriesCategory.InnerData
        {
            get
            {
                return ((IPrivateCategoryDataHolder)Data).InnerData;
            }
        }
        [SerializeField]
        int mViewOrder = 0;

        /// <summary>
        /// categories are viewed in acending order. lower values are drawn first.
        /// </summary>
        public int ViewOrder
        {
            get { return mViewOrder; }
            set { mViewOrder = value; }
        }
        
        public class EventHandlersHolder
        {
            public Action<object,ChannelType> OnSetArray;
            public Action<object, int> OnSet;
            public Action<object, int> OnRemove;
            public Action<object, int> OnBeforeRemove;
            public Action<object, int> OnInsert;
            public Action<object, OperationTree<int>> OnAfterCommit;
            public Action<object, OperationTree<int>> OnBeforeCommit;
            public Action<object> OnClear;
            public Action<object, ChannelType,int> OnAppendArray;
        }
        
        public static readonly string ActiveSettings = "active";
        
        public object EventHandlers { get; set; }

        /// <summary>
        /// invoked when the underlying daya of the dataseries category has changed , This would require redraw of certain parts of the parent chart
        /// </summary>
        public event Action<DataSeriesCategory> CategoryDataChanged;
        public event Action<DataSeriesCategory> CategoryOrderChanged;
        public event Action<DataSeriesCategory> CategoryOptimizationChanged;
        protected void OptimizationChanged()
        {
            if (CategoryOptimizationChanged != null)
                CategoryOptimizationChanged(this);
        }
        protected override void OrderChanged()
        {
            if (CategoryOrderChanged != null)
                CategoryOrderChanged(this);
        }

        /// <summary>
        /// called when the data of this instance is changed and an event should be propergated to the parent class
        /// </summary>
        protected override void DataChanged()
        {
            if (CategoryDataChanged != null)
                CategoryDataChanged(this);
        }

        [SerializeField]
        /// true if the category is active , false if the category is disabled and should not be displayed
        private bool active = true;

        public bool Active
        {
            get { return active; }
            set
            {
                active = value;
                DataChanged();
            }
        }

        [SerializeField]
        private GraphOptimization optimization;
        private GraphOptimization Optimization
        {
            get { return optimization; }
            set 
            { 
                optimization = value;
                OptimizationChanged();
            }
        }
        //[SerializeField]
        /// the type of input used with this category, (for example: streaming,static,realtime)
        InputType inputOptimization = InputType.GeneralPurpose;

        public InputType InputOptimization
        {
            get { return inputOptimization; }
            set
            {
                inputOptimization = value;
                DataChanged();
            }
        }

        DataBounds IPrivateDataSeriesCategory.Bounds { get { return mDataBounds; } }



        void IPrivateDataSeriesCategory.ClearMinMax()
        {
            mDataBounds.Clear();
        }

        void IPrivateDataSeriesCategory.ModifyMinMax(DoubleRect boundingVolume)
        {
            mDataBounds.ModifyMinMax(boundingVolume);
        }

        void IPrivateDataSeriesCategory.ModifyMinMax(DoubleVector3 point)
        {
            mDataBounds.ModifyMinMax(point);
        }

        public void AddVisualFeature(string name, DataSeriesVisualFeature prefab)
        {
            InnerAddVisualFeature(name,prefab);
        }

        void IPrivateDataSeriesCategory.Validate()
        {
            ((IPrivateCategoryDataHolder)Data).InnerData.ValidateViews();
        }
    }
}
