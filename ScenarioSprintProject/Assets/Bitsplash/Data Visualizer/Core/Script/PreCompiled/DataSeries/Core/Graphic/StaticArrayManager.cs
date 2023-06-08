using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThetaList;

namespace DataVisualizer{
    class StaticArrayManager : ArrayManagerBase
    {
        int mFixedItemSize;
        SimpleList<SeriesObject> mObjects = new SimpleList<SeriesObject>();
        bool mChanged = true;
        DataToArrayAdapter mAdapter = new DataToArrayAdapter();
        public StaticArrayManager(int itemSize,IVertexArray array,DataSeriesBase mapper,ushort context) : base(array,mapper, context)
        {
            mFixedItemSize = itemSize;
        }

        public override bool AddEntity(SeriesObject obj)
        {
            mChanged = true;
            obj.Attached();
            obj.EnsureItemCount(Mapper);
            int vertexCount = mFixedItemSize * obj.ItemCount;
            if (CanAddVertices(vertexCount) == false)
            {
                obj.SetRemoved();
                return false;
            }
            obj.Context = Context;
            mArray.AddVertices(vertexCount);   // add the size of the entity to the array
            return true;
        }

        public override void Clear()
        {
            mChanged = true;
            var array = mObjects.RawArrayWithExtraLastItem;
            int count = mObjects.Count;
            for (int i = 0; i < count; i++)
                array[i].SetRemoved();
            mObjects.Clear();
        }
        public override void MakeAllDirty(bool uvOnly, SimpleList<SeriesObject> takenOut)
        {
            mChanged = true;
            // this does not matter. When calling the WriteVertices all vertices are applied
        }
        
        public override MakeDirtyResult MakeEntityDirty(SeriesObject obj,bool uvOnly)
        {
            mChanged = true;
            // this does not matter. When calling the WriteVertices all vertices are applied
            return MakeDirtyResult.Succeeded;
        }

        public override void RemoveEntity(SeriesObject obj)
        {
            mChanged = true;
            mObjects.Remove(obj);
            obj.SetRemoved();
        }
        
        public override void OnAfterDataUpload()
        {
            base.OnAfterDataUpload();
            mArray.DiscardArray();
        }
        public override void OnBeforeDataUpload()
        {
            if (mChanged)
                mArray.EnsureArray();
        }
        public override void WriteVertices()
        {
            
            if (mChanged)
            {
                mAdapter.PopulateFrom(Mapper, mArray);
                ChartIntegrity.Assert(Math.Abs(mAdapter.mMultX) > 0.00000001);
                ChartIntegrity.Assert(Math.Abs(mAdapter.mMultY) > 0.00000001);
                var array = mObjects.RawArrayWithExtraLastItem;
                int count = mObjects.Count;
                int currentWrite = 0;
                for (int i = 0; i < count; i++)
                {
                    var obj = array[i];
                    for (int j = 0; j < obj.ItemCount; j++)
                    {
                        obj.WriteItemVertices(j,currentWrite, mAdapter);
                        obj.ClearDiry();
                        currentWrite += mFixedItemSize;
                    }
                }
                mChanged = false;
            }
        }

    }
}
