using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThetaList;
using UnityEngine;

namespace DataVisualizer{
    /// <summary>
    /// chunkes SeriesObjects in the vertex array to make modifications of SeriesObjects more efficient. 
    /// The implementation is built so that SeriesObjects that are frequently changde are more likely to be in the back of the array , thus taking less time to update.
    /// use case example: text labels often change the number of vertices they are made of, depending on their value. This
    /// Implementation details:
    /// 
    /// This Implementation contains two main arrays
    /// vertex array - inherited from ArrayManagerBase holds the actuall vertices of the graphic data
    /// helper array (of type ItemRef)- holds information about the fragmentation of graphic object within the array. 
    /// mStartArrayPosition - holds the minimum position in the array that was changed. So if only the last item in the array was changed , only that last item is iterated through. If it was the middle item , only half the array is iterated through
    /// 
    /// 1. When an SeriesObject is added to the manager it is tagged.
    /// 2. once a SeriesObject is added , it is imidiatly added to the array and has a vertex allocation
    /// 3. If a SeriesObject is marked dirty , WriteVertices will remove the object and reinsert at the end of the array. Thus objects that change frequently are more likely to be at the end of the array
    /// 4. When a SeriesObject is removed it is marked as "removing". In the WriteVertices loop all    instances of the object will be removed from the array completly.
    /// 
    /// additional notes:
    /// 1. when obj.IsChanging is marked it means that the item count of the object has changed , if an object is marked dirty and it's item count is the same it is not marked as changing
    /// </summary>
    class ChunkedArrayManager : ArrayManagerBase
    {
        struct ItemRef
        {
            public ItemRef(SeriesObject s, byte i)
            {
                Item = i;
                Series = s;
            }
            /// <summary>
            /// the item count within the SeriesObject
            /// </summary>
            public ushort Item;
            /// <summary>
            /// the series object
            /// </summary>
            public SeriesObject Series;
        }

        SimpleList<ItemRef> mItemRefs = new SimpleList<ItemRef>();

        const float DiryRatio = 10f;
        int mStartArrayPosition = 0;
        int mFixedSubItemSize;
        bool mAllDirty = false;

        HashSet<SeriesObject> mDirtyObjects = new HashSet<SeriesObject>();
        bool mIterateArray = false;
        DataToArrayAdapter mAdapter = new DataToArrayAdapter();
        int mNextArraySize;

        public ChunkedArrayManager(int fixedSubItemSize,IVertexArray array,DataSeriesBase mapper,ushort context)
            :base(array, mapper, context)
        {
            mFixedSubItemSize = fixedSubItemSize;
        }

        public override bool AddEntity(SeriesObject obj)
        {
            ChartCommon.DevLog(LogOptions.GraphicArrayManagers,GetType().Name, "Add entity", "index:",obj.MyIndex);
            if ((obj is SeriesObject) == false)
                throw new InvalidOperationException("chunked array manager can only work with SeriesObject items");
            mArray.EnsureArray();
            obj.Attached();
            obj.IsChanging = false;
            obj.MinArrayPosition = mArray.VertexCount;
            obj.IsTagged = false;
            // obj.IsTagged = true;    // if an object is tagged it means it was added to the array manager but not yet to the array
            //CheckSeriesObjectChanging(obj);
            obj.EnsureItemCount(Mapper);
            int objectVertexCount = obj.ItemCount * mFixedSubItemSize;
            if (CanAddVertices(objectVertexCount) == false)
            {
                obj.SetRemoved();
                obj.MinArrayPosition = -1;
                return false;
            }
            obj.Context = Context;
            obj.MinArrayPosition = mArray.VertexCount;
            InnerAdd(obj);
            if (AddAsDirtyObject(obj) == false)
                return false;
            ChartIntegrity.NotifyAddCollection(this, "main", obj);
            return true;
        }

        public override void MakeAllDirty(bool uvOnly, SimpleList<SeriesObject> takenOut)
        {
            ChartCommon.DevLog(LogOptions.GraphicArrayManagers, GetType().Name, "make all dirty", "uv only:",uvOnly);
            mArray.EnsureArray();
            mStartArrayPosition = 0;
            mAllDirty = true;
            IterateAll();
            for(int i=0; i<mItemRefs.Count; i++)
            {
                var series = mItemRefs[i].Series;
                if (series.IsDirty == false)
                    if (MakeEntityDirty(series, uvOnly) == MakeDirtyResult.Removed)
                        takenOut.Add(series);
            }
        }

