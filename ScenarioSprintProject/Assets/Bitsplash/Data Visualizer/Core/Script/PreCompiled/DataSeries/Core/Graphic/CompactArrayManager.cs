using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThetaList;
using UnityEngine;

namespace DataVisualizer{
    /// <summary>
    /// if the vertex size of each object is fixed. then using this array manager will increase performance
    /// This array manager will keep only the dirty items. so when refreshing the array only dirty items are refreshed and no need to traverse the array
    /// If there are too many dirty items then the array is traversed, this improves memory consumption of collecting all dirty objects.
    /// This array manager has a realtime mode that yields most performance with a specialed method, it is meant for use cases were all the series objects are updated each frame
    /// </summary>
    class CompactArrayManager : ArrayManagerBase
    {
        /// <summary>
        /// If the chart contains n items , then this array manager is allowed to store n/DiryRatio dirty series objects before triggering a full array iteration. This is used to save memory
        /// </summary>
        const float DiryRatio = 10f;
        /// <summary>
        /// the size of a series object entity in vertices. This array manager assumes the size is constant for all objects
        /// </summary>
        int mFixedEntitySize;
        /// <summary>
        /// a set of the dirty objects for the current frame
        /// </summary>
        SimpleList<SeriesObject> mDirtyObjects = new SimpleList<SeriesObject>();
        /// <summary>
        /// if true , the array will be traversed when writing vertices.
        /// </summary>
        bool mIterateArray = false;
        /// <summary>
        /// if true , all the objects in the array manager are considered dirty
        /// </summary>
        bool mAllDirty = false;
        /// <summary>
        /// if true, this array manager is for the purpose of handling realtime data ( all data values change each frame for example a wave from )
        /// </summary
        bool mRealtime;
        /// <summary>
        /// an array that contains the items contained in this array manager, in the order at which their vertices are set in the vertex array.
        /// for example if this array is set like this : obj1|obj3|obj2
        /// then the vertex array is set like this : obj1.v1|obj1.v2|obj1.v3|obj1.v4|obj3.v1|obj3.v2|obj3.v3|obj3.v4|obj2.v1|obj2.v2|obj2.v3|obj2.v4
        /// </summary>
        SimpleList<SeriesObject> mItemRefs = new SimpleList<SeriesObject>();
        /// <summary>
        /// 
        /// </summary>
        DataToArrayAdapter mAdapter = new DataToArrayAdapter();

        public CompactArrayManager(bool isRealtime,int fixedEntitySize,IVertexArray array,DataSeriesBase mapper,ushort context)
            :base(array,mapper, context)
        {

            mRealtime = isRealtime;
            mFixedEntitySize = fixedEntitySize;
        }

        public override void Clear()
        {
            mArray.EnsureArray();
            mAllDirty = false;
            var array = mItemRefs.RawArrayWithExtraLastItem;
            for (int i = 0; i < mItemRefs.Count; i++)
            {
                array[i].SetRemoved();
                array[i].MinArrayPosition = -1;
            }
            mArray.Clear();
            mItemRefs.Clear();
            ChartIntegrity.NotifyClearCollection(this, "main");
        }

        public override bool AddEntity(SeriesObject obj)
        {
            ChartCommon.DevLog(LogOptions.GraphicArrayManagers, GetType().Name, "Add entity");
            
            mArray.EnsureArray();
            obj.Attached();
            obj.EnsureItemCount(Mapper);
            if (obj.ItemCount > 1)
                throw new NotSupportedException("Compact series can only have series objects with 1 item");
            if (CanAddVertices(mFixedEntitySize) == false)
            {
                obj.SetRemoved();
                return false;
            }
            obj.Context = Context;
            mItemRefs.Add(obj);
            obj.MinArrayPosition = mArray.VertexCount;
            mArray.AddVertices(mFixedEntitySize);   // add the size of the entity to the array                
            ChartIntegrity.NotifyAddCollection(this, "main", obj);
            MakeEntityDirty(obj,false);
            return true;
        }

        public override void OnBeforeDataUpload()
        {
            mArray.EnsureArray();
        }

        public override MakeDirtyResult MakeEntityDirty(SeriesObject obj,bool uvOnly)
        {
            ChartIntegrity.Assert(obj.Context == this.Context);
            ChartIntegrity.AssertCollectionContains(this, "main", obj);
            ChartCommon.DevLog(LogOptions.GraphicArrayManagers, GetType().Name, "Make dirty","index:",obj.MyIndex);
            if (obj.IsDirty)    // the object was already added to dirty objects
                return MakeDirtyResult.Succeeded;
            mArray.EnsureArray();
            obj.MakeDirty();
            if (mIterateArray)
                return MakeDirtyResult.Succeeded;
            if (CheckDirtyRatio())
                IterateAll();
            else
            {
                if (mIterateArray == false)
                    mDirtyObjects.Add(obj);
            }
            return MakeDirtyResult.Succeeded;
        }

        void IterateAll()
        {
            mIterateArray = true;
            mDirtyObjects.Clear();
        }

        /// <summary>
        /// if the ratio is above t
        /// </summary>
        /// <returns></returns>
        bool CheckDirtyRatio()
        {
            ChartCommon.DevLog(LogOptions.GraphicArrayManagers, GetType().Name, "Dirty Ratio", "dirty count: ", mDirtyObjects.Count,"item refs: ", mItemRefs.Count,"exceeded:", mDirtyObjects.Count * DiryRatio > mItemRefs.Count);
            if (mDirtyObjects.Count * DiryRatio > mItemRefs.Count) 
                return true;
            return false;
        }

