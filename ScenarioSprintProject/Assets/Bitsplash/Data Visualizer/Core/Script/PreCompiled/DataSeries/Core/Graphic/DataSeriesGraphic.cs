using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ThetaList;
using UnityEngine;

namespace DataVisualizer{
    public class DataSeriesGraphic : EventHandlingGraphic, IChartSeriesGraphic
    {
        protected DataSeriesBase mParent;

        /// <summary>
        /// bounding box for the value of mLines, used for event handling
        /// </summary>
        protected float mMinX, mMinY, mMaxX, mMaxY;
        ushort mContext = SeriesObject.EmptyContext;

        ArrayManagerType mArrayType;
        int mItemSize;
        ArrayManagerBase mArrayManager;

        public bool AddEntity(SeriesObject entity)
        {
            return mArrayManager.AddEntity(entity);
        }

        public void RemoveEntity(SeriesObject entity)
        {
            mArrayManager.RemoveEntity(entity);
        }

        public MakeDirtyResult MakeEntityDirty(SeriesObject entity,bool uvOnly)
        {
            return mArrayManager.MakeEntityDirty(entity, uvOnly);
        }

        public void Clear()
        {
            ChartCommon.DevLog("Clear", "Called");
            if(mArrayManager != null)
                mArrayManager.Clear();
        }

        public void MakeAllDirty(bool uvOnly,SimpleList<SeriesObject> takenOut)
        {
            if (mArrayManager != null)
                mArrayManager.MakeAllDirty(uvOnly, takenOut);
        }

        public void ClearAndChangeProperties(ArrayManagerType arrayType, int itemSize)
        {
            Clear();
            mArrayType = arrayType;
            mItemSize = itemSize;
            DiscardArray();
            CreateArrayManager();
        }

        public ArrayManagerType ArrayType
        {
            get { return mArrayType; }
        }

        public  int ItemSize
        {
            get { return mItemSize; }
        }

        public void SetDataLink(DataSeriesBase parent,ushort context)
        {
            mParent = parent;
            mContext = context;
            if (mArrayManager != null)
            {
                mArrayManager.Mapper = parent;
                mArrayManager.Context = mContext;
            }
        }

        protected Vector2 Max
        {
            get
            {
                return new Vector2(mMaxX, mMaxY);
            }
        }

        protected Vector2 Min
        {
            get
            {
                return new Vector2(mMinX, mMinY);
            }
        }


        public GameObject MyGameObject
        {
            get {
                    if (this == null)
                    return null;
                return gameObject;
            }
        }

        public Material Material
        {
            get { return material; }
            set
            {
                material = value;
                SetMaterialDirty();
            }
        }

        void CreateArrayManager()
        {
            switch(mArrayType)
            {
                case ArrayManagerType.Static:
                    mArrayManager = new StaticArrayManager(mItemSize, this, mParent,mContext);
                    break;
                case ArrayManagerType.Compact:
                    mArrayManager = new CompactArrayManager(false,mItemSize, this, mParent, mContext);
                    break;
                case ArrayManagerType.CompactRealtime:
                    mArrayManager = new CompactArrayManager(true,mItemSize, this, mParent, mContext);
                    break;
                case ArrayManagerType.Chunked:
                    mArrayManager = new ChunkedArrayManager(mItemSize, this, mParent, mContext);
                    break;
                case ArrayManagerType.StreamingChunked:
                    mArrayManager = new ChunkedArrayManager(mItemSize, this, mParent, mContext);
                    break;
                case ArrayManagerType.StreamingCompact:
                    mArrayManager = new CompactArrayManager(false, mItemSize, this, mParent, mContext);
                    break;
                default:
                    throw new Exception("Unknown array type");
            }
        }

        protected override void OnDataUploaded()
        {
            base.OnDataUploaded();
            if (mArrayManager != null)
                mArrayManager.OnAfterDataUpload();
        }
        
