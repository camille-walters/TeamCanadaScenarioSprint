using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections.Generic;

namespace DataVisualizer
{
    [ExecuteInEditMode]
    [Serializable]
    [RequireComponent(typeof(RectTransform))]
    public partial class CanvasDataSeriesChart : DataSeriesChart
    {
        static List<Canvas> mCanvasResults = new List<Canvas>();
        public GameObject LoadingOverlay;
        ViewPortion mPrev;
        RectMask2D mMask;
        private Vector2? mLastPosition;
        private GraphicRaycaster mCaster;
        private static Type[] CanvasTypes = new Type[] { typeof(RectTransform) ,typeof(ChartItem),typeof(Canvas),typeof(CanvasRenderer),typeof(DataSeriesGraphic)};
//        private static Type[] MeshRendererTypes = new Type[] { typeof(RectTransform), typeof(ChartItem), typeof(MeshRenderer), typeof(WorldSpaceDataSeriesGraphic) };
        private GameObject mLoadingOverlayInstance;
        private GameObject mInteractionManagerInstance;
        UnityEngine.Object CanvasTemplate;
        bool mVisible = true;

        protected override void Start()
        {
            EnsureTangents();
            base.Start();
        }

        protected void OnTransformParentChanged()
        {
            EnsureTangents();
        }

        void EnsureTangents()
        {
            GetComponentsInParent<Canvas>(true, mCanvasResults);
            foreach(Canvas c in mCanvasResults)
                c.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
        }
        public static void SetRectTransformToFill(GameObject obj)
        {
            var rect = obj.GetComponent<RectTransform>();
            //if (rect == null)
            //    return;
            rect.anchorMin = new Vector2();
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = new Vector2();
            rect.offsetMax = new Vector2();
            ChartIntegrity.Assert(float.IsNaN(rect.anchoredPosition.x) == false);
            ChartIntegrity.Assert(float.IsNaN(rect.anchoredPosition.y) == false);
        }

        void SetRectTransformToContain(GameObject obj)
        {
            var rect = obj.GetComponent<RectTransform>();
            // if (rect == null)
            //        return;
            rect.anchorMin = new Vector2();
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = new Vector2(-5f,-5f);
            rect.offsetMax = new Vector2(-5f,-5f);
        }

        protected override GameObject CreateChildObject(Transform parent)
        {
            var obj = ChartCommon.CreateCanvasChartItem();
            if (ChartCommon.IsInEditMode)
            {
                obj.tag = "EditorOnly";
                obj.hideFlags = HideFlags.DontSaveInEditor;
            }
            obj.transform.SetParent(parent);
            SetRectTransformToFill(obj);
            ChartCommon.ZeroLocalTransform(obj.transform);
            
            return obj;
        }

        protected override GameObject FindChildObject(Transform parent,string name)
        {
            if (parent == null)
                parent = transform;
            var t = parent.Find(name);
            if (t == null)
                return null;
            return t.gameObject;
        }

        protected override GameObject CreateEmptyObject(Transform parent)
        {
            GameObject obj = ChartCommon.CreateCanvasChartItem();
            obj.transform.SetParent(parent);
            ChartCommon.ZeroLocalTransform(obj.transform);
            SetRectTransformToFill(obj);
            return obj;
        }

        protected override void MakeClippableObject(GameObject obj, bool clippable)
        {
            if (clippable)
            {
                ChartCommon.EnsureComponent<RectMask2D>(obj);
            }
            else
            {
                var mask = obj.GetComponent<RectMask2D>();
                if (mask != null)
                    ChartCommon.SafeDestroy(mask);
            }
        }

        protected override IChartSeriesGraphic CreateEdgeGraphic(Transform parent)
        {
            GameObject obj = new GameObject("EdgeGraphic", CanvasTypes);
            Canvas canvas = obj.GetComponent<Canvas>();
            CanvasRenderer renderer = obj.GetComponent<CanvasRenderer>();
            renderer.cullTransparentMesh = false;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.Tangent;
            obj.transform.SetParent(parent);
            SetRectTransformToFill(obj);
            ChartCommon.ZeroLocalTransform(obj.transform);
            return obj.GetComponent<DataSeriesGraphic>();
        }
        //protected override IChartSeriesGraphic CreateEdgeGraphic(Transform parent)
        //{
        //    GameObject obj = new GameObject("EdgeGraphic", MeshRendererTypes);
        //    obj.transform.SetParent(parent);
        //    SetRectTransformToFill(obj);
        //    ChartCommon.ZeroLocalTransform(obj.transform);
        //    return obj.GetComponent<WorldSpaceDataSeriesGraphic>();
        //}

