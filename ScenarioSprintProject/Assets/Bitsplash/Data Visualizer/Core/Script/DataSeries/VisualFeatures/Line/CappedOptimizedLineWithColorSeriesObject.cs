using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataVisualizer{
    public class CappedOptimizedLineWithColorSeriesObject : LineSeriesObject
    {
        static Vector2 mUV1 = new Vector2(0f, 0f), mUV2 = new Vector2(0f, 1f), mUV3 = new Vector2(1f, 1f), mUV4 = new Vector2(1f, 0f);
        public static int ConstItemSize { get { return 4; } }

        public override int ItemSize { get { return ConstItemSize; } }

        //   static PreMappedVertexSlim v = new PreMappedVertexSlim();

        public override void WriteItemVertices(int itemIndex, int position, DataToArrayAdapter arrays)
        {
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
            arrays.mPositionsArray[position] = fromMapped;
            arrays.mTangentArray[position] = tangent;
            arrays.mUVArray[position] = mUV1;
            arrays.mColorArray[position] = colorFrom;

            position += 3;

            arrays.mPositionsArray[position] = toMapped;
            arrays.mTangentArray[position] = tangent;
            arrays.mUVArray[position] = mUV4;
            arrays.mColorArray[position] = colorTo;
            position--;


            arrays.mPositionsArray[position] = toMapped;
            arrays.mTangentArray[position] = tangent2;
            arrays.mUVArray[position] = mUV3;
            arrays.mColorArray[position] = colorTo;
            position--;

            arrays.mPositionsArray[position] = fromMapped;
            arrays.mTangentArray[position] = tangent2;
            arrays.mUVArray[position] = mUV2;
            arrays.mColorArray[position] = colorFrom;

            position += 3;

            // Line Cap
            tangent.z = 1.2f;
            tangent.w = 0.4f;

            arrays.mPositionsArray[position] = fromMapped;
            arrays.mTangentArray[position] = tangent;
            arrays.mUVArray[position] = mUV1;
            arrays.mColorArray[position] = colorFrom;

            ++position;

            tangent.z = 1.2f;
            tangent.w = -0.4f;
            arrays.mPositionsArray[position] = fromMapped;
            arrays.mTangentArray[position] = tangent;
            arrays.mUVArray[position] = mUV2;
            arrays.mColorArray[position] = colorFrom;

            ++position;

            tangent2.z = 1f;
            tangent2.w = 1f;

            arrays.mPositionsArray[position] = fromMapped;
            arrays.mTangentArray[position] = tangent2;
            arrays.mUVArray[position] = mUV1;
            arrays.mColorArray[position] = colorFrom;
            ++position;

            tangent.z = 1f;
            tangent.w = 1f;
            arrays.mPositionsArray[position] = fromMapped;
            arrays.mTangentArray[position] = tangent;
            arrays.mUVArray[position] = mUV2;
            arrays.mColorArray[position] = colorFrom;

        }
    }
}