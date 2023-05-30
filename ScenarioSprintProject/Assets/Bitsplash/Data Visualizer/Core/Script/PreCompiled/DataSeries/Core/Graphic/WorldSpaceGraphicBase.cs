//using DataVisualizer;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using ThetaList;
//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;

//namespace DataVisualizer{
//    [RequireComponent(typeof(MeshFilter))]
//    [RequireComponent(typeof(MeshRenderer))]
//    public abstract class WorldSpaceGraphicBase : UIBehaviour, IChartGraphic, IVertexArray,IClippable
//    {
//        protected GraphicVertexArray VertexArray { get; private set; }
//        protected GraphicMaterialManager MaterialManager { get; private set; }
//        bool mInvalidated = false;
//        bool mTringleStrip = false;
//        bool mSharedMaterial = false;
//        protected MappingFunction mUvMappingFunction = MappingFunction.One;

//        protected int WriteBase;

//        Mesh mMesh = null;
//        bool mVisible = false;

//        bool mHasColor = false;
//        bool mHasTangent = false;
//        MappingFunction mTmpFunction;
//        [NonSerialized]
//        private RectMask2D m_ParentMask;

//        public bool TringleStrip
//        {
//            get { return mTringleStrip; }
//            set
//            {
//                mTringleStrip = value;

//            }
//        }

//        public bool HasColor
//        {
//            get
//            {
//                return mHasColor;
//            }
//            set
//            {
//                mHasColor = value;
//                VertexArray.HasColor = value;
//            }
//        }
//        public bool HasTangent
//        {
//            get
//            {
//                return mHasTangent;
//            }
//            set
//            {
//                mHasTangent = value;
//                VertexArray.HasTangent = value;
//            }
//        }

//        public float ExtrusionAmount
//        {
//            get { return MaterialManager.ExtrusionAmount; }
//            set
//            {
//                MaterialManager.ExtrusionAmount = value;

//            }
//        }
//        public MappingFunction UvMapping
//        {
//            get { return MaterialManager.UvMapping; }
//            set
//            {
//                SetUvMapping(MaterialManager.UvMapping);
//            }
//        }
//        private void SetUvMapping(MappingFunction function)
//        {

//            if (HasUvMapping())
//            {
//                mTmpFunction = function;
//                // if(gameObject.activeInHierarchy && IsActive())
//                //    StartCoroutine(SetUvMappingCoroutine());
//                // else
//                mUvMappingFunction = mTmpFunction;
//                MaterialManager.UvMapping = mTmpFunction;
//            }
//            else
//            {
//                mUvMappingFunction = function;
//                MaterialManager.UvMapping = function;
//                Invalidate(true);
//            }
//        }
//        public virtual bool HasFeature(int id)
//        {
//            return MaterialManager.HasFeature(id);
//        }

//        public virtual bool HasFeature(string featureName)
//        {
//            return MaterialManager.HasFeature(featureName);
//        }

//        public void Invalidate()
//        {
//            Invalidate(false);
//        }

//        private void Invalidate(bool uvMappingOnly)
//        {
//            ChartIntegrity.AssertMethodCalled((Action<int>)OnCreated);
//            mInvalidated = true;
//       }



//        public Matrix4x4 DirectionTranform
//        {
//            get { return MaterialManager.DirectionTranform; }
//            set
//            {
//                MaterialManager.DirectionTranform = value;
//            }
//        }

//        protected virtual void OnDestroy()
//        {
//            if (MaterialManager != null)
//                MaterialManager.OnDestory();
//        }

//        bool HasUvMapping()
//        {
//#if DEBUG
//            //   return false;
//#endif
//            if (MaterialManager != null)
//                return MaterialManager.HasUvMapping();
//            return false;
//        }

//        protected virtual void Update()
//        {
//            ChartIntegrity.AssertMethodCalled((Action<int>)OnCreated);
//            if (MaterialManager != null)
//                MaterialManager.Update();
//        }

//        protected virtual void LateUpdate()
//        {
//            ChartIntegrity.AssertMethodCalled((Action<int>)OnCreated);
//            if (mInvalidated == true)
//            {
//                BuildGeometry();
//                mInvalidated = false;
//            }
//            if (MaterialManager != null)
//                MaterialManager.LateUpdate();

//        }


//        public bool MaintainAngles
//        {
//            get { return MaterialManager.MaintainAngles; }
//            set
//            {
//                MaterialManager.MaintainAngles = value;
//            }
//        }


//        protected abstract void AssignVertices();

//#pragma warning disable 0672

//        public void BuildGeometry()
//        {
//            ChartIntegrity.AssertMethodCalled((Action<int>)OnCreated);

//            if (mInvalidated == false && mMesh != null)
//            {
//                AssignMesh();
//                //Debug.Log("discard");
//                return;
//            }

//            // Debug.Log("gemo");
//            mInvalidated = false;   // mark as invalidated

//            bool hasMesh = mMesh != null;
//            bool hasUvMapping = HasUvMapping();
//            bool hasExtrusion = MaterialManager.CheckExtrusion();

//            OnBeforeDataUpload();

//            //   ChartCommon.DevLog("updateGeometry", "enter");
//            if (hasMesh && VertexArray.ArrayDiscarded)   // no array data and mesh is created
//            {
//                ChartIntegrity.AssertMethodCalled((Action)AssignMesh);
//                if (hasExtrusion == false)
//                    VertexArray.ExtrudePositions(this, mMesh);
//                if (hasUvMapping == false) // if there no uv mapping then map all uvs
//                    VertexArray.MapUvs(this, mMesh);
//                return; // no need to do anything , the data is cached in the mesh object
//            }

