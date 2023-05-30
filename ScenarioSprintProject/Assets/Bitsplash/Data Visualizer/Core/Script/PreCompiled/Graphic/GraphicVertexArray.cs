using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThetaList;
using UnityEngine;
using UnityEngine.Rendering;

namespace DataVisualizer{
    /// <summary>
    /// a vertex array manager that can be used with all chart graphic objects.
    /// This method holds the vsertex arrays and allocated/decallocated the data according the requirement set by it's properties. This way only needed vertex data is allocated
    /// </summary>
    public class GraphicVertexArray
    {
        private SimpleList<Vector3> mPositions;
        private SimpleList<Vector2> mUvs;
        private SimpleList<Vector2> mMappedUvs = new SimpleList<Vector2>(false, 5000);
        private SimpleList<Vector4> mTangents;
        private SimpleList<Color32> mColors;
        private SimpleList<Vector3> mExtrudedPositions = new SimpleList<Vector3>(false, 5000);
        private List<int> mIndices;

        private Vector3[] mPositionsArray;
        private Vector2[] mUVArray;
        private Vector4[] mTangentArray;
        private Color32[] mColorArray;

        bool mHasTangent;
        bool mHasColor;
        bool mArrayDiscarded = false;
        MeshArrays mArrays = new MeshArrays();
        int mArraySize = 65000;
        int mLastVertexCount = 0;
        bool mTriangleStrip;

        Action<int> mAddMethod;
        Action<int> mRemoveMethod;
        Action<int, int, int> mCopyMethod;

        const MeshUpdateFlags mUpdateFlags = MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds;
        public GraphicVertexArray(int arraySize)
        {
            SelectMethods();
            mArraySize = arraySize;
            AllocateArrays();
        }

        /// <summary>
        /// ensures the array is allocated and ready for work. If the array was discarded it will be recreated
        /// </summary>
        public void EnsureArray()
        {  
            if(mArrayDiscarded)
            {
                AllocateArrays();
                TakeRawArrays();
            }
            //else
            //    EnsureOptionalArrays();
        }

        public bool ArrayDiscarded
        {
            get { return mArrayDiscarded; }
        }

        public MeshArrays GetArrays()
        {
            return mArrays;
        }
        
        /// <summary>
        /// allocates the data arrays
        /// </summary>
        public void AllocateArrays()
        {

            mArrayDiscarded = false;
            mPositions = new SimpleList<Vector3>(true,mArraySize);
            mPositions.MaxCapacity = mArraySize;
            int tringleNum = (mArraySize * 6) / 4;   // number of tringles based on the vertices
            mIndices = new List<int>(tringleNum);  
            mUvs = new SimpleList<Vector2>(true, mArraySize);
            mUvs.MaxCapacity = mArraySize;

            EnsureOptionalArrays();

        }

        /// <summary>
        /// ensures the optional arrays in the vertex array are either alloated or disposed (depending on the settings)
        /// </summary>
        void EnsureOptionalArrays()
        {
            
            if (mArrayDiscarded)     // this is only releant if the arrays is present
                return;
            if (HasTangent)
            {
                if (mTangents != null)
                    mTangents.SetCount(mPositions.Count);
                else
                {
                    mTangents = new SimpleList<Vector4>(true, mArraySize);
                    mTangents.MaxCapacity = mArraySize;
                    mTangents.AddEmpty(mPositions.Count);
                }
                ChartIntegrity.Assert(mTangents.Count == mPositions.Count);
            }
            else
                mTangents = null;  
            if (HasColor)
            {
                if (mColors != null)
                    mColors.SetCount(mPositions.Count);
                else
                {
                    mColors = new SimpleList<Color32>(true, mArraySize);
                    mColors.MaxCapacity = mArraySize;
                    mColors.AddEmpty(mPositions.Count);
                }
                ChartIntegrity.Assert(mColors.Count == mPositions.Count);
            }
            else
                mColors = null;
            if (mArrayDiscarded == false)
                TakeRawArrays();
        }

