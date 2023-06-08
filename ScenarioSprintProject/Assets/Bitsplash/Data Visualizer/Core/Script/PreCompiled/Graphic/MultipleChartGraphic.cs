using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using ThetaList;
using UnityEngine;

namespace DataVisualizer{
    /// <summary>
    /// Notes:
    /// * Every instance of this class holds at least one graphic at all times.
    /// * This graphic is not meant to be under another MultipleChartGraphic. 
    /// </summary>
    public class MultipleChartGraphic : MonoBehaviour, IChartSeriesGraphic
    {
        GraphicConfig mConfig = new GraphicConfig();
        ArrayManagerType mArrayManagerType = ArrayManagerType.Compact;
        int mItemSize;
        SimpleList<SeriesObject> mTakenOut;
        DataSeriesBase mDataLink;
        public delegate IChartSeriesGraphic ChartGraphicInstanciator(MultipleChartGraphic parent);
        GraphicHolder mCurrent;
        Material mCachedMaterial;
        GraphicMaterialManager mMaterialManager;
        bool mVisible = true;
        int mTotalEntities = 0;
        public int id;
        class GraphicHolder
        {
            public GraphicHolder(IChartSeriesGraphic graphic, ushort id)
            {
                Graphic = graphic;
                Id = id;
                Dirty = true;
                EntityCount = 0;
            }

            public IChartSeriesGraphic Graphic { get; private set; }
            public ushort Id { get; private set; }
            public int Order { get; set; }
            public bool Dirty { get; set; }
            public int EntityCount;  // used for debugining only
        }

        ushort mCurrentComponentId = 0;

        ChartGraphicInstanciator mInstanciator;
        Dictionary<int, GraphicHolder> mComponentIds;
        /// <summary>
        /// this list is sorted by vertex amount , the last element has the lowest amount of vertices
        /// </summary>
        List<GraphicHolder> mComponentMinOrder;
        bool mInnerInit = false;

        void Awake()
        {
            InnerInit();
        }

        public void Init(ChartGraphicInstanciator instanciator)
        {
            InnerInit();
            ChartIntegrity.AssertMethodNotCalled((Action<ChartGraphicInstanciator>)Init);   // this method should only be called once per object
            ChartIntegrity.NotifyMethodCall((Action<ChartGraphicInstanciator>)Init);
            TargetGraphicVertices = ChartSettings.GraphicListSize;
            ChartCommon.DevLog(LogOptions.MultipleGraphic, "init");
            ChartCommon.ClearChildren(gameObject);
            mInstanciator = instanciator;
            EnsureOneGraphic();
        }

        void InnerInit()
        {
            if (mInnerInit == true)
                return;
            mMaterialManager = new GraphicMaterialManager();
            mMaterialManager.SetMaterial(this, null, true);
            mTakenOut = new SimpleList<SeriesObject>();
            mComponentIds = new Dictionary<int, GraphicHolder>(ChartCommon.DefaultIntComparer);
            mComponentMinOrder = new List<GraphicHolder>();
            mInnerInit = true;
        }

        public bool TringleStrip
        {
            get { return mConfig.TringleStrip; }
            set
            {
                mConfig.TringleStrip = value;
                RunOnAllGraphics(x => { x.TringleStrip = value; });
            }
        }

        public MappingFunction UvMapping
        {
            get { return mConfig.UvMapping; }
            set
            {
                mConfig.UvMapping = value;
                mMaterialManager.UvMapping = value;
                
                RunOnAllGraphics(x => { x.UvMapping = value; });
            }
        }

        public bool HasColor
        {
            get { return mConfig.HasColor; }
            set
            {
                mConfig.HasColor = value;
                RunOnAllGraphics(x => { x.HasColor = value; });
            }
        }

        public bool HasTangent
        {
            get
            {
                return mConfig.HasTangent;
            }
            set
            {
                mConfig.HasTangent = value;
                RunOnAllGraphics(x => { x.HasTangent = value; });
            }
        }
        
        public float ExtrusionAmount
        {
            get { return mConfig.ExtrusionAmount; }
            set
            {
                mConfig.ExtrusionAmount = value;
                mMaterialManager.ExtrusionAmount = value;
                RunOnAllGraphics(x => { x.ExtrusionAmount = value; });
            }
        }

        public GameObject MyGameObject
        {
            get { return gameObject; }
        }

        public bool MaintainAngles
        {
            get
            {
                return mConfig.MaintainAngles;
            }
            set
            {
                mConfig.MaintainAngles = value;
                mMaterialManager.MaintainAngles = value;
                RunOnAllGraphics(x => { x.MaintainAngles = value; });
            }
        }

