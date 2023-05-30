using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    /// <summary>
    /// this is a helper class that conatins all refrences required for taking a data object and turning it into vertices. 
    /// this contains all the data in flat struct style in order to minimize access time from the VERY perfromance critical drawing sections
    /// using this class you can access any relevant drawing data by calling  : (adapter.*field*)
    /// </summary>
    public class DataToArrayAdapter : StackDataViewer
    {
        /// <summary>
        /// the data series that is associated with this drawing operation
        /// </summary>
        public DataSeriesBase mMapper;

        /// <summary>
        /// mapping of chart cordinates into world/transform coordinates
        /// </summary>
        public double mMultX, mMultY, mMultZ;
        /// <summary>
        /// mapping of chart cordinates into world/transform coordinates
        /// </summary>
        public double mAddX, mAddY, mAddZ;

        /// <summary>
        /// mapping of the uv coordinates , this is used for LineFill data series  and similar data series
        /// </summary>
        public double mUvMultX, mUvMultY, mUvMultZ;
        /// <summary>
        /// mapping of the uv coordinates , this is used for LineFill data series and similar data series
        /// </summary>
        public double mUvAddX, mUvAddY, mUvAddZ;

        /// <summary>
        /// these are double arguments that are specific to each data series , their use depends on the data series implementation
        /// </summary>
        public double mArgument1, mArgument2, mArgument3;
        /// <summary>
        /// the view setting object for the data series , it's type and use depend on the implementation of the data series.
        /// </summary>
        public object mSettingsObject;
        /// <summary>
        /// the output raw vertex position array. raw arrays are use to minimize access time
        /// </summary>
        public Vector3[] mPositionsArray;
        /// <summary>
        /// the output raw vertex uv array. raw arrays are use to minimize access time
        /// </summary>
        public Vector2[] mUVArray;
        /// <summary>
        /// the output raw vertex tangent array. raw arrays are use to minimize access time
        /// </summary>
        public Vector4[] mTangentArray;
        /// <summary>
        /// the output raw vertex color array. raw arrays are use to minimize access time
        /// </summary>
        public Color32[] mColorArray;


        /// <summary>
        /// this array depends on the settings of the dataseriesmapper. It is the data source for the series
        /// </summary>
        public IDataArray<DoubleVector3> FromArray;
        /// <summary>
        /// this array depends on the settings of the dataseriesmapper. It is 
        /// </summary>
        public IDataArray<DoubleVector3> ToArray;

        /// <summary>
        /// this is the offset into ToArray relative to the current point
        /// </summary>
        public int fromOffset = 1;


        public delegate void MapVertexOptimizedFixedTangentDelegate(int position, ref PreMappedVertex v);
        public MapVertexOptimizedFixedTangentDelegate MapVertexOptimizedFixedTangent;

        public DataToArrayAdapter()
        {

        }


        IDataArray<DoubleVector3> SelectArray(VectorDataSource array)
        {
            switch(array)
            {
                case VectorDataSource.Positions:
                    return RawPositionArray;
                case VectorDataSource.EndPositions:
                    return RawEndPositionArray;
                default:
                    break;

            }
            return RawPositionArray;
        }

        /// <summary>
        /// once this method is called. you should not modify either the data and the graph arrays size until all vertex writing is done. 
        /// this method is normally called at the start of a graphic generation procdure
        /// </summary>
        /// <param name="mapper"></param>
        /// <param name="reciver"></param>
        public void PopulateFrom(DataSeriesBase mapper,IVertexReciver reciver)
        {
            mMapper = mapper;
            mArgument1 = mMapper.GetDoubleArgument(0);  // assign the arguments
            mArgument2 = mMapper.GetDoubleArgument(1);
            mArgument3 = mMapper.GetDoubleArgument(2);
            var arrays = reciver.GetArrays();   // assign the arrays
            mPositionsArray = arrays.mPositionsArray;
            mUVArray = arrays.mUVArray;
            mTangentArray = arrays.mTangentArray;
            mColorArray = arrays.mColorArray;
            mSettingsObject = mMapper.GraphicSettingsObject;
            CopyFrom(mapper.RawData);   //copy the refrences to the underlying data arrays
            PrepareMapping(); // copy the mapping values from the mapper
            if (mColorArray == null)
                MapVertexOptimizedFixedTangent = MapVertexOptimizedFixedTangentNoColor;
            else
                MapVertexOptimizedFixedTangent = MapVertexOptimizedFixedTangentColor;

            FromArray = SelectArray(mapper.FromArray);
            ToArray = SelectArray(mapper.ToArray);
            fromOffset = mapper.ToOffset;

        }
        
        /// <summary>
        /// copies the mapping values from the mapper into this object
        /// </summary>
        void PrepareMapping()
        {
            mMultX = mMapper.Mapping.Mult.x;
            mMultY = mMapper.Mapping.Mult.y;
            mMultZ = mMapper.Mapping.Mult.z;

            mAddX = mMapper.Mapping.Add.x;
            mAddY = mMapper.Mapping.Add.y;
            mAddZ = mMapper.Mapping.Add.z;

            var uvMapping = mMapper.UVMappingFunction();
            mUvMultX = uvMapping.Mult.x;
            mUvMultY = uvMapping.Mult.y;
            mUvMultZ = uvMapping.Mult.z;

            mUvAddX = uvMapping.Add.x;
            mUvAddY = uvMapping.Add.y;
            mUvAddZ = uvMapping.Add.z;

        }

        /// <summary>
        /// helper method that maps a position from chart space into world/transform space with zero z value
        /// </summary>
        /// <param name="position">used with ref in order to make calls faster</param>
        /// <returns></returns>
        public Vector3 MapPositionWithZeroZ(ref DoubleVector3 position)
        {
            return new Vector3()
            {
                x = (float)(position.x * mMultX + mAddX),
                y = (float)(position.y * mMultY + mAddY),
                z = 0f
            };
        }

        /// <summary>
        /// helper method that maps an index position from chart space into world/transform space
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Vector3 MapPositionWithZeroZ(int index)
        {
            DoubleVector3 position = RawPositionArray.Get(index);
            return new Vector3()
            {
                x = (float)(position.x * mMultX + mAddX),
                y = (float)(position.y * mMultY + mAddY),
                z = 0f
            };
        }

        public void MapVertexOptimizedFixedTangentColor(int position,ref PreMappedVertex v)
        {
            Vector4 tmp = v.tangent;
            tmp.x *= v.ExtrusionFactor;
            tmp.y *= v.ExtrusionFactor;
            tmp.z = v.ExtrusionFactor;
            tmp.w = v.ExtrusionAngleInterpolator;

            mPositionsArray[position] = new Vector3()
            {
                x = (float)(v.preMapped.x * mMultX + mAddX), //+ v.tangent.x * 0.0001f,
                y = (float)(v.preMapped.y * mMultY + mAddY), //+ v.tangent.y * 0.0001f,
                z = 0f
            };

            mUVArray[position] = v.uv; 
            mTangentArray[position] = tmp;
            mColorArray[position] = v.color;
        }

        public void MapVertexOptimizedFixedTangentNoColor(int position, ref PreMappedVertex v)
        {
            Vector4 tmp = v.tangent;
            tmp.x *= v.ExtrusionFactor;
            tmp.y *= v.ExtrusionFactor;
            tmp.z = v.ExtrusionFactor;
            tmp.w = v.ExtrusionAngleInterpolator;

            mPositionsArray[position] = new Vector3()
            {
                x = (float)(v.preMapped.x * mMultX + mAddX), //+ v.tangent.x * 0.0001f,
                y = (float)(v.preMapped.y * mMultY + mAddY), //+ v.tangent.y * 0.0001f,
                z = 0f
            };
            mUVArray[position] = v.uv;
            mTangentArray[position] = tmp;
        }

        public void MapEmptyVertex(int position, DataToArrayAdapter arrays)
        {
            arrays.mTangentArray[position] = new Vector4();
            arrays.mPositionsArray[position] = new Vector3();
            arrays.mUVArray[position] = new Vector2();
        }

        public void MapVertexFixedWhite(ref PreMappedVertexSlim v, int position)
        {
            mTangentArray[position] = new Vector4()
            {
                x = (float)(v.tangent.x * mMultX),
                y = (float)(v.tangent.y * mMultY),
                z = 1f,
                w = 1f
            };

            mPositionsArray[position] = new Vector3()
            {
                x = (float)(v.preMapped.x * mMultX + mAddX),
                y = (float)(v.preMapped.y * mMultY + mAddY),
                z = 0f
            };

            mUVArray[position] = v.uv;
            mColorArray[position] = ChartCommon.White;
        }

        public void MapVertexFixed(ref PreMappedVertexSlim v, int position)
        {
            mTangentArray[position] = new Vector4()
            {
                x = (float)(v.tangent.x * mMultX),
                y = (float)(v.tangent.y * mMultY),
                z = 1f,
                w = 1f
            };

            mPositionsArray[position] = new Vector3()
            {
                x = (float)(v.preMapped.x * mMultX + mAddX),
                y = (float)(v.preMapped.y * mMultY + mAddY),
                z = 0f
            };

            mUVArray[position] = v.uv;
            mColorArray[position] = v.color;
        }

        /// <summary>
        /// this method is like MapVertex method
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public void MapVertex(ref PreMappedVertex v, int position)
        {
            mTangentArray[position] = new Vector4()
            {
                x = (float)(v.tangent.x * mMultX) * v.ExtrusionFactor,
                y = (float)(v.tangent.y * mMultY) * v.ExtrusionFactor,
                z = v.ExtrusionFactor,
                w = v.ExtrusionAngleInterpolator
            };

            mPositionsArray[position] = new Vector3()
            {
                x = (float)(v.preMapped.x * mMultX + mAddX),
                y = (float)(v.preMapped.y * mMultY + mAddY),
                z = (float)v.preMapped.z
            };

            mUVArray[position] = v.uv;
            mColorArray[position] = v.color;
        }

    }
}