        /// <summary>
        /// discards the data held in the array in order to save memory
        /// </summary>
        public void DiscardArray()
        {
            mArrayDiscarded = true;

            mArrays.mPositionsArray = mPositionsArray = null;
            mArrays.mUVArray = mUVArray = null;
            mArrays.mColorArray = mColorArray = null;
            mArrays.mTangentArray = mTangentArray = null;

            mPositions = new SimpleList<Vector3>(true,1); // used for getting the count of the object
            mUvs = null;
            mTangents = null;
            mColors = null;
            mIndices = null;
        }

        /// <summary>
        /// Takes the raw arrays from the SimpleList instances. They are used for faster access
        /// </summary>
        void TakeRawArrays()
        {
            mArrays.mPositionsArray = mPositionsArray = mPositions.RawArrayWithExtraLastItem;
            mArrays.mUVArray = mUVArray = mUvs.RawArrayWithExtraLastItem;
            if (mHasColor)
                mArrays.mColorArray = mColorArray = mColors.RawArrayWithExtraLastItem;
            else
                mArrays.mColorArray = mColorArray = null;
            if (mHasTangent)
                mArrays.mTangentArray = mTangentArray = mTangents.RawArrayWithExtraLastItem;
            else
                mArrays.mTangentArray = mTangentArray = null;
        }

        public void Add(int count)
        {
            mAddMethod(count);
        }

        public void Remove(int count)
        {
            mRemoveMethod(count);
        }

        public void CopyFromTo(int fromIndex, int toIndex, int itemCount)
        {
            mCopyMethod(fromIndex, toIndex, itemCount);
        }

        public int Count
        {
            get
            {
                return mPositions.Count;
            }
        }

        void SelectMethods()
        {
            if(mHasTangent && mHasColor)
            {
                mAddMethod = AddAll;
                mRemoveMethod = RemoveAll;
                mCopyMethod = CopyFromToAll;
                return;
            }
            if(mHasTangent)
            {
                mAddMethod = AddTangentOnly;
                mRemoveMethod = RemoveTangentOnly;
                mCopyMethod = CopyFromToTangentOnly;
                return;
            }
            if(mHasColor)
            {
                mAddMethod = AddColorOnly;
                mRemoveMethod = RemoveColorOnly;
                mCopyMethod = CopyFromToColorOnly;
                return;
            }
            mAddMethod = AddMinimal;
            mRemoveMethod = RemoveMinimal;
            mCopyMethod = CopyFromToMinimal;

        }

        public void ExtrudePositions(IChartGraphic graphic, Mesh mesh)
        {
            ChartIntegrity.Assert((HasTangent && mTangents != null) || (HasTangent == false && mTangents == null));
            if(HasTangent == false) // no tangents so no extrusion
            {
                mesh.vertices = mPositions.RawArrayWithExtraLastItem;
                return;
            }
            ChartIntegrity.Assert(mTangents.Count == mPositions.Count);
            ChartIntegrity.AssertMethodCalled((Action<IChartGraphic, Mesh, bool, bool>)AssignToMesh);   // this method works on a mesh that has already been created and is in use
            var extrudedPositions = mExtrudedPositions;
            ChartCommon.DevLog("software extruion", "on");
            Vector3 scale;
            if(graphic.ParentGraphic != null)
                scale = graphic.ParentGraphic.gameObject.transform.localScale;
            else
                scale = graphic.gameObject.transform.localScale;
            Vector3 invertScale = ChartCommon.InvertScale(scale);
            if (graphic.MaintainAngles == false)
                scale = new Vector3(1f, 1f, 1f);
            var directionTransform = graphic.DirectionTranform;
            float extrusion = graphic.ExtrusionAmount;
            int i = 0;
            var posions = mPositions.RawArrayWithExtraLastItem;
            var tangents = mTangents.RawArrayWithExtraLastItem;
            int to = mPositions.Count +1;
            extrudedPositions.SetCount(to);
            for (i = 0; i < to; i++)
            {

                Vector3 v = posions[i];
                // ChartCommon.DevLog("vertex", v.position);
                Vector4 tangent = tangents[i];
                float mag = tangent.z;
                float w = tangent.w;
                float sign = Mathf.Sign(w);
                tangent = new Vector4(tangent.x * scale.x, tangent.y * scale.y, 0f);

                Vector3 normal = ChartCommon.Perpendicular((Vector3)tangent);

                normal.x *= sign;
                normal.y *= sign;
                Vector3 dir = directionTransform * Vector3.LerpUnclamped(tangent, normal, Mathf.Abs(w)).normalized;
                Vector3 add = ChartCommon.MultiplayVector3(dir * extrusion * mag , invertScale);
                extrudedPositions[i] = v + add;
                //   ChartCommon.DevLog("vertex final", v.position);s
            }
            for(; i<extrudedPositions.Count; i++)
            {
                extrudedPositions[i] = Vector3.zero;
            }
            mesh.SetVertices(extrudedPositions.RawArrayWithExtraLastItem,0,to, mUpdateFlags);
        }