        public Matrix4x4 DirectionTranform
        {
            get { return mConfig.DirectionTranform;  }
            set
            {
                mConfig.DirectionTranform = value;
                mMaterialManager.DirectionTranform = value;
                RunOnAllGraphics(x => { x.DirectionTranform= value; });
            }
        }

        bool IChartGraphic.IsObjectActive
        {
            get
            {
                return MyGameObject.activeInHierarchy;
            }
        }

        public int VertexCapacity
        {
            get { return int.MaxValue; }
        }

        public int ItemSize
        {
            get {  return mItemSize;}
        }

        /// <summary>
        /// the target amount of vertice each individual graphic should contain
        /// </summary>
        public int TargetGraphicVertices { get; set; }

        ushort CreateComopnentId()
        {
            return mCurrentComponentId++;
        }

        void RunOnAllGraphics(Action<IChartGraphic> toRun)
        {
            if (mComponentMinOrder == null)
                return;
            for (int i = 0; i < mComponentMinOrder.Count; i++)
            {
                toRun(mComponentMinOrder[i].Graphic);
            }
        }
        
        void SetGraphicConfig(IChartGraphic graphic)
        {
            ChartCommon.DevLog(LogOptions.MultipleGraphic, "set config:" , mConfig);
            graphic.TringleStrip = mConfig.TringleStrip;
            graphic.DirectionTranform = mConfig.DirectionTranform;
            graphic.ExtrusionAmount = mConfig.ExtrusionAmount;
            graphic.HasColor = mConfig.HasColor;
            graphic.HasTangent = mConfig.HasTangent;
            graphic.MaintainAngles = mConfig.MaintainAngles;
            graphic.SetMaterial(mMaterialManager.mCachedMaterial, true);
            graphic.UvMapping = mConfig.UvMapping;
        }

        GraphicHolder CreateGraphic()
        {
            var graphic = mInstanciator(this);
            graphic.ParentGraphic = this;
            graphic.OnCreated(TargetGraphicVertices);
            SetGraphicConfig(graphic);
            graphic.ClearAndChangeProperties(mArrayManagerType, mItemSize);
            ushort id = CreateComopnentId();
            ChartCommon.DevLog(LogOptions.MultipleGraphic, "graphic created", "id:", id, "vertex count:", VertexCount, "array manager:", mArrayManagerType, "item size:" +mItemSize);
            GraphicHolder holder = new GraphicHolder(graphic, id);
            holder.Dirty = true;
            mComponentIds.Add(id, holder);
            holder.Order = mComponentMinOrder.Count;
            mComponentMinOrder.Add(holder);
            //graphic.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
            //graphic.gameObject.transform.localPosition = Vector3.zero;
            //graphic.gameObject.transform.localRotation = Quaternion.identity;
            graphic.SetVisible(mVisible);
            if (mDataLink != null)
                graphic.SetDataLink(mDataLink, id);
            ChartIntegrity.Assert(mComponentIds.Count == mComponentIds.Count);
            return holder;
        }

        void SwitchComponentOrder(int i, int j)
        {
            GraphicHolder tmp = mComponentMinOrder[j];
            mComponentMinOrder[j] = mComponentMinOrder[i];
            mComponentMinOrder[i] = tmp;
            mComponentMinOrder[i].Order = i;
            mComponentMinOrder[j].Order = j;
        }

        void FixOrder(GraphicHolder holder)
        {
            int order = holder.Order;
            int count = holder.Graphic.VertexCount;
            int rightCount = int.MinValue;
            int leftCount = int.MaxValue;
            if (order + 1 < mComponentMinOrder.Count) // if there is an item on the right
                rightCount = mComponentMinOrder[order + 1].Graphic.VertexCount;
            if (order > 0)  // if there is an object on the left
                leftCount = mComponentMinOrder[order - 1].Graphic.VertexCount;
            if (count < leftCount && count > rightCount) // the index is sorted, so nothing to do here
                return;
            if (count > leftCount && count < rightCount) // this means the array was not sorted previously. This is an error
                throw new Exception("invalid object state. object was not sorted");
            if(count > leftCount)
            {
                for(int i=order; i>0; i--)
                {
                    leftCount = mComponentMinOrder[i - 1].Graphic.VertexCount;
                    if(count > leftCount)
                    {
                        SwitchComponentOrder(i, i - 1);
                    }
                }
            }
            else // must be that count < rightCount
            {
                int end = mComponentMinOrder.Count - 1;
                for (int i = order; i < end; i++)
                {
                    rightCount = mComponentMinOrder[i + 1].Graphic.VertexCount;
                    if (count < rightCount)
                    {
                        SwitchComponentOrder(i, i + 1);
                    }
                }
            }
        }

