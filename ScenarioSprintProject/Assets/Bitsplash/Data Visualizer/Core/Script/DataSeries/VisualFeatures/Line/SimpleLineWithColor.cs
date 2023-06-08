using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    class SimpleLineWithColor : LineSeriesObject
    {
        public static int ConstItemSize { get { return 4; } }

        public override int ItemSize { get { return ConstItemSize; } }
        public override void WriteItemVertices(int itemIndex, int position, DataToArrayAdapter arrays)
        {
            //  DataSeriesBase mapper = Mapper;
            DoubleVector3 from = arrays.FromArray.Get(mMyIndex);
            DoubleVector3 to = arrays.ToArray.Get(mMyIndex + arrays.fromOffset); // this mehtod is called only if there is a next value

            Color32 colorFrom = arrays.RawColorArray.Get(mMyIndex);
            Color32 colorTo = arrays.RawColorArray.Get(mMyIndex + arrays.fromOffset);

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

            arrays.mColorArray[position] = colorFrom;

            position += 3;

            arrays.mPositionsArray[position] = toMapped;
            arrays.mTangentArray[position] = tangent;
            arrays.mUVArray[position] = new Vector2()
            {
                x = maxx,
                y = miny,
            };
            arrays.mColorArray[position] = colorTo;

            position--;


            arrays.mPositionsArray[position] = toMapped;
            arrays.mTangentArray[position] = tangent2;
            arrays.mUVArray[position] = new Vector2()
            {
                x = maxx,
                y = maxy,
            };
            arrays.mColorArray[position] = colorTo;

            position--;

            arrays.mPositionsArray[position] = fromMapped;
            arrays.mTangentArray[position] = tangent2;
            arrays.mUVArray[position] = new Vector2()
            {
                x = minx,
                y = maxy,
            };
            arrays.mColorArray[position] = colorFrom;

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
            arrays.mColorArray[position] = colorFrom;
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
            arrays.mColorArray[position] = colorFrom;
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
            arrays.mColorArray[position] = colorFrom;
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
            arrays.mColorArray[position] = colorFrom;

        }
    }
}