        public void MapUvs(IChartGraphic graphic,Mesh mesh)
        {
            ChartIntegrity.AssertMethodCalled((Action<IChartGraphic, Mesh, bool, bool>)AssignToMesh);  // this method works on a mesh that has already been created and is in use
            var mappedUvs = mMappedUvs;
            
            var Uvs = mUvs.RawArrayWithExtraLastItem;
            int to = mUvs.Count + 1;
            mappedUvs.SetCount(to);
            var mapping = graphic.UvMapping;
            for (int i = 0; i < to; i++)
                mappedUvs[i] = mapping.MapUv(Uvs[i]);
            mesh.SetUVs(0,mappedUvs.RawArrayWithExtraLastItem,0,to, mUpdateFlags);
        }

        public void AssignToMesh(IChartGraphic graphic,Mesh mesh, bool extrudeVertices,bool mapUvs)
        {
            ChartIntegrity.Assert(mPositions.Count == mUvs.Count);
            ChartIntegrity.NotifyMethodCall((Action< IChartGraphic , Mesh , bool , bool>)AssignToMesh);

            mesh.Clear();
            if (extrudeVertices)
                ExtrudePositions(graphic, mesh);
            else
                mesh.SetVertices(mPositions.RawArrayWithExtraLastItem, 0, mPositions.Count +1, mUpdateFlags);

            if(mapUvs)
                MapUvs(graphic, mesh);
            else
                mesh.SetUVs(0,mUvs.RawArrayWithExtraLastItem, 0, mUvs.Count +1 , mUpdateFlags);

            if (HasTangent)
                mesh.SetTangents(mTangents.RawArrayWithExtraLastItem, 0, mTangents.Count +1, mUpdateFlags);
            else
                mesh.SetTangents((Vector4[])null);

            if (HasColor)
                mesh.SetColors(mColors.RawArrayWithExtraLastItem, 0, mColors.Count+1, mUpdateFlags);
            else
                mesh.SetColors((Color[])null);

            if (mTriangleStrip)
                AddTringleStrip();
            else
                AddQuadTringles();
            mesh.SetTriangles(mIndices, 0);
            mesh.RecalculateBounds();
            var b = mesh.bounds;
            b = FixAABB(b);
            b.extents += new Vector3(1f, 1f, 1f);
            mesh.bounds = b;
        }