        bool TryAddToGraphic(GraphicHolder holder, SeriesObject entity)
        {
            if (holder.Graphic.VertexCount > TargetGraphicVertices)
            { 
                ChartCommon.DevLog(LogOptions.MultipleGraphic, "try add graphic", "failed", "graphic vertex count:", holder.Graphic.VertexCount, "target vertex count:", TargetGraphicVertices);
                // don't add if the graphic has exceeded the target vertex count
                return false;
            }

            if (holder.Graphic.AddEntity(entity) == false)
            {
                ChartCommon.DevLog(LogOptions.MultipleGraphic, "try add graphic", "failed on add");
                return false;
            }

            holder.EntityCount++;
            mTotalEntities++;
            FixOrder(holder);

            CheckCollectionIntegrity();

            entity.Context = holder.Id;
            holder.Dirty = true;
            ChartCommon.DevLog(LogOptions.MultipleGraphic, "added", "successs");
            return true;
        }

        public int InnerGraphicCount
        {
            get { return mComponentMinOrder.Count; }
        }

        int CalculateVertexCount()
        {
            int result = 0;
            for (int i = 0; i < mComponentMinOrder.Count; i++)
                result += mComponentMinOrder[i].Graphic.VertexCount;
            return result;
        }

        public int VertexCount
        {
            get
            {
                return CalculateVertexCount();
            }
        }

        public IChartGraphic ParentGraphic { get; set; }

        void EnsureOneGraphic()
        {
            if (InnerGraphicCount == 0)
            {
                ChartCommon.DevLog(LogOptions.MultipleGraphic, "one graphic created");
                CreateGraphic();
            }
            CheckCollectionIntegrity();
        }

        void EnsureCurrentGraphic()
        {
            if(mCurrent == null && mComponentMinOrder.Count > 0)
                mCurrent = mComponentMinOrder[mComponentMinOrder.Count - 1];
        }

        public void Update()
        {
            id = GetInstanceID();
            if (mMaterialManager != null)
                mMaterialManager.Update();
        }

        public void LateUpdate()
        {
            if (mMaterialManager != null)
                mMaterialManager.LateUpdate();
        }

        public bool AddEntity(SeriesObject entity)
        {
            //ChartIntegrity.Assert(mComponentMinOrder.Count > 0);
            if(entity.IsAttached)
                throw new Exception("series object is already attached to another graphic");
            ChartCommon.DevLog(LogOptions.MultipleGraphic, "add entity", "index:", entity.MyIndex,"graphic count:", mComponentMinOrder.Count);
            bool added = false;
            EnsureCurrentGraphic();
            if(mCurrent != null)
            {
                if (TryAddToGraphic(mCurrent, entity))
                    added = true;
                else
                {
                    mCurrent = null;
                    EnsureCurrentGraphic();
                    if (TryAddToGraphic(mCurrent, entity))
                        added = true;
                    else
                    {
                        if (mCurrent.Graphic.VertexCount <= 0)  // if the graphic has no points. There is not reason to create another one
                        {
                           
                            ChartCommon.DevLog(LogOptions.MultipleGraphic, "add entity", "object too big");
                            return false;   // imidialy fail
                        }
                    }
                }
            }

            if (added == false) // if the graphic with the least amount of vertices could not fit the entity , create a new one
            {
                var holder = CreateGraphic();
                mCurrent = holder;
                if (TryAddToGraphic(holder, entity) == false)
                {
                    ChartCommon.DevLog(LogOptions.MultipleGraphic, "add entity", "failed add to new graphic");
                    return false;
                }

            }
            else
            {
                if (mCurrent != null)
                   ChartCommon.DevLog(LogOptions.MultipleGraphic, "add entity", "added to:",mCurrent.Id);
            }
            CheckCollectionIntegrity();
            ChartIntegrity.NotifyAddCollection(this, "main", entity);
            return true;
        }

        public void Clear()
        {
            if (mComponentMinOrder == null)
            {
                //EnsureOneGraphic();
                return;
            }
            if (mTotalEntities == 0)
                return;
            mCurrent = null;
            ChartCommon.DevLog(LogOptions.MultipleGraphic, "clear");
            for (int i = 0; i < mComponentMinOrder.Count; i++)  // clear all graphic objects from their content
            {
                var item = mComponentMinOrder[i];
                if (item != null) 
                {
                    item.Graphic.Clear();
                   // if(((MonoBehaviour)item.Graphic) != null)
                   //     ChartCommon.SafeDestroy(item.Graphic.gameObject);   // detroy all game objects
                }
            }
            //mComponentMinOrder.Clear();
            //mComponentIds.Clear();  // clear all data from this instance
            mTotalEntities = 0;
            //mCurrentComponentId = 0;
            //if (this != null)
            //    ChartCommon.ClearChildren(gameObject);
            CheckCollectionIntegrity();
            //EnsureOneGraphic();
            ChartIntegrity.NotifyClearCollection(this, "main");
        }

