using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{ 
    /// <summary>
    /// base class for all axis systems and views 
    /// </summary>
    [Serializable]
    public class AxisSystem : VisualFeatureHolder, IPrivateAxisSystem
    {
        IPrivateDataSeriesChart mParent;
        ViewPortion mLocalView;
        bool mIsXViewDate, mIsYViewDate;
        ViewPortion mChartSpaceView = new ViewPortion(0.0, 0.0, 1.0, 1.0, 2.0,false,false);
        MappingFunction? mChartToLocal;
        MappingFunction? mLocalToChart;
        bool mHasFit = false;
        bool mApplySettings = false;
        Transform mAxisFeatureGraphicsObject;
        Transform mAxisFeatureGraphicsObjectTopLevel;
        VisualObjectCollection mVisualObjects;

        DataBounds mDataBounds;
        bool mEnabled = true;
        event Action InnerVisualObjectsChanged;

        event Action IPrivateAxisSystem.VisualObjectsChanged
        {
            add
            {
                InnerVisualObjectsChanged += value;
            }

            remove
            {
                InnerVisualObjectsChanged -= value;
            }
        }

        void InternalSetEnabled(bool enabled)
        {
            mEnabled = enabled;
            if(mEnabled)
            {
                GenerateVisualObjects();
                ValidateAxis();
            }
            else
            {
                if (mVisualObjects != null)
                {
                    mVisualObjects.Destroy();
                    ChartCommon.SafeDestroy(mVisualObjects);
                }
                if (mAxisFeatureGraphicsObject != null)
                    ChartCommon.SafeDestroy(mAxisFeatureGraphicsObject.gameObject);
                if (mAxisFeatureGraphicsObjectTopLevel != null)
                    ChartCommon.SafeDestroy(mAxisFeatureGraphicsObjectTopLevel.gameObject);
                mVisualObjects = null;
                mAxisFeatureGraphicsObject = null;
                mAxisFeatureGraphicsObjectTopLevel = null;
            }
        }

        void UpdateMappingFunctions()
        {
            if (mHasFit)
            {
                mChartToLocal = new MappingFunction(mChartSpaceView.ToRect(), mLocalView.ToRect());
                mLocalToChart = new MappingFunction(mLocalView.ToRect(), mChartSpaceView.ToRect());
            }
        }

        void IPrivateAxisSystem.SetEnabled(bool enabled)
        {
            InternalSetEnabled(enabled);
        }

        void CreateAxisFeatureObject()
        {
            var obj = ((IPrivateDataSeriesChart)mParent).FindChildObject(mParent.getTransform(), "Axis");
            if (obj != null)
                ChartCommon.SafeDestroy(obj);
            obj = ((IPrivateDataSeriesChart)mParent).FindChildObject(mParent.getTransform(), "AxisTopLevel");
            if (obj != null)
                ChartCommon.SafeDestroy(obj);

            obj = ((IPrivateDataSeriesChart)mParent).CreateChildObject(mParent.getTransform());
            obj.transform.SetSiblingIndex(0); 
            obj.name = "Axis";
            ChartCommon.HideObject(obj);
            obj.SetActive(mEnabled);
            mAxisFeatureGraphicsObject = obj.transform;

            obj = ((IPrivateDataSeriesChart)mParent).CreateChildObject(mParent.getTransform());
            obj.transform.SetAsLastSibling();
            obj.name = "AxisTopLevel";
            ChartCommon.HideObject(obj);
            obj.SetActive(mEnabled);

            var interactionManager = mParent.getTransform().Find("Interactions");
            if (interactionManager != null)
                interactionManager.transform.SetAsLastSibling();

            var loadingOverlay = mParent.getTransform().Find("LoadingOverlay");
            if (loadingOverlay != null) // loading overlay is always on top
                loadingOverlay.transform.SetAsLastSibling();
            mAxisFeatureGraphicsObjectTopLevel = obj.transform;
        }

        VisualObjectCollection IPrivateAxisSystem.GetVisualObjects()
        {
            return mVisualObjects;
        }
#pragma warning disable 0108
        public String Name { get; protected set; } 
#pragma warning restore 0108
        public bool IsActive { get; set; }

        bool IPrivateAxisSystem.IsActive { get { return IsActive; } }

        public event Action<AxisSystem> OnAxisViewChanged;
        /// <summary>
        /// called when an axis type has changed between date and number
        /// </summary>
        public event Action<AxisSystem> OnAxisTypeChanged;
        
        [SerializeField] 
        AxisSystemView view = new AxisSystemView();

        public AxisSystemView View { get { return view; } }

        protected void FitInto(ViewPortion localView)
        {
            mHasFit = true; 
            mLocalView = localView;
            UpdateMappingFunctions();
            if (mVisualObjects != null)
                mVisualObjects.FitInto(localView);
        }

        public ViewPortion ChartSpaceView { get { return mChartSpaceView; } }
        public ViewPortion LocalView { get { return mLocalView; } }
        HashSet<DataSeriesBase> mRegisteredDataSeries = new HashSet<DataSeriesBase>();
        public bool LocalViewContains(Vector3 localPoint)
        {
            return LocalView.ToRect().Contains(localPoint);
        }
        void RaiseOnAxisViewChanged()
        {
            if (OnAxisViewChanged != null)
                OnAxisViewChanged(this);
        }
        void RaiseOnAxisTypeChanged()
        {
            if (OnAxisTypeChanged != null)
                OnAxisTypeChanged(this);
        }

        protected virtual void RegisterDataSeries(DataSeriesBase mapper)
        {
            mRegisteredDataSeries.Add(mapper);
        }

        protected virtual void UnregisterDataSeries(DataSeriesBase mapper)
        {
            mRegisteredDataSeries.Remove(mapper);
        }

        void IPrivateAxisSystem.FitInto(ViewPortion localView)
        {
            FitInto(localView);
        }

        void IPrivateAxisSystem.RegisterDataSeries(DataSeriesBase mapper)
        {
            RegisterDataSeries(mapper);
        }

        void IPrivateAxisSystem.UnregisterDataSeries(DataSeriesBase mapper)
        {
            UnregisterDataSeries(mapper);
        }

        public Vector3 ChartSpaceToLocalSpace(DoubleVector3 chartSpace)
        {
            if (mHasFit == false || mChartToLocal.HasValue == false)
                return new Vector2();

            Vector3 localSpace =  mChartToLocal.Value.MapToFloat(chartSpace);
            if (mChartSpaceView.OppositeX)
                localSpace.x = (float)(mLocalView.To.x - localSpace.x + mLocalView.From.x);
            if (mChartSpaceView.OppositeY)
                localSpace.y = (float)(mLocalView.To.y - localSpace.y + mLocalView.From.y);
            return localSpace;
        }

        public DoubleVector3 LocalSpaceToChartSpace(Vector3 localSpace)
        {
            if(mHasFit == false || mLocalToChart.HasValue == false)
                    return new DoubleVector3();
            if (mChartSpaceView.OppositeX)
                localSpace.x = (float)(mLocalView.To.x - localSpace.x + mLocalView.From.x);
            if (mChartSpaceView.OppositeY)
                localSpace.y = (float)(mLocalView.To.y - localSpace.y + mLocalView.From.y);
            return mLocalToChart.Value.MapVector(new DoubleVector3(localSpace));
        }
        void CalculateBounds()
        {
            if (mParent == null)
                return;
            if (mDataBounds == null)
                mDataBounds = new DataBounds();
            var dataSource = mParent.DataSource;
            mDataBounds.Clear();
          
            foreach (DataSeriesCategory cat in dataSource.Categories.Values)
            {
                if (cat.Active == false)
                    continue;
                mDataBounds.ModifyMinMax(((IPrivateDataSeriesCategory)cat).Bounds);
            }

            if (mDataBounds.MinX.HasValue == false)
                mDataBounds.MinX = 0.0;
            if (mDataBounds.MinY.HasValue == false)
                mDataBounds.MinY = 0.0;
            if (mDataBounds.MaxX.HasValue == false)
                mDataBounds.MaxX = mDataBounds.MinX.Value + 0.0000001;
            if (mDataBounds.MaxY.HasValue == false)
                mDataBounds.MaxY = mDataBounds.MinY.Value + 0.0000001;
        }
        
        void CalculateViewPortions()
        {
            CalculateBounds();
            bool isXDate = View.HorizontalIsDateTime;
            bool isYDate = View.VerticalIsDateTime;
            ViewPortion newChartSpace = GetScrollParams();
            if (newChartSpace.IsZeroSize())
            {
                //ChartIntegrity.Assert(false); // should not be zero size
                newChartSpace.MakeIdentitySize();
            }
            if(isXDate != mIsXViewDate || isYDate != mIsYViewDate)
            {
                mIsXViewDate = isXDate;
                mIsYViewDate = isYDate;
                RaiseOnAxisTypeChanged();
            }
            if (mChartSpaceView.CompareWithError(newChartSpace,0.00001) == false)
            {
                mChartSpaceView = newChartSpace;
                UpdateMappingFunctions();
                if (mVisualObjects != null)
                    mVisualObjects.OnSetView(mChartSpaceView);
                RaiseOnAxisViewChanged();  
            }              
        }

        void IPrivateAxisSystem.UniformUpdate()
        {
            if (mApplySettings == true)
                ApplySettings();
            CalculateViewPortions();
            if(mParent.IsEnabled != mEnabled)
                InternalSetEnabled(mParent.IsEnabled);
            if (mVisualObjects != null)
                mVisualObjects.UniformUpdate();
        }
        
        void GenerateVisualObjects()
        {
            if (mVisualObjects != null)
                ChartCommon.SafeDestroy(mVisualObjects);
            CalculateViewPortions();
            CreateAxisFeatureObject();
            var obj = mAxisFeatureGraphicsObject.gameObject;
            var topObj = mAxisFeatureGraphicsObjectTopLevel.gameObject;
            ChartCommon.ZeroLocalTransform(obj.transform);
            ChartCommon.ZeroLocalTransform(topObj.transform);
            var prevVisualObjects = mVisualObjects;
            mVisualObjects = obj.GetComponent<VisualObjectCollection>();
            
            if(mVisualObjects != prevVisualObjects)
            {
                if (InnerVisualObjectsChanged != null)
                    InnerVisualObjectsChanged();
            }

            if (mVisualObjects == null)
            {
                mVisualObjects = obj.AddComponent<VisualObjectCollection>();
                mVisualObjects.SetParent(mParent);
                mVisualObjects.OnInit();
            }
            else
                mVisualObjects.SetParent(mParent);

            mVisualObjects.OnSetView(mChartSpaceView);
            if (mHasFit)
                mVisualObjects.FitInto(mLocalView);


            //ChartIntegrity.Assert(() =>
            //{
            //    if (mParent.transform.GetComponentsInChildren<VisualObjectCollection>().Length > 1)   // can't have more then one of this type in the entire hirarchy
            //    {
            //        ChartCommon.DevLog(LogOptions.Axis, "To many visual object collections", mParent.transform.GetComponentsInChildren<VisualObjectCollection>().Length);
            //        return false;
            //    }
            //    return true;
            //});
        }

        void EnsureVisualObejcts()
        {
            if (mVisualObjects == null)
                GenerateVisualObjects();
        }
        
        protected override void OnFeatureAdded(VisualFeatureBase visualProp, string name)
        {
            base.OnFeatureAdded(visualProp, name);
            var visualFeature = (AxisVisualFeature)visualProp;
            var axisVisualObject =  GenerateFeatureObject(visualFeature, name);
            axisVisualObject.ApplySettings(visualFeature, "Axis",name);
            ChartIntegrity.AsseetCollectionDistinct(mVisualObjects.FeatureNames, ((IVisualFeatureHolderPrivate)this).Properties.Keys);    // the items in visualObjets should match those defined in the properties
        }

        protected override void OnFeatureRemoved(VisualFeatureBase visualProp, string name)
        {
            var obj = mVisualObjects.RemoveVisualObject(name);
            if(obj != null)
                obj.Destroy();
            base.OnFeatureRemoved(visualProp, name);
            ChartIntegrity.AsseetCollectionDistinct(mVisualObjects.FeatureNames, ((IVisualFeatureHolderPrivate)this).Properties.Keys);    // the items in visualObjets should match those defined in the properties
        }

        IChartVisualObject GenerateFeatureObject(AxisVisualFeature visualFeature,string name)
        {
            ChartIntegrity.Assert(mParent != null);
            string objectName = "Axis-" + visualFeature.VisualFeatureTypeName + "-" + name;

            var prevObject = ((IPrivateDataSeriesChart)mParent).FindChildObject(mAxisFeatureGraphicsObject, objectName);
            if (prevObject != null)
                ChartCommon.SafeDestroy(prevObject);

            prevObject = ((IPrivateDataSeriesChart)mParent).FindChildObject(mAxisFeatureGraphicsObjectTopLevel, objectName);
            if (prevObject != null)
                ChartCommon.SafeDestroy(prevObject);

            GameObject obj = null;
            if (visualFeature.TopLevel)
                obj = ((IPrivateDataSeriesChart)mParent).CreateChildObject(mAxisFeatureGraphicsObjectTopLevel);
            else
                obj = ((IPrivateDataSeriesChart)mParent).CreateChildObject(mAxisFeatureGraphicsObject);

            var axisVisualObject = visualFeature.GenerateAxisObject(obj);
            axisVisualObject.underlyingGameObject.name = objectName;
            var prev = mVisualObjects.AddFeature(name, axisVisualObject,visualFeature);
            ChartIntegrity.Assert(prev == null); 
            if (prev != null)   // this should not happen
                prev.Destroy();
            return axisVisualObject;
        }

        void ValidateAxis()
        {
            EnsureVisualObejcts();
            mVisualObjects.SetParent(mParent);
            var privateHolder = (IVisualFeatureHolderPrivate)this;

            foreach(var pair in privateHolder.Properties)
            {
                var visualFeature = (AxisVisualFeature)pair.Value;
                var axisVisualObject = mVisualObjects.GetVisualObject(pair.Key);
                if (axisVisualObject != null)
                {
                    if(visualFeature.TopLevel && axisVisualObject.underlyingGameObject.transform.parent != mAxisFeatureGraphicsObjectTopLevel)
                        axisVisualObject.underlyingGameObject.transform.SetParent(mAxisFeatureGraphicsObjectTopLevel);
                    else if(!visualFeature.TopLevel && axisVisualObject.underlyingGameObject.transform.parent != mAxisFeatureGraphicsObject)
                        axisVisualObject.underlyingGameObject.transform.SetParent(mAxisFeatureGraphicsObject);
                }
                if (axisVisualObject == null)
                    axisVisualObject = GenerateFeatureObject(visualFeature, pair.Key);            
                axisVisualObject.ApplySettings(visualFeature, "Axis", pair.Key);    // apply the settings to the object
            }

            mVisualObjects.RemoveExcess(privateHolder.Properties, (x) =>
            {   
                try
                {
                    x.Destroy();
                    ChartCommon.SafeDestroy(x.underlyingGameObject);
                }
                catch(Exception)
                {

                }
            });

            ChartIntegrity.AsseetCollectionDistinct(mVisualObjects.FeatureNames, ((IVisualFeatureHolderPrivate)this).Properties.Keys);    // the items in visualObjets should match those defined in the properties
        }

        
        protected override void OrderChanged()
        {
            ValidateAxis();
        }
        private void ApplySettings()
        {
            mVisualObjects.ApplySettings(this, "Axis", "");
            ChartIntegrity.AsseetCollectionDistinct(mVisualObjects.FeatureNames, ((IVisualFeatureHolderPrivate)this).Properties.Keys);    // the items in visualObjets should match those defined in the properties
            mApplySettings = false;
        }
        protected override void DataChanged()
        {
            mApplySettings = true;
            
            
        }

        void IPrivateAxisSystem.SetParent(IPrivateDataSeriesChart parent)
        {
            ChartIntegrity.Assert(parent != null);
            if(mVisualObjects != null)
                mVisualObjects.SetParent(parent);
            mParent = parent;
          //
        }
        
        protected double GetScrollOffset(int axis,double sMax)
        {
            //if (View.Scrollable == false)
            //    return 0f;
            if ((View.AutoScrollHorizontally && axis == 0) || (View.AutoScrollVertically && axis == 1))
            {
                //float sMin = (float)((IInternalGraphData)Data).GetMinValue(axis,false);
                double dMax = (axis==0) ? mDataBounds.MaxX.Value : mDataBounds.MaxY.Value;
                //float dMin = (float)((IInternalGraphData)Data).GetMinValue(axis, true);
                double scrolling = dMax - sMax;
                if (axis == 1)
                    View.VerticalScrolling = scrolling;
                else if (axis == 0)
                    View.HorizontalScrolling = scrolling;
                return scrolling;
            }
            if (axis == 1)
                return View.VerticalScrolling;
            else if (axis == 0)
                return View.HorizontalScrolling;
            return 0.0;
        }

        ViewPortion GetScrollParams()
        {
            double radius = 0.0;
            if (mDataBounds.MaxRadius.HasValue)
                radius = mDataBounds.MaxRadius.Value;
            double minX = ChartCommon.Select(view.HorizontalViewOrigin, mDataBounds.MinX, view.AutomaticHorizontalView, 0.0);
            double minY = ChartCommon.Select(view.VerticalViewOrigin, mDataBounds.MinY, view.AutomaticVerticallView, 0.0);
            double maxX = ChartCommon.Select(view.HorizontalViewOrigin + view.HorizontalViewSize, mDataBounds.MaxX, view.AutomaticHorizontalView, 10.0);
            double maxY = ChartCommon.Select(view.VerticalViewOrigin + view.VerticalViewSize, mDataBounds.MaxY, view.AutomaticVerticallView, 10.0);
            double xScroll = (view.AutomaticHorizontalView ? 0.0 : GetScrollOffset(0, maxX)) + minX;
            double yScroll = (view.AutomaticVerticallView ? 0.0 : GetScrollOffset(1, maxY)) + minY;
            double xSize = maxX - minX;
            double ySize = maxY - minY;
            view.RestoreValues(0, minX, xSize);
            view.RestoreValues(1, minY, ySize);
            ViewPortion portion = new ViewPortion();
            portion.From = new DoubleVector3(xScroll, yScroll, 0.0);
            portion.To = new DoubleVector3(xScroll + xSize, yScroll + ySize, 0.0);
            portion.ViewDiagonalBase = View.ThicknessBaseDiagonal;
            portion.OppositeX = View.HorizontalDirection == AxisDirection.Opposite;
            portion.OppositeY = View.VerticalDirection == AxisDirection.Opposite;
            //portion.InflateWithEpsilon();
            return portion;
        }

        public void OnValidate()
        {
      //      ValidateAxis();
        }

        void IPrivateAxisSystem.ValidateAxis()
        {
            ValidateAxis();
        }

    }
}