        Bounds FixAABB(Bounds input)
        {
            float x = input.center.x;
            float y = input.center.y;
            float z = input.center.z;
            if (float.IsNaN(x) || float.IsInfinity(x))
                x = 0f;
            if (float.IsNaN(y) || float.IsInfinity(y))
                y = 0f;
            if (float.IsNaN(z) || float.IsInfinity(z))
                z = 0f;
            float ex = input.size.x;
            float ey = input.size.y;
            float ez = input.size.z;
            if (float.IsNaN(ex) || float.IsInfinity(ex))
                ex = 0f;
            if (float.IsNaN(ey) || float.IsInfinity(ey))
                ey = 0f;
            if (float.IsNaN(ez) || float.IsInfinity(ez))
                ez = 0f;
            return new Bounds(new Vector3(x, y, z), new Vector3(ex, ey, ez));
        }
        #region Add variants
        void AddAll(int count)
        {
            ChartIntegrity.Assert(mHasTangent);
            ChartIntegrity.Assert(mHasColor);
            mArrays.mPositionsArray = mPositionsArray = mPositions.AddEmpty(count);
            mArrays.mUVArray = mUVArray = mUvs.AddEmpty(count);
            mArrays.mColorArray = mColorArray = mColors.AddEmpty(count);
            mArrays.mTangentArray = mTangentArray = mTangents.AddEmpty(count);
            ChartIntegrity.Assert(mTangents.Count == mPositions.Count && mPositions.Count == mUvs.Count && mColors.Count == mPositions.Count);
        }

        void AddTangentOnly(int count)
        {
            ChartIntegrity.Assert(mHasTangent);
            ChartIntegrity.Assert(!mHasColor);
            mArrays.mPositionsArray = mPositionsArray = mPositions.AddEmpty(count);
            mArrays.mUVArray = mUVArray = mUvs.AddEmpty(count);
            mArrays.mTangentArray = mTangentArray = mTangents.AddEmpty(count);
            ChartIntegrity.Assert(mTangents.Count == mPositions.Count && mPositions.Count == mUvs.Count);
        }

        void AddColorOnly(int count)
        {
            ChartIntegrity.Assert(!mHasTangent);
            ChartIntegrity.Assert(mHasColor);
            mArrays.mPositionsArray = mPositionsArray = mPositions.AddEmpty(count);
            mArrays.mUVArray = mUVArray = mUvs.AddEmpty(count);
            mArrays.mColorArray = mColorArray = mColors.AddEmpty(count);
            ChartIntegrity.Assert(mPositions.Count == mUvs.Count && mColors.Count == mPositions.Count);
        }

        void AddMinimal(int count)
        {
            ChartIntegrity.Assert(!mHasTangent);
            ChartIntegrity.Assert(!mHasColor);
            mArrays.mPositionsArray = mPositionsArray = mPositions.AddEmpty(count);
            mArrays.mUVArray = mUVArray = mUvs.AddEmpty(count);
            ChartIntegrity.Assert(mPositions.Count == mUvs.Count);
        }
        #endregion