        protected void OnDestroy()
        {
            if(mComponentMinOrder != null)
                mComponentMinOrder.Clear(); 
            if(mMaterialManager != null)
                mMaterialManager.OnDestory();
        }

        void RemoveGraphic(GraphicHolder holder)
        {
            if (mCurrent == holder)
                mCurrent = null;
            ChartCommon.DevLog(LogOptions.MultipleGraphic, "graphic removed", "id: ", holder.Id);
            mComponentIds.Remove(holder.Id);
            ChartIntegrity.Assert(mComponentMinOrder[holder.Order] == holder);
            mComponentMinOrder.RemoveAt(holder.Order);

            for (int i = 0; i < mComponentMinOrder.Count; i++)   // fix the order index of all items after the item that was removed
                mComponentMinOrder[i].Order = i;

            CheckCollectionIntegrity();

            ChartCommon.SafeDestroy(holder.Graphic.gameObject);
            EnsureOneGraphic();
        }

        public void ClearAndChangeProperties(ArrayManagerType arrayType, int itemSize)
        {
            mArrayManagerType = arrayType;
            mItemSize = itemSize;

            Clear();    // all new grpahic objects are created with the config defined in the lines above this one
            for (int i = 0; i < mComponentMinOrder.Count; i++)   // fix the order index of all items after the item that was removed
                mComponentMinOrder[i].Graphic.ClearAndChangeProperties(arrayType, itemSize);
        }

        public bool HasFeature(string featureName)
        {
            for (int i = 0; i < mComponentMinOrder.Count; i++)
                if (mComponentMinOrder[i].Graphic.HasFeature(featureName) == false)
                    return false;
            return true;
        }

        public bool HasFeature(int id)
        {
            for (int i = 0; i < mComponentMinOrder.Count; i++)
                if (mComponentMinOrder[i].Graphic.HasFeature(id) == false)
                    return false;
            return true;
        }


        public void Invalidate()
        {
            int dirtyCount = 0;
            for (int i = 0; i < mComponentMinOrder.Count; i++)
            {
                var item = mComponentMinOrder[i];
                if (item != null && item.Dirty)  // invalidate only graphic objects that were changed , to save unnessecary work
                {
                    dirtyCount++;
                    ChartCommon.DevLog(LogOptions.MultipleGraphic, "invalidate", "id:",item.Id);
                    item.Dirty = false;
                    item.Graphic.Invalidate();

                }
            }
     //       UnityEngine.Debug.Log(dirtyCount);
        }

        public void MakeAllDirty(bool uvOnly,SimpleList<SeriesObject> takenOut)
        {
            mTakenOut.Clear();
            for (int i = 0; i < mComponentMinOrder.Count; i++)
            {
                var item = mComponentMinOrder[i];
                if (item != null)
                {
                    item.Graphic.MakeAllDirty(uvOnly, mTakenOut);
                    item.EntityCount -= mTakenOut.Count;
                    item.Dirty = true;
                }
            }

            for(int i=0; i<mTakenOut.Count; i++)
            {
                var entity = mTakenOut[i];
                if (AddEntity(entity) == false)
                {
                    entity.Context = SeriesObject.EmptyContext;
                    ChartIntegrity.NotifyRemoveCollection(this, "main", entity);
                    takenOut.Add(entity);
                }
            }
            mTotalEntities -= takenOut.Count;
            mComponentMinOrder.Sort((x, y) => { return y.Graphic.VertexCount - x.Graphic.VertexCount; });
            for (int i = 0; i < mComponentMinOrder.Count; i++)   // fix the order index of all items after the item that was removed
                mComponentMinOrder[i].Order = i;
            CheckCollectionIntegrity();
        }