        protected override void OnBeforeDataUpload()
        {
            base.OnBeforeDataUpload();
            if (mArrayManager != null)
                mArrayManager.OnBeforeDataUpload();
        }

        public void PrintArrayType()
        {
            ChartCommon.DevLog("manager type", mArrayManager.GetType().Name);
        }
        
        protected override void AssignVertices()
        {
            if (mParent == null) 
                return;
            if (mParent.RawData == null)
                return;           
            mParent.EnsureDataIntegrity(); 
            mMinX = 0f;// float.PositiveInfinity;
            mMinY = 0f;// float.PositiveInfinity;
            mMaxX = (float)mParent.FitIntoRect.Max.x;// float.NegativeInfinity;
            mMaxY = (float)mParent.FitIntoRect.Max.y;
            mParent.PrepareVertices();
            if (mArrayManager != null)
            {
        //        Debug.Log(mArrayManager.GetType().Name);
                mArrayManager.WriteVertices();
            }
        }

        protected void Pick(Vector3 mouse, out int pickedIndex, out int pickedType, out object SelectionData)
        {
            SelectionData = null;
            pickedType = pickedIndex = -1;

            //if (mDataLink == null)
            //    return;
            //double minDist = double.PositiveInfinity;
            //CanvasGraphicObject selected = null;
            //DoubleVector3 unitSpaceMouse;
            //mParent.ReverseMapVector(new DoubleVector3(mouse), out unitSpaceMouse);
            //mParent.EnsureDataIntegrity();
            //for (int i = 0; i < mDataLink.ViewableIndexCount; i++)
            //{
            //    int viewableIndex = mDataLink.GetViewableIndex(i);
            //    int inner = mDataLink.InnerCount(viewableIndex);
            //    for (int j = 0; j < inner; j++)
            //    {
            //        var obj = mDataLink[viewableIndex, j] as CanvasGraphicObject;
            //        if (obj == null)
            //            continue;
            //        var bound = obj.BoundingBox(viewableIndex);
            //        if (bound.HasValue == false)    // no bounding box , object is empty
            //            continue;
            //        if (mParent.CurrentView.Intersect(bound.Value) == false)    // object is completly out of view , not nessary to proceed
            //            continue;
            //        double dist = obj.SqaureDist(unitSpaceMouse);                 
            //        if (dist < minDist)
            //        {
            //            minDist = dist;
            //            selected = obj;
            //        }
            //    }
            //}

            //if (selected == null)
            //    return;

            //float thresh = Sensitivity;
            //ChartCommon.DevLog("pick", "start");
            //if (!((ViewRect.HasValue && !ViewRect.Value.Contains(mouse)) || minDist > thresh * thresh))
            //{
            //    ChartCommon.DevLog("pick", "set");
            //    selected.GetSelectionData(out pickedType, out pickedIndex, out SelectionData);
            //}
        }

        protected void SetUpHoverObject(object hover, int index, int type, object selectionData)
        {
            //var graphic = hover.GetComponent<DataSeriesHoverGraphic>();
            //if (graphic == null)
            //    return;
            //var data = (selectionData as PickableGraphicObject.CanvasGraphicObjectData);
            //if (data != null)
            //{
            //    var cached = data.Object as CachedCanvasGraphicObject;
            //    if (cached != null)
            //    {
            //        if (cached.IsDestoryed)
            //            DestoryHoverObject(hover);  // if the chart object was destoryed then destory the hover object and do nothing more
            //        else
            //        {
            //            if (graphic.TagObject != cached || cached.Version > graphic.TagVersion) // if the vertices changed
            //            {
            //                graphic.TagObject = cached;
            //                graphic.TagVersion = cached.Version;
            //                cached.ApplyToDataHover(mParent, graphic);  //reapply them
            //            }
            //            else       // otherwise just update the position of the grpahic
            //            {
            //                cached.UpdateDataHover(mParent, graphic);
            //            }
            //        }
            //    }
            //}
        }

        public void RefreshHoverObjects()
        {
         //   throw new NotImplementedException();
        }
    }
}