        #region CopyFromTo variants
        protected void CopyFromToAll(int fromIndex, int toIndex, int itemCount)
        {
            ChartIntegrity.Assert(mHasTangent);
            ChartIntegrity.Assert(mHasColor);
            if (fromIndex < toIndex)
            {
                for (int i = itemCount - 1; i >= 0; i--)
                {
                    int currentFrom = fromIndex + i;
                    int currentTo = toIndex + i;
                    mPositionsArray[currentTo] = mPositionsArray[currentFrom];
                    mUVArray[currentTo] = mUVArray[currentFrom];
                    mColorArray[currentTo] = mColorArray[currentFrom];
                    mTangentArray[currentTo] = mTangentArray[currentFrom];
                }
            }
            else
            {
                for (int i = 0; i < itemCount; i++)
                {
                    int currentFrom = fromIndex + i;
                    int currentTo = toIndex + i;
                    mPositionsArray[currentTo] = mPositionsArray[currentFrom];
                    mUVArray[currentTo] = mUVArray[currentFrom];
                    mColorArray[currentTo] = mColorArray[currentFrom]; 
                    mTangentArray[currentTo] = mTangentArray[currentFrom];
                }
            }
        }
        protected void CopyFromToTangentOnly(int fromIndex, int toIndex, int itemCount)
        {
            ChartIntegrity.Assert(mHasTangent);
            ChartIntegrity.Assert(!mHasColor);
            if (fromIndex < toIndex)
            {
                for (int i = itemCount - 1; i >= 0; i--)
                {
                    int currentFrom = fromIndex + i;
                    int currentTo = toIndex + i;
                    mPositionsArray[currentTo] = mPositionsArray[currentFrom];
                    mUVArray[currentTo] = mUVArray[currentFrom];
                    mTangentArray[currentTo] = mTangentArray[currentFrom];
                }
            }
            else
            {
                for (int i = 0; i < itemCount; i++)
                {
                    int currentFrom = fromIndex + i;
                    int currentTo = toIndex + i;
                    mPositionsArray[currentTo] = mPositionsArray[currentFrom];
                    mUVArray[currentTo] = mUVArray[currentFrom];
                    mTangentArray[currentTo] = mTangentArray[currentFrom];
                }
            }
        }
        protected void CopyFromToColorOnly(int fromIndex, int toIndex, int itemCount)
        {
            ChartIntegrity.Assert(!mHasTangent);
            ChartIntegrity.Assert(mHasColor);
            if (fromIndex < toIndex)
            {
                for (int i = itemCount - 1; i >= 0; i--)
                {
                    int currentFrom = fromIndex + i;
                    int currentTo = toIndex + i;
                    mPositionsArray[currentTo] = mPositionsArray[currentFrom];
                    mUVArray[currentTo] = mUVArray[currentFrom];
                    mColorArray[currentTo] = mColorArray[currentFrom];
                }
            }
            else
            {
                for (int i = 0; i < itemCount; i++)
                {
                    int currentFrom = fromIndex + i;
                    int currentTo = toIndex + i;
                    mPositionsArray[currentTo] = mPositionsArray[currentFrom];
                    mUVArray[currentTo] = mUVArray[currentFrom];
                    mColorArray[currentTo] = mColorArray[currentFrom];
                }
            }
        }

        protected void CopyFromToMinimal(int fromIndex, int toIndex, int itemCount)
        {
            ChartIntegrity.Assert(!mHasTangent);
            ChartIntegrity.Assert(!mHasColor);
            if (fromIndex < toIndex)
            {
                for (int i = itemCount - 1; i >= 0; i--)
                {
                    int currentFrom = fromIndex + i;
                    int currentTo = toIndex + i;
                    mPositionsArray[currentTo] = mPositionsArray[currentFrom];
                    mUVArray[currentTo] = mUVArray[currentFrom];
                }
            }
            else
            {
                for (int i = 0; i < itemCount; i++)
                {
                    int currentFrom = fromIndex + i;
                    int currentTo = toIndex + i;
                    mPositionsArray[currentTo] = mPositionsArray[currentFrom];
                    mUVArray[currentTo] = mUVArray[currentFrom];
                }
            }
        }
        #endregion

        #region RemoveVariantss
        protected void RemoveAll(int count)
        {
            ChartIntegrity.Assert(mHasTangent);
            ChartIntegrity.Assert(mHasColor);
            //    ChartCommon.DevLog("objectId", gameObject.GetInstanceID());
            //    ChartCommon.DevLog("remove", count);
            int last = mPositions.Count - count;
            for (int i = mPositions.Count - 1; i >= last; i--) 
            {
                mPositions.RemoveAt(i);
                mUvs.RemoveAt(i);
                mColors.RemoveAt(i);
                mTangents.RemoveAt(i);
            }
            ChartIntegrity.Assert(mTangents.Count == mPositions.Count && mPositions.Count == mUvs.Count && mColors.Count == mPositions.Count);
        }

        protected void RemoveTangentOnly(int count)
        {
            ChartIntegrity.Assert(mHasTangent);
            ChartIntegrity.Assert(!mHasColor);
            //    ChartCommon.DevLog("objectId", gameObject.GetInstanceID());
            //    ChartCommon.DevLog("remove", count);
            ChartIntegrity.Assert(mPositions.Count >= count);
            ChartIntegrity.Assert(mTangents.Count == mPositions.Count && mPositions.Count == mUvs.Count,"tangent count :" + mTangents.Count + " count: " + count + " positions count: " +mPositions.Count);
            int last = mPositions.Count - count;
            for (int i = mPositions.Count - 1; i >= last; i--)
            {
                mPositions.RemoveAt(i);
                mUvs.RemoveAt(i);
                mTangents.RemoveAt(i);
            }
            ChartIntegrity.Assert(mTangents.Count == mPositions.Count && mPositions.Count == mUvs.Count);
        }

