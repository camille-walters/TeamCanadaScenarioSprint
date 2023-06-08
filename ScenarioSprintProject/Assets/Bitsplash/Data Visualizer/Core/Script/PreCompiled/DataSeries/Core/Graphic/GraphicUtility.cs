//using DataVisualizer;
//using DataVisualizer.Common;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;

//namespace Assets.Data_Visualizer.Script.DataSeries.Core.Graphic
//{
//    class GraphicUtility
//    {

//        protected GraphicVertexArray VertexArray { get; private set; }
//        protected GraphicMaterialManager MaterialManager { get; private set; }

//        //   WorldSpaceChartMesh mWorldSpaceMesh = null;

//        bool mInvalidated = false;
//        bool mTringleStrip = false;
//        bool mSharedMaterial = false;
//        protected MappingFunction mUvMappingFunction = MappingFunction.One;

//        protected int WriteBase;

//        Mesh mMesh = null;
//        bool mVisible = false;

//        const int MaxListSize = 60000;

//        bool mHasColor = false;
//        bool mHasTangent = false;
//        IChartGraphic mParent;

//        public GraphicUtility(IChartGraphic parent)
//        {
//            mParent = parent;
//        }

//        public void OnCreated(int targetVertexCount)
//        {
//            ChartIntegrity.NotifyMethodCall((Action<int>)OnCreated);
//            MaterialManager = new GraphicMaterialManager();
//            if (targetVertexCount < 0)
//                targetVertexCount = MaxListSize;
//            VertexArray = new GraphicVertexArray(targetVertexCount);
//        }


//        public float ExtrusionAmount
//        {
//            get { return MaterialManager.ExtrusionAmount; }
//            set
//            {
//                MaterialManager.ExtrusionAmount = value;

//            }
//        }

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



//        MappingFunction mTmpFunction;

//        public MappingFunction UvMapping
//        {
//            get { return MaterialManager.UvMapping; }
//            set
//            {
//                SetUvMapping(MaterialManager.UvMapping);
//            }
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
//                mParent.Invalidate(true);
//            }
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
//            if (MaterialManager != null)
//                MaterialManager.LateUpdate();
//        }

//        public Matrix4x4 DirectionTranform
//        {
//            get { return MaterialManager.DirectionTranform; }
//            set
//            {
//                MaterialManager.DirectionTranform = value;
//            }
//        }
//        public void Invalidate()
//        {
//            mInvalidated = true;
//        }
//        public void UpdateMesh()
//        {
//            ChartIntegrity.AssertMethodCalled((Action<int>)OnCreated);
//            if (mInvalidated == false && mMesh != null)
//            {
////                AssignMesh();
//                //Debug.Log("discard");
//                return mMesh;
//            }
//            mInvalidated = false;   // mark as invalidated

//            bool hasMesh = mMesh != null;
//            bool hasUvMapping = HasUvMapping();
//            bool hasExtrusion = MaterialManager.CheckExtrusion();
//            mParent.OnBeforeDataUpload();

//            //   ChartCommon.DevLog("updateGeometry", "enter");
//            if (hasMesh && VertexArray.ArrayDiscarded)   // no array data and mesh is created
//            {
//                if (hasExtrusion == false)
//                    VertexArray.ExtrudePositions(this, mMesh);
//                if (hasUvMapping == false) // if there no uv mapping then map all uvs
//                    VertexArray.MapUvs(this, mMesh);
//                return ; // no need to do anything , the data is cached in the mesh object
//            }

//            // ChartCommon.DevLog("updateGeometry", "assign " + mArrayDiscarded);
//            // if the mesh is not created , or there are some optimizations that are supported on the material .Do the minimum required
//            if (VertexArray.ArrayDiscarded == false)
//                AssignVertices();
//            if (mMesh == null)
//            {
//                mMesh = new Mesh();
//                // Debug.Log("mesh");
//            }

//            VertexArray.AssignToMesh(this, mMesh, !hasExtrusion, !hasUvMapping);
//            mParent.OnDataUploaded();
//            return mMesh;
//        }
//    }
//}
