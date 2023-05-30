using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    /// <summary>
    /// a series object is an abstract object that represents a graphical entity in the graph. It is used instead of a unityobejct which consumes alot of performance and memory. This is a liter object that is contained in chart series
    /// </summary>
    public abstract class SeriesObject
    {
        public const ushort EmptyContext = ushort.MaxValue;
        /// <summary>
        /// this flags are cleared when calling clear dirty
        /// </summary>
        private const SeriesObjecFlags ClearDirtyFlags = ~(SeriesObjecFlags.Dirty | SeriesObjecFlags.UVDirty);
        private const SeriesObjecFlags AttachedObjectFlags = SeriesObjecFlags.Removed | SeriesObjecFlags.Removing;

        public SeriesObject()
        {
            SetRemoved();
            MinArrayPosition = -1;
        }

        /// <summary>
        /// a byte containing flags the indicate the state of the series object
        /// </summary>
        [Flags]
        public enum SeriesObjecFlags : byte
        {
            /// <summary>
            /// the series object is marked as dirty
            /// </summary>
            Dirty = 1,
            /// <summary>
            /// the legnth of the series object is marked as invalid
            /// </summary>
            LengthInvalid = 2,
            /// <summary>
            /// the series object is being removed from an array manager
            /// </summary>
            Removing = 4,
            /// <summary>
            /// the series object is not attached to an array manager , or has been removed from one
            /// </summary>
            Removed = 8,
            /// <summary>
            /// the series object structed is chanining (for example it's vertex count changes(
            /// </summary>
            Changing = 16,
            /// <summary>
            /// the uv of the series object is dirty
            /// </summary>
            UVDirty = 32,
            /// <summary>
            /// the series object is tagged (the meaning of this is implementation specific)
            /// </summary>
            Tag = 64
        }

        /// <summary>
        /// flags representing the state of the series object
        /// </summary>
        SeriesObjecFlags mFlags;
        /// <summary>
        /// the length of the series object . this field is used for uv mapping in some implementations
        /// </summary>
      //  double mLength = 0.0;
        /// <summary>
        /// the index in the underlying data array that is linked to this series object. 
        /// </summary>
        protected int mMyIndex;

        /// <summary>
        /// this is used for sorting series objects between multiple graphic components. this is essensily the index of the graphic object this object belongs to 
        /// </summary>
        protected ushort mContext = EmptyContext;

        public ushort Context { get { return mContext; } set { mContext = value; } }

        /// <summary>
        /// the index in the underlying data array that is linked to this series object. 
        /// </summary>
        public int MyIndex { get { return mMyIndex; } set { mMyIndex = value; } }

        /// <summary>
        /// the minimum position in the array Series Graphic array where this object is located. This can used when iterating a chunked array containing this object. if you start iterating the array at MinArrayPosition , you can be sure you will iterate through all the chunkes relating to this object
        /// </summary>
        public int MinArrayPosition { get; set; }

        /// <summary>
        /// true if this object is 3d and ZBounds should be used
        /// </summary>
        public abstract bool Is3D { get; }

        /// <summary>
        /// call this when this object is attached to a new renderer (see SeriesObjecFlags)
        /// </summary>
        public void Attached()
        {
            if (IsRemoving || IsRemoved == false)
                throw new Exception("Object is already attached to another object");
            mFlags = mFlags & ~SeriesObjecFlags.Removed;
            mFlags = mFlags & ~SeriesObjecFlags.Removing;

            ChartIntegrity.Assert(IsAttached);
        }

        public bool IsAttached
        {
            get { return (mFlags & AttachedObjectFlags) == 0; }
        }
        /// <summary>
        /// a boolean used by implemntation to tag this object. (the meaning of this is only according to implementation)
        /// </summary>
        public bool IsTagged
        {
            get
            {
                return (mFlags & SeriesObjecFlags.Tag) > 0;
            }
            set
            {
                if (value == true)
                    mFlags = mFlags | SeriesObjecFlags.Tag;
                else
                    mFlags = mFlags & ~SeriesObjecFlags.Tag;
            }
        }
        /// <summary>
        /// the series object structed is chanining (for example it's vertex count changes) (see SeriesObjecFlags)
        /// </summary>
        public bool IsChanging
        {
            get
            {
                return (mFlags & SeriesObjecFlags.Changing) > 0;
            }
            set
            {
                if (value == true)
                    mFlags = mFlags | SeriesObjecFlags.Changing;
                else
                    mFlags = mFlags & ~SeriesObjecFlags.Changing;
            }
        }

        /// <summary>
        /// changes the state flags of this object to removing (see SeriesObjecFlags)
        /// </summary>
        public void SetRemoving()
        {
            mFlags = mFlags | SeriesObjecFlags.Removing;
            mFlags = mFlags & ~SeriesObjecFlags.Removed;
        }
        /// <summary>
        /// changes the state flags of this object to removed (see SeriesObjecFlags)
        /// </summary>
        public void SetRemoved()
        {
            mFlags = mFlags & ~SeriesObjecFlags.Removing;
            mFlags = mFlags | SeriesObjecFlags.Removed;
            Context = EmptyContext;
            ChartIntegrity.Assert(IsRemoved);
        }
        /// <summary>
        /// true if this objects flags are marked as "removing" (see SeriesObjecFlags)
        /// </summary>
        public bool IsRemoving
        {
            get
            {
                return (mFlags & SeriesObjecFlags.Removing) > 0;
            }
        }

        /// <summary>
        /// (see SeriesObjecFlags)
        /// </summary>
        private bool LengthInvalid
        {
            get
            {
                return (mFlags & SeriesObjecFlags.LengthInvalid) > 0;
            }
            set
            {
                if (value == true)
                    mFlags = mFlags | SeriesObjecFlags.LengthInvalid;
                else
                    mFlags = mFlags & ~SeriesObjecFlags.LengthInvalid;
            }
        }

        /// <summary>
        /// (see SeriesObjecFlags)
        /// </summary>
        protected bool UvDirty
        {
            get
            {
                return (mFlags & SeriesObjecFlags.UVDirty) > 0;
            }
            set
            {
                if (value == true)
                    mFlags = mFlags | SeriesObjecFlags.UVDirty;
                else
                    mFlags = mFlags & ~SeriesObjecFlags.UVDirty;
            }
        }

        /// <summary>
        /// (see SeriesObjecFlags)
        /// </summary>
        protected bool Dirty
        {
            get
            {
                return (mFlags & SeriesObjecFlags.Dirty) > 0;
            }
            set
            {
                if (value == true)
                    mFlags = mFlags | SeriesObjecFlags.Dirty;
                else
                    mFlags = mFlags & ~SeriesObjecFlags.Dirty;
            }
        }

        /// <summary>
        /// true if this object is marked as "removed" (see SeriesObjecFlags)
        /// </summary>
        public bool IsRemoved
        {
            get
            {
                return (mFlags & SeriesObjecFlags.Removed) > 0;
            }
        }

        /// <summar               y>
        /// notifies this object of it's current index value in the object array of the series. The index may change if the chart data has changed
        /// </summary>
        /// <param name="index"></param>
        public void NotifyIndex(int index)
        {
            MyIndex = index;
        }
         
        /// <summary>
        /// invalidates the length of this objects. the length will be recalculated next time it is requested by another object
        /// </summary>
        protected void InvalidateLength()
        {
            LengthInvalid = true;
        }

        /// <summary>
        /// writes the vertices of an item in this series object into the arrays in the specified array index.
        /// This method is called by an array manager in order to convert data points into mesh vertices. the vertices are to be written in the index "position"
        /// 
        /// </summary>
        /// <param name="itemIndex"></param>
        /// <param name="position"></param>
        /// <param name="arrays"></param>
        public abstract void WriteItemVertices(int itemIndex, int position, DataToArrayAdapter arrays);
        /// <summary>
        /// returns true if the item count has changed
        /// </summary>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public abstract bool EnsureItemCount(DataSeriesBase mapper);
        public abstract int ItemCount { get; }

        /// <summary>
        /// marks this object as not dirty
        /// </summary>
        public virtual void ClearDiry()
        {
            mFlags = mFlags & ClearDirtyFlags;
            ChartIntegrity.Assert(IsDirty == false);
        }

        public bool IsDirty
        {
            get { return Dirty || UvDirty; }
        }

        /// <summary>
        /// discards the data of this object , if this object holds any data
        /// </summary>
        public abstract void DiscardData();
        /// <summary>
        /// true if ItemSize * ItemCount never changes when the data value of this object is modified ( however it can change if settings change)
        /// </summary>
        protected virtual bool CompactSize { get { return false; } }
        /// <summary>
        /// the size of items in a
        /// </summary>
        public virtual int ItemSize { get { return 4; } }
        /// <summary>
        /// calculates the length of the object along the uv line
        /// </summary>
        /// <returns></returns>
        protected abstract double CalculateLength(DataSeriesBase mapper);

        /// <summary>
        /// Makes the entire object dirty
        /// </summary>
        public abstract void MakeDirty();

        /// <summary>
        /// Makes only the uv of the object dirty
        /// </summary>
        public virtual void MakeUvDirty()
        {
            UvDirty = true;
        }

        /// <summary>
        /// called when this object is destroyed
        /// </summary>s
        public abstract void Destory();

        /// <summary>
        /// caculates the length of this object . This is used for uv mapping. The length should be cached for future use and invalidated when the object is modified
        /// </summary>
        /// <param name="accumilated"></param>
        /// <returns></returns>
        public double Legnth(DataSeriesBase mapper)
        {
            return CalculateLength(mapper);
            //if(LengthInvalid)
            //{
            //    LengthInvalid = false;
            //    mLength = CalculateLength(mapper);
            //}
            //return mLength;
        }
    }
}
