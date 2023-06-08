using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThetaList;
using UnityEngine;

namespace DataVisualizer{
    class PointSeriesObject : FixedSeriesObject
    {
        protected static Vector2 mUV1 = new Vector2(0f, 0f), mUV2 = new Vector2(0f, 1f), mUV3 = new Vector2(1f, 1f), mUV4 = new Vector2(1f, 0f);
        protected static Vector4 mTangent1 = new Vector4(0f, 1f, 1f, 0.5f), mTangent2 = new Vector4(-1f, 0f, 1f, 0.5f), mTangent3 = new Vector4(0f, -1f, 1f, 0.5f), mTangent4 = new Vector4(1f, 0f, 1f,0.5f);

        public class RectCanvasGraphicSettings
        {
            public bool mScalable = false;
            public double mSize = 0.0;
            public double mHalfSize = 0.0;
        }
        public override DoubleVector3 Center(DataSeriesBase mapper)
        {
            if (mapper.RawData.RawPositionArray == null)
                return new DoubleVector3();
            return mapper.RawData.RawPositionArray.Get(MyIndex);
        }


        protected override bool MapTangents
        {
            get { return false; }
        }

        public override bool Is3D { get { return false; } }

        /// <summary>
        /// returns a rect for a point at the graph, If the size value is defined then the point will have that size. Otherwise the size of the point is the -param- argument
        /// </summary>
        /// <param name="val"></param>
        /// <param name="param"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        static public bool Point(StackDataViewer viewer, int index, double param, out DoubleRect rect)
        {
            rect = new DoubleRect();
            DoubleVector3 pos = viewer.RawPositionArray.Get(index);
            double size = param;
            if (viewer.RawSizeArray.IsNull == false)
                size = viewer.RawSizeArray.Get(index);
            double half = size * 0.5;
            double left = pos.x - half;
            double top = pos.y - half;
            //  ChartCommon.DevLog("position y", pos.y);
            rect = new DoubleRect(left, top, size, size);
            return true;
        }

        /// <summary>
        /// returns the rect for the high part of a candle based of the data provided
        /// </summary>
        /// <param name="val"></param>
        /// <param name="param"the param is used as line thickness></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        static public bool CandleLow(StackDataViewer viewer, int index, double param, out DoubleRect rect)
        {
            rect = new DoubleRect();
            DoubleVector3 pos = viewer.RawPositionArray.Get(index);
            double min = viewer.RawStartEndArray.Get(index).Min;
            double low = viewer.RawHighLowArray.Get(index).Min;
            param *= 0.5;
            double left = pos.x - param * 0.5f;
            rect = new DoubleRect(left, low, param, min - low);
            return true;
        }

        /// <summary>
        /// returns the rect for the high part of a candle based of the data provided
        /// </summary>
        /// <param name="val"></param>
        /// <param name="param"the param is used as line thickness></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        static public bool CandleHigh(StackDataViewer viewer, int index, double param, out DoubleRect rect)
        {
            rect = new DoubleRect();
            DoubleVector3 pos = viewer.RawPositionArray.Get(index);
            double max = viewer.RawStartEndArray.Get(index).Max;
            double high = viewer.RawHighLowArray.Get(index).Max;
            param *= 0.5;
            double left = pos.x - param * 0.5f;
            rect = new DoubleRect(left, max, param, high - max);
            return true;
        }

        /// <summary>
        /// returns the rect for the body part of a candle based of the data provided
        /// </summary>
        /// <param name="val"></param>
        /// <param name="param"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        static public bool CandleBody(StackDataViewer viewer, int index, double param, out DoubleRect rect)
        {
            rect = new DoubleRect();
            DoubleVector3 pos = viewer.RawPositionArray.Get(index);
            DoubleRange startEnd = viewer.RawStartEndArray.Get(index);
            param *= 0.5;
            double left = pos.x - param * 0.5f;
            rect = new DoubleRect(left, startEnd.Min, param, startEnd.Size);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paramsFunc"></param>
        /// <param name="param">arbitary param , it's meaning is dicatated by the paramsFunc</param>
        /// <param name="type"></param>
        /// <param name="userData"></param>
        public PointSeriesObject()
        {
        }

        public override double SqaureDist(DataSeriesBase mapper, DoubleVector3 mouse)
        {
            var rect = CalcBoundingBox(mapper);
            if (rect.HasValue == false)
                return double.MaxValue;
            if (rect.Value.Contains(mouse))
                return 0.0;
            DoubleVector3 max = rect.Value.Max;
            DoubleVector3 min = rect.Value.Min;
            DoubleVector3 minXMaxY = new DoubleVector3(min.x, max.y);
            DoubleVector3 maxXMinY = new DoubleVector3(max.x, min.y);
            double dist = ChartCommon.SegmentPointSqrDistance(min, minXMaxY, mouse);
            dist = Math.Min(dist, ChartCommon.SegmentPointSqrDistance(min, maxXMinY, mouse));
            dist = Math.Min(dist, ChartCommon.SegmentPointSqrDistance(max, minXMaxY, mouse));
            dist = Math.Min(dist, ChartCommon.SegmentPointSqrDistance(max, maxXMinY, mouse));
            return dist;
        }

        public override void WriteItemVertices(int itemIndex, int position, DataToArrayAdapter arrays)
        {
           // var settings = (RectCanvasGraphicSettings)arrays.mSettingsObject;
            DoubleVector3 center = arrays.RawPositionArray.Get(mMyIndex);
            Vector3 positionMapped = new Vector3()
            {
                x = (float)(center.x * arrays.mMultX + arrays.mAddX),
                y = (float)(center.y * arrays.mMultY + arrays.mAddY),
                z = 0f
            };

            arrays.mPositionsArray[position] = positionMapped;
            arrays.mTangentArray[position] = mTangent1;
            arrays.mUVArray[position] = mUV1;
            ++position;
            arrays.mPositionsArray[position] = positionMapped;
            arrays.mTangentArray[position] = mTangent2;
            arrays.mUVArray[position] = mUV2;
            ++position;
            arrays.mPositionsArray[position] = positionMapped;
            arrays.mTangentArray[position] = mTangent3;
            arrays.mUVArray[position] = mUV3;
            ++position;
            arrays.mPositionsArray[position] = positionMapped;
            arrays.mTangentArray[position] = mTangent4;
            arrays.mUVArray[position] = mUV4;

        }
        protected override DoubleRect? CalcBoundingBox(DataSeriesBase mapper)
        {
            var settings = (RectCanvasGraphicSettings)mapper.GraphicSettingsObject;
            if (mapper.RawData.RawPositionArray == null)
                return null;
            DoubleVector3 center = mapper.RawData.RawPositionArray.Get(MyIndex);
            return new DoubleRect(center.x - settings.mHalfSize, center.y - settings.mHalfSize, settings.mSize, settings.mSize);
        }


        protected override double CalculateLength(DataSeriesBase mapper)
        {
            return 1.0;
        }

        public override int ItemCount { get { return 1; } }

        public override void DiscardData()
        {
        
        }
    }
}
