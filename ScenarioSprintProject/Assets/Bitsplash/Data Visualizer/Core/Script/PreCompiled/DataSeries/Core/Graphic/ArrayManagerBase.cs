using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThetaList;
using UnityEngine;

namespace DataVisualizer{
    /// <summary>
    /// base class for array managers. 
    /// array managers , manage the way vertices are written in to the mesh array. The goal of array managers is to maximize performance for a specific task (realtime,streaming,static ,etc)
    /// most implementations should and will write vertices only to the portions of the array that were changed , in order to save cpu time.
    /// Array managers are handled using SeriesObjects , these obejcts are added and removed from the array manage. once they are contain within the array manager , they will be drawn into the mesh array.
    /// When an object is marked as dirty , it means the underlying data has changed and it's vertices require redrawing. for most implementations , non dirty objects are skipped to maximize performance
    /// </summary>
    abstract class ArrayManagerBase 
    {
        /// <summary>
        /// the context that identify
        /// </summary>
        public ushort Context { get;  set; }
        /// <summary>
        /// an abstract representation of a mesh array object. the vertices are written into this array
        /// </summary>
        protected IVertexArray mArray { get; private set; }
        /// <summary>
        /// the data series object that is linked to this array manager. It can be used to query information about the data series
        /// </summary>
        public DataSeriesBase Mapper { get;  set; }

        public ArrayManagerBase(IVertexArray array,DataSeriesBase mapper,ushort context)
        {
            mArray = array;
            Mapper = mapper;
            Context = context;
        }


        protected bool CanAddVertices(int vertexCount)
        {
            if (mArray.VertexCount + vertexCount >= mArray.VertexCapacity)
            {
                ChartCommon.DevLog(LogOptions.GraphicArrayManagers, GetType().Name, "can add vertices","failed","array vertex count:",mArray.VertexCount, "add vertex count:", vertexCount,"vertex capacity:", mArray.VertexCapacity);
                return false;
            }
            ChartCommon.DevLog(LogOptions.GraphicArrayManagers, GetType().Name, "can add vertices", "successs");
            return true;
        }

        /// <summary>
        /// clear
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// makes all series object in the array manager dirty. this means they will all be drawn into the array in the next iteration.
        /// If because of changes to some object, some objects cannot be fit into the array manager anymore, they are added to takenOut param. They are also removed from the array manager 
        /// </summary>
        /// <param name="uvOnly"></param>
        public abstract void MakeAllDirty(bool uvOnly,SimpleList<SeriesObject> takenOut);

        /// <summary>
        /// Adds a SeriesObject into the array manager. Once a series object is added it may not be added into another array manager until it is removed.
        /// returns false if the entity could not be added ( this usually means that the array manager is full )
        /// </summary>
        /// <param name="obj"></param>
        public abstract bool AddEntity(SeriesObject obj);

        /// <summary>
        /// makes a series object dirty(the objects vertices should be recalculated). The series object muse be part of the array manager. 
        /// if the capacity if the array manager cannot fit the object after being marked as dirty the object is taken out of the manager. see MakeDirtyResult
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="uvOnly"></param>
        public abstract MakeDirtyResult MakeEntityDirty(SeriesObject obj,bool uvOnly);

        /// <summary>
        /// removes a series object from the array manager. It can only be removed if it is attached to this array manager
        /// </summary>
        /// <param name="obj"></param>
        public abstract void RemoveEntity(SeriesObject obj);

        /// <summary>
        /// This method is called by a Graphic object in order to obtain the vertices of the mesh. This method will apply all dirty vertices into the array and will mark all objects as not dirty
        /// </summary>
        public abstract void WriteVertices();


        /// <summary>
        /// This method is called by a Graphic object before calling WriteVertices. This method is used in order to allocate or discard data on mArray.
        /// for example for some uses the vertex array is uploaded into the mesh and then discarded to consume memory. in such case This method will allocate a new array
        /// </summary>
        public abstract void OnBeforeDataUpload();

        /// <summary>
        /// This method is called by a Graphic after before calling WriteVertices. This method is used in order to allocate or discard data on mArray.
        /// for example for some uses the vertex array is uploaded into the mesh and then discarded to consume memory. in such case This method will discard the array after it has been applied to the mesh
        /// </summary>
        public virtual void OnAfterDataUpload()
        {

        }
    }
}
