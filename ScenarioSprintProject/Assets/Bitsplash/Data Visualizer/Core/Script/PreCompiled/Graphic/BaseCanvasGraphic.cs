
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace DataVisualizer{
    /// <summary>
    /// base class for all graphic objects . This class encapsulates all graphic vertex assignemnt regardless of unity version
    /// </summary>
    public abstract class BaseCanvasGraphic : MaskableGraphic, IVertexReciver, IVertexArray, IChartGraphic
    {

        protected GraphicVertexArray VertexArray { get; private set; }
        protected GraphicMaterialManager MaterialManager { get; private set; }

        //   WorldSpaceChartMesh mWorldSpaceMesh = null;

        bool mInvalidated = false;
        bool mTringleStrip = false;
        bool mSharedMaterial = false;
        bool mAssignMesh = false;
        protected MappingFunction mUvMappingFunction = MappingFunction.One;

        protected int WriteBase;

        Mesh mMesh = null;

        const int MaxListSize = ChartSettings.GraphicListSize;

        [SerializeField]
        [HideInInspector]
        bool mHasColor = false;
        [SerializeField]
        [HideInInspector]
        bool mHasTangent = false;

        public BaseCanvasGraphic()
        {
            
        }

        protected override void Awake()
        {
            base.Awake();
            if (ChartCommon.IsInEditMode)
                OnCreated(MaxListSize);
        }

        public void OnAnimatorIK(int layerIndex)
        {

        }

#if ChartDevLogEnabled
        public int LogVertexCount;
#endif

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (ChartCommon.IsInEditMode)
                OnCreated(MaxListSize);
        }
#endif
        public void OnCreated(int targetVertexCount)
        {
            ChartIntegrity.NotifyMethodCall((Action<int>)OnCreated);
            MaterialManager = new GraphicMaterialManager();
            if (targetVertexCount < 0)
                targetVertexCount = MaxListSize;
            VertexArray = new GraphicVertexArray(targetVertexCount);
            VertexArray.HasTangent = HasTangent;
            VertexArray.HasColor = HasColor;
        }

        public float ExtrusionAmount
        {
            get { return MaterialManager.ExtrusionAmount; }
            set
            {
                MaterialManager.ExtrusionAmount = value;

            }
        }

        public bool TringleStrip
        {
            get { return mTringleStrip; }
            set
            {
                mTringleStrip = value;

            }
        }

        public bool HasColor
        {
            get
            {
                return mHasColor;
            }
            set
            {
                mHasColor = value;
                VertexArray.HasColor = value;
            }
        }


        public bool HasTangent
        {
            get
            {
                return mHasTangent;
            }
            set
            {
                mHasTangent = value;
                VertexArray.HasTangent = value;
            }
        }



        MappingFunction mTmpFunction;
        IEnumerator SetUvMappingCoroutine()
        {
            yield return new WaitForEndOfFrame();
            mUvMappingFunction = mTmpFunction;
        }

        public MappingFunction UvMapping
        {
            get { return MaterialManager.UvMapping; }
            set
            {
                SetUvMapping(MaterialManager.UvMapping);
            }
        }
        private void SetUvMapping(MappingFunction function)
        {

            if (HasUvMapping())
            {
                mTmpFunction = function;
                mUvMappingFunction = mTmpFunction;
                MaterialManager.UvMapping = mTmpFunction;
            }
            else
            {
                mUvMappingFunction = function;
                MaterialManager.UvMapping = function;
                Invalidate(true);
            }
        }

        public virtual bool HasFeature(int id)
        {
            return MaterialManager.HasFeature(id);
        }

        public virtual bool HasFeature(string featureName)
        {
            return MaterialManager.HasFeature(featureName);
        }

        public void Invalidate()
        {
            Invalidate(false); 
        }

        private void Invalidate(bool uvMappingOnly)
        {
            ChartIntegrity.AssertMethodCalled((Action<int>)OnCreated);
            mInvalidated = true;
            SetVerticesDirty();
        }
        public override void SetVerticesDirty()
        {
            base.SetVerticesDirty();
        }

        IEnumerator InvalidateCoroutine()
        {
            yield return 0;// new WaitForEndOfFrame();

            if (mInvalidated)
            {
                SetVerticesDirty();
                Rebuild(UnityEngine.UI.CanvasUpdate.PreRender);
            }
        }
        public Matrix4x4 DirectionTranform
        {
            get { return MaterialManager.DirectionTranform; }
            set
            {
                MaterialManager.DirectionTranform = value;
            }
        }
        protected override void OnDisable()
        {
            base.OnDisable();
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (mMesh != null)
            {
                ChartCommon.SafeDestroy(mMesh);
                mMesh = null;
            }
            if (MaterialManager != null)
                MaterialManager.OnDestory();
        }


        protected override void UpdateMaterial()
        {
            base.UpdateMaterial();
            if (material != null)
                canvasRenderer.SetTexture(material.mainTexture);
            else
                canvasRenderer.SetTexture(null);
        }

        bool HasUvMapping()
        {
#if DEBUG
            //   return false;
#endif
            if (MaterialManager != null)
                return MaterialManager.HasUvMapping();
            return false;
        }

        protected virtual void Update()
        {
            ChartIntegrity.AssertMethodCalled((Action<int>)OnCreated);
            if (MaterialManager != null)
                MaterialManager.Update();
           // SetMaterialDirty();
        }

        protected virtual void LateUpdate()
        {
            ChartIntegrity.AssertMethodCalled((Action<int>)OnCreated);
            if (MaterialManager != null)
                MaterialManager.LateUpdate();
            if(m_MaskMaterial!= null)
                SetMaterialDirty();
        }

        /// <summary>
        /// overriding the default implementation to support materials with _ChartTiling property. This is used to tile texture along the graphic lines
        /// </summary>
        public override Material material
        {
            get
            {
                return (m_Material != null) ? m_Material : defaultMaterial;
            }
            set
            {
                MonoBehaviour toSet = GetComponentInParent<MultipleChartGraphic>();
                if (toSet == null)
                    toSet = this;
                MaterialManager.SetMaterial(toSet, value, true, false);
                base.material = MaterialManager.mCachedMaterial;
            }
        }

        public bool MaintainAngles
        {
            get { return MaterialManager.MaintainAngles; }
            set
            {
                MaterialManager.MaintainAngles = value;
            }
        }


        public override Material materialForRendering
        {
            get
            {
                var mat = base.materialForRendering;
                if(mat!=null && mat != MaterialManager.mCachedMaterial)
                    MaterialManager.CopyTo(mat);
                return mat;
            }
        }

        protected abstract void AssignVertices();

#pragma warning disable 0672


        public void BuildGeometry()
        {
            UpdateGeometry();
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            mAssignMesh = true;


        }
        protected override void OnRectTransformDimensionsChange()
        {
           // base.OnRectTransformDimensionsChange();
        }
        protected override void UpdateGeometry()
        {
            //Debug.Log("update geom " + GetInstanceID());
            ChartIntegrity.AssertMethodCalled((Action<int>)OnCreated);
           // base.UpdateGeometry();
           // return;

            if (mInvalidated == false && mMesh != null)
            {
                if(mAssignMesh)
                    AssignMesh();
                //Debug.Log("discard");
                return;
            }

            // Debug.Log("gemo");
            mInvalidated = false;   // mark as invalidated

            bool hasMesh = mMesh != null;
            bool hasUvMapping = HasUvMapping();
            bool hasExtrusion = MaterialManager.CheckExtrusion();
            
            OnBeforeDataUpload();

            //   ChartCommon.DevLog("updateGeometry", "enter");
            if (hasMesh && VertexArray.ArrayDiscarded)   // no array data and mesh is created
            {
                ChartIntegrity.AssertMethodCalled((Action)AssignMesh);
                if (hasExtrusion == false)
                    VertexArray.ExtrudePositions(this, mMesh);
                if (hasUvMapping == false) // if there no uv mapping then map all uvs
                    VertexArray.MapUvs(this, mMesh);
                return; // no need to do anything , the data is cached in the mesh object
            }

            // ChartCommon.DevLog("updateGeometry", "assign " + mArrayDiscarded);
            // if the mesh is not created , or there are some optimizations that are supported on the material .Do the minimum required
            if (VertexArray.ArrayDiscarded == false)
                AssignVertices();
            if (mMesh == null)
            {
                mMesh = new Mesh();
                mMesh.MarkDynamic();
            }
#if ChartDevLogEnabled
            LogVertexCount = VertexArray.Count;
#endif
            VertexArray.AssignToMesh(this, mMesh, !hasExtrusion, !hasUvMapping);
            
            AssignMesh();
            OnDataUploaded();
        }

        protected virtual void OnBeforeDataUpload()
        {

        }

        protected virtual void OnDataUploaded()
        {

        }


        void AssignMesh()
        {
            ChartIntegrity.NotifyMethodCall((Action)AssignMesh);
            mAssignMesh = false;
            var renderer = GetComponent<CanvasRenderer>();
            renderer.SetMesh(mMesh);
        }

        public void SetMaterial(Material mat, bool isShared)
        {
            mSharedMaterial = isShared;
            material = mat;
            SetMaterialDirty();
        }


        void MapVertexUv(ref UIVertex v)
        {
            v.uv0 = mUvMappingFunction.MapUv(v.uv0);
        }

        protected int ArrayCount
        {
            get { return VertexArray.Count; }
        }

        public int VertexCount
        {
            get
            {
                return VertexArray.Count;
            }
        }

        int IVertexArray.WriteBase { get { return WriteBase; } set { WriteBase = value; } }

        public int VertexCapacity { get { return MaxListSize; } }

        public MeshArrays GetArrays()
        {
            return VertexArray.GetArrays();
        }

        public void DiscardArray()
        {
            VertexArray.DiscardArray();
        }

        void IVertexArray.AddVertices(int count)
        {
            VertexArray.Add(count);
        }

        void IVertexArray.RemoveVertices(int count)
        {
            VertexArray.Remove(count);
        }

        void IVertexArray.CopyFromTo(int from, int to, int count)
        {
            VertexArray.CopyFromTo(from, to, count);
        }

        void IVertexArray.Clear()
        {
            VertexArray.Clear();
        }

        public void EnsureArray()
        {
            VertexArray.EnsureArray();
        }

        public void SetVisible(bool enabled)
        {
            if (gameObject.activeSelf != enabled)
            {
                gameObject.SetActive(enabled);
                Invalidate();
            }
        }

        public virtual bool IsObjectActive { get { return gameObject.activeInHierarchy; } }

        public IChartGraphic ParentGraphic { get; set; }
    }
}