        /// <summary>
        /// removes an entity by copying the last entity into its position
        /// </summary>
        /// <param name="obj"></param>
        public override void RemoveEntity(SeriesObject obj)
        {
            ChartIntegrity.Assert(obj.Context == this.Context);
            ChartIntegrity.AssertCollectionContains(this, "main", obj);
            mArray.EnsureArray();
            if (obj.MinArrayPosition < 0)
                return;
            var itemRefs = mItemRefs;
            int last = itemRefs.Count - 1;            
            int current = obj.MinArrayPosition / mFixedEntitySize;
            itemRefs[current] = itemRefs[last];
            itemRefs[current].MinArrayPosition = current * mFixedEntitySize;
            itemRefs.RemoveAt(last);
            mArray.CopyFromTo(mArray.VertexCount - mFixedEntitySize, obj.MinArrayPosition, mFixedEntitySize); // copy the item to the new position
            mArray.RemoveVertices(mFixedEntitySize);
            obj.MinArrayPosition = -1;
            obj.SetRemoved();
            ChartIntegrity.NotifyRemoveCollection(this, "main", obj);
        }

        public override void MakeAllDirty(bool uvOnly, SimpleList<SeriesObject> takenOut)
        {
            ChartCommon.DevLog(LogOptions.GraphicArrayManagers, GetType().Name, "Make all dirty", "uvonly:", uvOnly);
            mArray.EnsureArray();
            mAllDirty = true;
            IterateAll();
        }

        /// <summary>
        /// realtime updates are done with this realtime optimized method. It does not contain any excess calculations
        /// </summary>
        public void RealtimeUpdate()
        {
            ChartCommon.DevLog(LogOptions.GraphicArrayManagers, GetType().Name, "Realtime write vertices", "count:", mItemRefs.Count);
            var mainArray = mArray;
            var array = mItemRefs.RawArrayWithExtraLastItem;
            int count = mItemRefs.Count;
            for (int i = 0,writePos = 0; i < count; i++, writePos+=mFixedEntitySize)
                array[i].WriteItemVertices(0, writePos, mAdapter);
            mIterateArray = false;
        }
       
        public override void WriteVertices()
        {
            ChartIntegrity.AsseetCollectionDistinct(this, "main", mItemRefs.Cast<object>());
            //after items has been created , we generate the array data adapter
            mAdapter.PopulateFrom(Mapper, mArray);
            //ChartIntegrity.Assert(Math.Abs(mAdapter.mMultX) > 0.00000001);
            //ChartIntegrity.Assert(Math.Abs(mAdapter.mMultY) > 0.00000001);
            //  mArray.EnsureArray();
            var mainArray = mArray;
            if(mRealtime)
                RealtimeUpdate();
            else
            {
                if (mIterateArray)      // we iterate the array if to many objects are dirty, to the point that iterating only them will be memory consuming
                {
                    ChartCommon.DevLog(LogOptions.GraphicArrayManagers, GetType().Name, Mapper.GetType(), "Iterate array", "count:", mItemRefs.Count);
                    var array = mItemRefs.RawArrayWithExtraLastItem;
                    int count = mItemRefs.Count;
                    for (int i = 0; i < count; i++)
                    {
                        var obj = array[i];
                        ChartIntegrity.Assert(obj.Context == this.Context);
                        if (mAllDirty || obj.IsDirty)       // if an object is dirty it is rewritten to the array
                        {
                            ChartCommon.DevLog(LogOptions.GraphicArrayManagers, GetType().Name, Mapper.GetType(), "dirty object", "index:", obj.MyIndex);
                            obj.WriteItemVertices(0, i * mFixedEntitySize, mAdapter);   
                            obj.ClearDiry();
                        }
                    }
                    mIterateArray = false;

                }
                else
                {
                    ChartCommon.DevLog(LogOptions.GraphicArrayManagers, GetType().Name, Mapper.GetType(), "Write dirty", "count:", mDirtyObjects.Count);
                    int dirtyCount = mDirtyObjects.Count;
                    for (int i= 0; i< dirtyCount; i++) // if not many objects were dirty , they are written to the array individually.
                    {
                        SeriesObject obj = mDirtyObjects[i];
                        if (!obj.IsRemoved && !obj.IsRemoving)
                        {
                            ChartIntegrity.Assert(obj.Context == this.Context);
                            ChartCommon.DevLog(LogOptions.GraphicArrayManagers, GetType().Name, Mapper.GetType(), "dirty object", "index:", obj.MyIndex);
                            obj.WriteItemVertices(0, obj.MinArrayPosition, mAdapter);
                            obj.ClearDiry();
                        }
                    }
                }
            }

            ChartIntegrity.Assert(() =>    // once done all array elements should be clear
            {
                var array = mItemRefs.RawArrayWithExtraLastItem;
                int count = mItemRefs.Count;
                for (int i = 0; i < count; i++)
                {
                    if (array[i].IsDirty)   // if an array element is dirty the asset fails
                        return false;
                }
                return true;
            });

            mDirtyObjects.Clear();  // dirty objects are cleared at the end
            mAllDirty = false;
        }
    }
}