        public DoubleVector2 CanvasSpaceToChartSpace(Vector2 point)
        {

            Vector3 transformed = transform.InverseTransformPoint(point);
            return RectTransformSpaceToChartSpace(transformed);

        }

        public DoubleVector2 RectTransformSpaceToChartSpace(Vector2 point)
        {
            double x = ((point.x - LocalFitPortion.From.x) / LocalFitPortion.Width);
            double y = ((point.y - LocalFitPortion.From.y) / LocalFitPortion.Height);
            x += 0.5f;
            y += 0.5f;
            x = (x * Axis.ChartSpaceView.Width) + Axis.ChartSpaceView.From.x;
            y = (y * Axis.ChartSpaceView.Height) + Axis.ChartSpaceView.From.y;
            return new DoubleVector2(x, y);
        }



        protected override void Awake()
        {
            EnsureInteractionManager();
            EnsureLoadingOverlay();
            base.Awake();
        }

        void EnsureInteractionManager()
        {
            if(mInteractionManagerInstance == null)
            {
                var t =  transform.Find("Interactions");
                if (t != null)
                {
                    mInteractionManagerInstance = t.gameObject;
                }
                else
                {
                    mInteractionManagerInstance = new GameObject();
                    mInteractionManagerInstance.transform.SetParent(transform,false);
                    mInteractionManagerInstance.transform.localScale = new Vector3(1f, 1f, 1f);
                    mInteractionManagerInstance.transform.localPosition = new Vector3(0f, 0f, 0f);
                    mInteractionManagerInstance.name = "Interactions";
                }
                var rend = ChartCommon.EnsureComponent<CanvasRenderer>(mInteractionManagerInstance);
                rend.hideFlags = HideFlags.HideInInspector;
                var mang = ChartCommon.EnsureComponent<CanvasInteractionManager>(mInteractionManagerInstance);
                mang.hideFlags = HideFlags.HideInInspector;
                CanvasDataSeriesChart.SetRectTransformToFill(mInteractionManagerInstance);
                var rect = mInteractionManagerInstance.GetComponent<RectTransform>();
                rect.pivot = new Vector2(0, 0);
            }
        }

        void EnsureLoadingOverlay()
        {
            if(mLoadingOverlayInstance == null)
            {
                if (LoadingOverlay == null)
                    LoadingOverlay = (GameObject)Resources.Load("LoadingOverlay");
                mLoadingOverlayInstance = (GameObject)GameObject.Instantiate(LoadingOverlay, transform);
                mLoadingOverlayInstance.name = "LoadingOverlay";
                mLoadingOverlayInstance.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy | HideFlags.HideInInspector;
#if DONTHIDEINNEROBJECTS
                mLoadingOverlayInstance.hideFlags = HideFlags.DontSave;
#endif
                mLoadingOverlayInstance.tag = "EditorOnly";
                mLoadingOverlayInstance.SetActive(false);
            }
            if(mMask == null)
            {
                mMask = ChartCommon.EnsureComponent<RectMask2D>(gameObject);
                mMask.enabled = false;
            }
        }

        protected override void CategoryAdded(IDataSeries series)
        {
            base.CategoryAdded(series);
            series.SetVisible(mVisible);
        }

        protected override void StartLoading()
        {
            // if (mMask != null && mLoadingOverlayInstance != null)
            // {
                mMask.enabled = true;
                mLoadingOverlayInstance.SetActive(true);
                mVisible = false;
                foreach (IDataSeries series in DataSeriesObjects)
                    series.SetVisible(false);
            //}
        }

        protected override void DoneLoading()
        {
           // if (mMask != null && mLoadingOverlayInstance != null)
            //{
                mMask.enabled = false;
                mLoadingOverlayInstance.SetActive(false);
                mVisible = true;
                foreach (IDataSeries series in DataSeriesObjects)
                    series.SetVisible(true);
            //}
        }

        public override void Update()
        {
            var rectT = GetComponent<RectTransform>();
            ViewPortion portion = new ViewPortion(0, 0, rectT.rect.width, rectT.rect.height, 
                Axis.View.ThicknessBaseDiagonal,
                Axis.View.HorizontalDirection == AxisDirection.Opposite,Axis.View.VerticalDirection == AxisDirection.Opposite);
            if(portion.CompareWithError(mPrev,0.000001) == false)
            {
                
                mPrev = portion;
                LocalFitPortion = portion;
            }
            HandleDrag();
            HandleZoom();
            base.Update();  
        }
    }
}