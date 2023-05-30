using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThetaList;
using UnityEngine;

namespace DataVisualizer{
    public abstract class LineSeriesObject : TwoPointSeriesObject
    {

        public class LineCanvasGraphSettings
        {
            public double mThickness = 0.0;
            public bool mLineCap = true;
            public bool mScaleableLine = false;
        }

        public override bool Is3D { get { return false; } }

        public LineSeriesObject()
        {
        }

        public override double SqaureDist(DataSeriesBase mapper, DoubleVector3 mouse)
        {
            DoubleVector3? from = null;
            DoubleVector3? to = null;
            AssignFromTo(mapper,ref from, ref to);
            if (from.HasValue == false || to.HasValue == false)
                return double.MaxValue;
            return ChartCommon.SegmentPointSqrDistance(from.Value, to.Value, mouse);
        }

        public override void WriteItemVertices(int itemIndex, int position, DataToArrayAdapter arrays)
        {
            //  DataSeriesBase mapper = Mapper;
            DoubleVector3 from = arrays.FromArray.Get(mMyIndex);
            DoubleVector3 to = arrays.ToArray.Get(mMyIndex + arrays.fromOffset); // this mehtod is called only if there is a next value

            Vector3 fromMapped = new Vector3()
            {
                x = (float)(from.x * arrays.mMultX + arrays.mAddX),
                y = (float)(from.y * arrays.mMultY + arrays.mAddY),
                z = 0f
            };

            Vector3 toMapped = new Vector3()
            {
                x = (float)(to.x * arrays.mMultX + arrays.mAddX),
                y = (float)(to.y * arrays.mMultY + arrays.mAddY),
                z = 0f
            };
 
            Vector4 tangent = new Vector4()
            {
                x = (float)((from.x - to.x) * arrays.mMultX),
                y = (float)((from.y - to.y) * arrays.mMultY),
                z = 1f,
                w = 1f
            };

            Vector4 tangent2 = new Vector4()
            {
                x = -tangent.x,
                y = -tangent.y,
                z = 1f,
                w = 1f
            };

            Rect uvRect = arrays.mMapper.GetUvRect(MyIndex, this);

            float minx = uvRect.xMin;
            float miny = uvRect.yMin;
            float maxx = uvRect.xMax;
            float maxy = uvRect.yMax;

            arrays.mPositionsArray[position] = fromMapped;
            arrays.mTangentArray[position] = tangent;
            arrays.mUVArray[position] = new Vector2()
            {
                x = minx,
                y = miny,
            };

            position +=3;

            arrays.mPositionsArray[position] = toMapped;
            arrays.mTangentArray[position] = tangent;
            arrays.mUVArray[position] = new Vector2()
            {
                x = maxx,
                y = miny,
            };

            position--;

            arrays.mPositionsArray[position] = toMapped;
            arrays.mTangentArray[position] = tangent2;
            arrays.mUVArray[position] = new Vector2()
            {
                x = maxx,
                y = maxy,
            };

            position--;

            arrays.mPositionsArray[position] = fromMapped;
            arrays.mTangentArray[position] = tangent2;
            arrays.mUVArray[position] = new Vector2()
            {
                x = minx,
                y = maxy,
            };

            position += 3;

            // Line Cap
            tangent.z = 1.2f;
            tangent.w = 0.4f;

            arrays.mPositionsArray[position] = fromMapped;
            arrays.mTangentArray[position] = tangent;
            arrays.mUVArray[position] = new Vector2()
            {
                x = minx,
                y = miny,
            };

            ++position;

            tangent.z = 1.2f;
            tangent.w = -0.4f;
            arrays.mPositionsArray[position] = fromMapped;
            arrays.mTangentArray[position] = tangent;
            arrays.mUVArray[position] = new Vector2()
            {
                x = minx,
                y = maxy,
            };

            ++position;

            tangent2.z = 1f;
            tangent2.w = 1f;

            arrays.mPositionsArray[position] = fromMapped;
            arrays.mTangentArray[position] = tangent2;
            arrays.mUVArray[position] = new Vector2()
            {
                x = minx,
                y = miny,
            };
            ++position;

            tangent.z = 1f;
            tangent.w = 1f;
            arrays.mPositionsArray[position] = fromMapped;
            arrays.mTangentArray[position] = tangent;
            arrays.mUVArray[position] = new Vector2()
            {
                x = minx,
                y = maxy,
            };

            //Mapper.MapVertexFixed(ref v, reciver);
            //v.uv.y = maxy;
            //v.tangent.x = -tan1x;
            //v.tangent.y = -tan1y;
            //Mapper.MapVertexFixed(ref v, reciver);
            //v.uv.x = maxx;
            //v.preMapped = to;
            //v.preMapped.z = 0.0;
            //Mapper.MapVertexFixed(ref v, reciver);
            //v.uv.y = miny;
            //v.tangent.x = tan1x;
            //v.tangent.y = tan1y;
            //Mapper.MapVertexFixed(ref v, reciver);
        }

        public override int ItemCount { get { return 1; } }

        protected override DoubleRect? CalcBoundingBox(DataSeriesBase mapper)
        {
            DoubleVector3? from = null;
            DoubleVector3? to = null;
            AssignFromTo(mapper,ref from, ref to);
            if (from.HasValue == false || to.HasValue == false) // no next point so there's no line
                return null;
            return DoubleRect.FromTwoPoints(from.Value.ToDoubleVector2(), to.Value.ToDoubleVector2());
        }

        public override void DiscardData()
        {
           
        }
    }
}