        public MakeDirtyResult MakeEntityDirty(SeriesObject entity, bool uvOnly)
        {            
            GraphicHolder h;
            ChartIntegrity.AssertCollectionContains(this, "main", entity);
            ChartIntegrity.Assert(entity.IsAttached);
            if (mComponentIds.TryGetValue(entity.Context, out h) == false)
                throw new Exception("invalid entity");
            var comp = mComponentIds[entity.Context];
            comp.Dirty = true;
            if (comp.Graphic.MakeEntityDirty(entity, uvOnly) == MakeDirtyResult.Removed)    // if the dirty entity did not fit it the graphic
            {
                comp.EntityCount--;
                ChartCommon.DevLog(LogOptions.MultipleGraphic, "moving dirty entity", "index:", entity.MyIndex);
                if (AddEntity(entity) == false)     // we add it to another
                {
                    ChartCommon.DevLog(LogOptions.MultipleGraphic, "failed moving dirty entity", "index:", entity.MyIndex);
                    ChartIntegrity.NotifyRemoveCollection(this, "main", entity);
                    entity.Context = SeriesObject.EmptyContext;
                    CheckCollectionIntegrity();
                    mTotalEntities--;
                    return MakeDirtyResult.Removed;
                }
            }
            else
            {           
                FixOrder(comp); // the vertex count may have changed. So we are checking to make sure the order of mComponentMinOrder is fixed
            }
            CheckCollectionIntegrity();
            return MakeDirtyResult.Succeeded;
        }

        public void RefreshHoverObjects()
        {
            for (int i = 0; i < mComponentMinOrder.Count; i++)
            {
                var item = mComponentMinOrder[i];
                if (item != null)
                    item.Graphic.RefreshHoverObjects();
            }
        }

        public void RemoveEntity(SeriesObject entity)
        {
            ChartIntegrity.AssertCollectionContains(this, "main", entity);
            if (entity.IsAttached == false)
                return;
            ChartCommon.DevLog(LogOptions.MultipleGraphic, "remove entity", "index:", entity.MyIndex);
            var holder = mComponentIds[entity.Context];
            holder.Graphic.RemoveEntity(entity);
            holder.EntityCount--;
            mTotalEntities--;
            FixOrder(holder);
            entity.Context = SeriesObject.EmptyContext;
            //if (InnerGraphicCount > 1 && holder.Graphic.VertexCount <= 0) // no vertice left and there is more then one graphic objects
            //{
            //    ChartCommon.DevLog(LogOptions.MultipleGraphic, "remove entity", "remove graphic", "id:", holder.Id, "entity count:", holder.EntityCount);
            //    RemoveGraphic(holder);
            //}
            CheckCollectionIntegrity();
            ChartIntegrity.NotifyRemoveCollection(this, "main", entity);
        }

        public void SetDataLink(DataSeriesBase parent,ushort context)
        {
            mDataLink = parent;
            for (int i = 0; i < mComponentMinOrder.Count; i++)
                mComponentMinOrder[i].Graphic.SetDataLink(parent, mComponentMinOrder[i].Id);
        }

        public void SetMaterial(Material mat, bool isShared)
        {
            mMaterialManager.SetMaterial(this, mat, false);
            RunOnAllGraphics((x) => { x.SetMaterial(mMaterialManager.mCachedMaterial, true); });
        }

        public void OnCreated(int targetVertexCount)
        {
            TargetGraphicVertices = targetVertexCount;
            RunOnAllGraphics((x) => { x.OnCreated(TargetGraphicVertices); });
        }

        #region Tests
        [Conditional("UNITY_EDITOR")]
        void CheckCollectionIntegrity()
        {
            ChartIntegrity.Assert(() =>
            {
                return mComponentIds.Values.Distinct().Count() == mComponentIds.Count;
            });
            ChartIntegrity.Assert(() =>
            {
                return mComponentMinOrder.Distinct().Count() == mComponentMinOrder.Count;
            });

            ChartIntegrity.Assert(() =>    // asset that the order array is sorted
            {
                for (int i = 1; i < mComponentMinOrder.Count; i++)
                {
                    if (mComponentMinOrder[i - 1].Graphic.VertexCount < mComponentMinOrder[i].Graphic.VertexCount)
                        return false;
                }
                return true;
            });
            ChartIntegrity.Assert(() =>    // assert that each item's order points to the right place in the array
            {
                for (int i = 0; i < mComponentMinOrder.Count; i++)
                {
                    if (mComponentMinOrder[i].Order != i)
                        return false;
                }
                return true;
            });
            ChartIntegrity.Assert(() =>
            {
                if (mComponentIds.Count != mComponentMinOrder.Count)
                    return false;
                return mComponentIds.Values.Intersect(mComponentMinOrder).Count() == mComponentIds.Count;   // both have the same values
                    
            });
        }

        public void SetVisible(bool enabled)
        {
            mVisible = enabled;
            RunOnAllGraphics((x) => { x.SetVisible(mVisible); });
            Invalidate();
        }
        
        public void PrepareForCapacity(int capacity)
        {
            
        }
        #endregion
    }
}