        public override void Clear()
        {
            ChartCommon.DevLog(LogOptions.GraphicArrayManagers, GetType().Name, "clear");
            mArray.EnsureArray();
            mAllDirty = false;
            mStartArrayPosition = 0;
            var array = mItemRefs.RawArrayWithExtraLastItem;
            for (int i = 0; i < mItemRefs.Count; i++)
            {
                array[i].Series.SetRemoved();
                array[i].Series.MinArrayPosition = -1;
            }
            mArray.Clear();
            mItemRefs.Clear();
            ChartIntegrity.NotifyClearCollection(this, "main");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj">the series object to add</param>
        /// <param name="startFrom">the inner item index to start adding from</param>
        void InnerAdd(SeriesObject obj,int startFrom = 0)
        {
            ChartCommon.DevLog(LogOptions.GraphicArrayManagers, GetType().Name, "inner add", "index:", obj.MyIndex, "item count:", obj.ItemCount);
            Debug.Assert(obj.ItemCount > 0);
            int count = obj.ItemCount - startFrom;
            for (int i = startFrom; i < obj.ItemCount; i++)
                mItemRefs.Add(new ItemRef(obj, (byte)i));  // add all items to the array with a newObject mark
            mArray.AddVertices(count * mFixedSubItemSize);   // add the size of the entity to the array     
        }

        void RemoveObjectImidiate(SeriesObject obj)
        {
            if (obj.Context != Context)    // you can remove and object that does not belong to this object
                return;
            obj.Context = SeriesObject.EmptyContext;
            obj.SetRemoved();
            obj.MinArrayPosition = -1;
        }
        /// <summary>
        /// this will notify the array manager the the selected position requires an update. 
        /// </summary>
        /// <param name="updatedPosition"></param>
        void UpdateArrayPosition(int updatedPosition)
        {
            mStartArrayPosition = Math.Min(mStartArrayPosition, updatedPosition);
        }
        /// <summary>
        /// adds an object to the dirty list and marks it as dirty. returns false if the object could not fit into the array
        /// </summary>
        /// <param name="obj"></param>
        bool AddAsDirtyObject(SeriesObject obj)
        {
            obj.MakeDirty();
            UpdateArrayPosition(obj.MinArrayPosition);
            int itemCount = obj.ItemCount;
            if(obj.EnsureItemCount(Mapper))
            {
                int diff = obj.ItemCount - itemCount;
                if (diff > 0)
                {
                    if(CanAddVertices(diff * mFixedSubItemSize) == false)    // if the amount of vertices cannot be added
                    {
                        RemoveObjectImidiate(obj);
                        return false;
                    }
                    InnerAdd(obj, itemCount); //if new inner items were added. We append them into the array
                    obj.MinArrayPosition = Math.Min(mArray.VertexCount, obj.MinArrayPosition);
                }
                // if the item count was lower then the hight items will be removed in the next writeVertices call
            }
            
            if (mIterateArray)
                return true;
            if (CheckDirtyRatio())
                IterateAll();
            else
            {
                if (mIterateArray == false)
                    mDirtyObjects.Add(obj);
            }
            return true;
        }

        public override MakeDirtyResult MakeEntityDirty(SeriesObject obj,bool uvOnly)
        {
            ChartIntegrity.Assert(obj.Context == Context);
            ChartIntegrity.AssertCollectionContains(this, "main", obj);
            ChartCommon.DevLog(LogOptions.GraphicArrayManagers, GetType().Name, "make entity dirty");
            mArray.EnsureArray();
            if (AddAsDirtyObject(obj) == false)
            {
                ChartIntegrity.NotifyRemoveCollection(this, "main", obj);
                return MakeDirtyResult.Removed;
            }
            return MakeDirtyResult.Succeeded;
        }

        void IterateAll()
        {
            ChartCommon.DevLog(LogOptions.GraphicArrayManagers, GetType().Name, "iterate all");
            mIterateArray = true;
            mDirtyObjects.Clear();
        }

        bool CheckDirtyRatio()
        {
            if (mDirtyObjects.Count * DiryRatio > mItemRefs.Count)
                return true;
            return false;
        }

        public override void RemoveEntity(SeriesObject obj)
        {
            ChartIntegrity.Assert(obj.Context == Context);
            ChartIntegrity.AssertCollectionContains(this, "main", obj);
            ChartCommon.DevLog(LogOptions.GraphicArrayManagers, GetType().Name, "remove entity");
            mArray.EnsureArray();
            UpdateArrayPosition(obj.MinArrayPosition);
            RemoveObjectImidiate(obj);
            ChartIntegrity.NotifyRemoveCollection(this, "main", obj);
        }

        public override void OnBeforeDataUpload()
        {
           mArray.EnsureArray();
        }

        void RemoveAndCopy()
        {

        }

        void RemoveArrayItem(ref int i, ItemRef[] array,ref int count)
        {
            ChartCommon.DevLog(LogOptions.GraphicArrayManagers, GetType().Name, "remove array item", "item refs:", mItemRefs.Count, "vertices:", mArray.VertexCount);
            int last = mItemRefs.Count - 1;
            var obj = array[last].Series;
            array[i] = array[last];
            mItemRefs.RemoveAt(last);
            count = mItemRefs.Count;
            int arrayPosition = i * mFixedSubItemSize;
            mArray.CopyFromTo(last * mFixedSubItemSize, arrayPosition, mFixedSubItemSize); // copy the item to the new position
            mArray.RemoveVertices(mFixedSubItemSize);     
            if (obj.MinArrayPosition > arrayPosition)
                obj.MinArrayPosition = arrayPosition;
            i--;
            ChartCommon.DevLog(LogOptions.GraphicArrayManagers, GetType().Name, "remove array item result","item refs:",mItemRefs.Count,"vertices:",mArray.VertexCount);
        }

        /// <summary>
        /// unmarks an object at the end of WriteVertices loop.
        /// If the object was dirty or changing , it is no longer.
        /// if the object was removing , it is marked as removed
        /// </summary>
        /// <param name="obj"></param>
        void UnmarkObject(SeriesObject obj)
        {
            if (obj.Context != Context)
                return;
            obj.IsTagged = false;
            obj.IsChanging = false;
            obj.ClearDiry();
        }

        /// <summary>
        /// unmarks all objects in the array manager at the end WriteVertices loop
        /// </summary>
        /// <param name="obj"></param>
        void UnmarkAllObjects()
        {
            var array = mItemRefs.RawArrayWithExtraLastItem;
            int count = mItemRefs.Count;
            int start = mStartArrayPosition / mFixedSubItemSize;

            //clear all flags from objects
            if (mIterateArray)  // if a lot of objects were marked then iterate all the array
            {
                for (int i = start; i < count; i++)
                    UnmarkObject(array[i].Series);
            }
            else  //otherwise do only the object that were changed
            {
                mDirtyObjects.RemoveWhere((x) =>
                {
                    UnmarkObject(x);
                    return true;    // remove all
                });
            }
        }
        
        public override void WriteVertices()
        {
            //generate the array data adapter
            mAdapter.PopulateFrom(Mapper, mArray);
            //ChartIntegrity.Assert(Math.Abs(mAdapter.mMultX) > 0.00000001);
            //ChartIntegrity.Assert(Math.Abs(mAdapter.mMultY) > 0.00000001);
            ChartIntegrity.Assert((mAdapter.FromArray != mAdapter.ToArray) || mAdapter.fromOffset > 0);
            ChartCommon.DevLog(LogOptions.GraphicArrayManagers, GetType().Name, "write vertices - items added", "array size", mArray.VertexCount, "item count", mItemRefs.Count);
            var array = mItemRefs.RawArrayWithExtraLastItem;
            int count = mItemRefs.Count;
            int start = mStartArrayPosition / mFixedSubItemSize;
            for (int i = start; i < count; i++) // go through all the array items from mStartArrayPosition. 
            {           
                var itemRef = array[i];
                var obj = itemRef.Series;
                if (obj.Context != Context || itemRef.Item >= obj.ItemCount)   //if the object is marked as being removed  , or if this is a part of the object that was removed 
                {
                    ChartCommon.DevLog(LogOptions.GraphicArrayManagers, GetType().Name, "item removing", "index:", obj.MyIndex, "item:", itemRef.Item);
                    RemoveArrayItem(ref i, array, ref count);
                    continue; // don't redraw removed objects
                }
                // rewrite a dirty items vertices
                if (mAllDirty || obj.IsDirty)
                    obj.WriteItemVertices(itemRef.Item, i * mFixedSubItemSize, mAdapter);
            }
            UnmarkAllObjects();
            ChartIntegrity.AsseetCollectionDistinct(this, "main", mItemRefs.Select(x => x.Series).Cast<object>());
            ChartIntegrity.Assert(() =>    // once done all array elements should be clear
            {
                var arrayAssert = mItemRefs.RawArrayWithExtraLastItem;
                int countAssert = mItemRefs.Count;
                for (int i = 0; i < countAssert; i++)
                {
                    if (arrayAssert[i].Series.IsDirty)   // if an array element is dirty the asset fails
                        return false;
                }
                return true;
            });
            // reset all variables for next iteration
            mAllDirty = false;  
            mIterateArray = false;
            mStartArrayPosition = mArray.VertexCount;    // rest the start array position
        }
    }
}