        protected void RemoveColorOnly(int count)
        {
            ChartIntegrity.Assert(!mHasTangent);
            ChartIntegrity.Assert(mHasColor);
            //    ChartCommon.DevLog("objectId", gameObject.GetInstanceID());
            //    ChartCommon.DevLog("remove", count);
            int last = mPositions.Count - count;
            for (int i = mPositions.Count - 1; i >= last; i--)
            {
                mPositions.RemoveAt(i);
                mUvs.RemoveAt(i);
                mColors.RemoveAt(i);
            }
            ChartIntegrity.Assert( mPositions.Count == mUvs.Count && mColors.Count == mPositions.Count);
        }

        protected void RemoveMinimal(int count)
        {
            //    ChartCommon.DevLog("objectId", gameObject.GetInstanceID());
            //    ChartCommon.DevLog("remove", count);
            ChartIntegrity.Assert(!mHasTangent);
            ChartIntegrity.Assert(!mHasColor);
            int last = mPositions.Count - count;
            for (int i = mPositions.Count - 1; i >= last; i--)
            {
                mPositions.RemoveAt(i);
                mUvs.RemoveAt(i);
            }
            ChartIntegrity.Assert(mPositions.Count == mUvs.Count);
        }

        #endregion

        void AddTriangle(int a, int b, int c)
        {
            mIndices.Add(a);
            mIndices.Add(b);
            mIndices.Add(c);
        }

        public void Clear()
        {
            mPositions.Clear();
            mUvs.Clear();
            if(mHasColor)
                mColors.Clear();
            if(mHasTangent)
                mTangents.Clear();
            mIndices.Clear();
            mLastVertexCount = 0;
        }
        void AddTringleStrip()
        {
            int count = mPositions.Count;
            if (count < 4)
            {
                mIndices.Clear();
                mLastVertexCount = 0;
                return;
            }

            int diff = mLastVertexCount - count;
            if (diff < 0) // we need to add indices
            {
                if (mLastVertexCount < 4)
                {
                    AddTriangle(0, 1, 2);
                    AddTriangle(2, 3, 0);
                }
                count -= 2;
                int start = Math.Max(2, mLastVertexCount - 2);
                for (int i = start; i < count; i += 2)
                {
                    AddTriangle(i + 1, i, i + 2);
                    AddTriangle(i + 2, i + 3, i + 1);
                }
            }
            else if (diff > 0)
            {
                int totalindex = (count - 2) * 3;
                mIndices.RemoveRange(totalindex, mIndices.Count - totalindex);

            }
            
            mLastVertexCount = mPositions.Count;
        }

        void AddQuadTringles()
        {
            int count = mPositions.Count;
            if (mLastVertexCount < count)
            {
                for (int i = mLastVertexCount; i < count; i += 4)
                {
                    AddTriangle(i, i + 1, i + 2);
                    AddTriangle(i + 2, i + 3, i);
                }
            }
            else
            {
                int totalindex = (count / 4) * 6;
                mIndices.RemoveRange(totalindex, mIndices.Count - totalindex);
            }
            ChartIntegrity.Assert((mPositions.Count / 4) * 6 == mIndices.Count);
            mLastVertexCount = mPositions.Count;
        }

        public bool TraingleStrip
        {
            get
            {
                return mTriangleStrip;
            }
            set
            {
                mTriangleStrip = value;
                mIndices.Clear();
                mLastVertexCount = 0;
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
                EnsureOptionalArrays();
                SelectMethods();
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
                EnsureOptionalArrays();
                SelectMethods();
            }
        }

    }
}