//            // ChartCommon.DevLog("updateGeometry", "assign " + mArrayDiscarded);
//            // if the mesh is not created , or there are some optimizations that are supported on the material .Do the minimum required
//            if (VertexArray.ArrayDiscarded == false)
//                AssignVertices();
//            if (mMesh == null)
//            {
//                mMesh = new Mesh();
//                mMesh.MarkDynamic();
//                //mMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
//            }
//            VertexArray.AssignToMesh(this, mMesh, !hasExtrusion, !hasUvMapping);
//            AssignMesh();
//            OnDataUploaded();
//        }

//        protected virtual void OnBeforeDataUpload()
//        {

//        }

//        protected virtual void OnDataUploaded()
//        {

//        }

//        Mesh mAssignedMesh = null;
//        Material mAssignedMaterial = null;

//        void AssignMesh()
//        {
//            ChartIntegrity.NotifyMethodCall((Action)AssignMesh);
//            var filter = GetComponent<MeshFilter>();
//            var renderer = GetComponent<MeshRenderer>();

//            if (mVisible == false)
//            {
//                if (mAssignedMaterial != null)
//                     renderer.sharedMaterial = null;
//                mAssignedMaterial = null;
//            }
//            else
//            {
//                var cachedMaterial = MaterialManager.mCachedMaterial;
//                if (mAssignedMaterial != cachedMaterial)
//                {
//                    renderer.sharedMaterial = cachedMaterial;
//                }
//                mAssignedMaterial = cachedMaterial;
//            }
//            if (mAssignedMesh != mMesh)
//            {
//                filter.mesh = mMesh;
//                mAssignedMesh = mMesh;
//            }            
//        }

//        public void SetMaterial(Material mat, bool isShared)
//        {
//            mSharedMaterial = isShared;
//            MaterialManager.SetMaterial(this, mat, mSharedMaterial);
//            AssignMesh();
//        }


//        void MapVertexUv(ref UIVertex v)
//        {
//            v.uv0 = mUvMappingFunction.MapUv(v.uv0);
//        }

//        protected int ArrayCount
//        {
//            get { return VertexArray.Count; }
//        }

//        public int VertexCount
//        {
//            get
//            {
//                return VertexArray.Count;
//            }
//        }

//        int IVertexArray.WriteBase { get { return WriteBase; } set { WriteBase = value; } }

//        public int VertexCapacity { get { return ChartSettings.GraphicListSize; } }

//        public MeshArrays GetArrays()
//        {
//            return VertexArray.GetArrays();
//        }

//        public void DiscardArray()
//        {
//            VertexArray.DiscardArray();
//        }

//        void IVertexArray.AddVertices(int count)
//        {
//            VertexArray.Add(count);
//        }

//        void IVertexArray.RemoveVertices(int count)
//        {
//            VertexArray.Remove(count);
//        }

//        void IVertexArray.CopyFromTo(int from, int to, int count)
//        {
//            VertexArray.CopyFromTo(from, to, count);
//        }

//        void IVertexArray.Clear()
//        {
//            VertexArray.Clear();
//        }

//        public void EnsureArray()
//        {
//            VertexArray.EnsureArray();
//        }

//        public void SetVisible(bool enabled)
//        {
//            mVisible = enabled;
//            Invalidate();

//        }

//        public virtual bool IsObjectActive { get { return gameObject.activeInHierarchy; } }

//        public IChartGraphic ParentGraphic
//        {
//            get;
//            set;
//        }



//        protected override void Awake()
//        {
//            base.Awake();
//            if (ChartCommon.IsInEditMode)
//                OnCreated(ChartSettings.GraphicListSize);
//        }
//        protected override void OnValidate()
//        {
//            base.OnValidate();
//            if (ChartCommon.IsInEditMode)
//                OnCreated(ChartSettings.GraphicListSize);
//            UpdateClipParent();
//        }
//        protected override void OnEnable()
//        {
//            base.OnEnable();
//            UpdateClipParent();
//        }
//        protected override void OnDisable()
//        {
//            base.OnDisable();
//            UpdateClipParent();
//        }
//        protected override void OnTransformParentChanged()
//        {
//            base.OnTransformParentChanged();
//            if (!isActiveAndEnabled)
//                return;
//            UpdateClipParent();
//        }
//        protected override void OnCanvasHierarchyChanged()
//        {
//            base.OnCanvasHierarchyChanged();
//            UpdateClipParent();
//        }
//        public void OnCreated(int targetVertexCount)
//        {
//            ChartIntegrity.NotifyMethodCall((Action<int>)OnCreated);
//            MaterialManager = new GraphicMaterialManager();
//            if (targetVertexCount < 0)
//                targetVertexCount = ChartSettings.GraphicListSize;
//            VertexArray = new GraphicVertexArray(targetVertexCount);
//        }

//        public RectTransform rectTransform { get { return gameObject.GetComponent<RectTransform>(); } }

//        void UpdateClipParent()
//        {
//            var newParent = (IsActive()) ? MaskUtilities.GetRectMaskForClippable(this) : null;

//            // if the new parent is different OR is now inactive
//            if (m_ParentMask != null && (newParent != m_ParentMask || !newParent.IsActive()))
//            {
//                m_ParentMask.RemoveClippable(this);
//            }

//            // don't re-add it if the newparent is inactive
//            if (newParent != null && newParent.IsActive())
//                newParent.AddClippable(this);

//            m_ParentMask = newParent;
//        }
//        public void RecalculateClipping()
//        {
//            UpdateClipParent();
//        }

//        public void Cull(Rect clipRect, bool validRect)
//        {

//        }

//        public void SetClipRect(Rect value, bool validRect)
//        {
//            MaterialManager.SetClipRect(validRect,value);
//        }

//        public void SetClipSoftness(Vector2 clipSoftness)
//        {
//        }
//    }
//}
