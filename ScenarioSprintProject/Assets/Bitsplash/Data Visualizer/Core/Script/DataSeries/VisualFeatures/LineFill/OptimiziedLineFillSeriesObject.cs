using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    public class OptimiziedLineFillSeriesObject : LineFillSeriesObject
    {
        public override void WriteItemVertices(int itemIndex, int position, DataToArrayAdapter arrays)
        {
            DoubleVector3 from = arrays.RawPositionArray.Get(mMyIndex);
            DoubleVector3 to = arrays.RawPositionArray.Get(mMyIndex + 1);

            float mappedFromX = (float)(from.x * arrays.mMultX + arrays.mAddX);
            float mappedToX = (float)(to.x * arrays.mMultX + arrays.mAddX);
            float mappedFromY = (float)(from.y * arrays.mMultY + arrays.mAddY);
            float mappedToY = (float)(to.y * arrays.mMultY + arrays.mAddY);
            float mappedBottomPosition = (float)(arrays.mArgument1 * arrays.mMultY + arrays.mAddY);

            arrays.mPositionsArray[position] = new Vector3()
            {
                x = mappedFromX,
                y = mappedFromY,
                z = 0f
            };

            arrays.mUVArray[position] = new Vector2()
            {
                x = 0f,
                y = 0f,
            };

            position++;

            arrays.mPositionsArray[position] = new Vector3()
            {
                x = mappedFromX,
                y = mappedBottomPosition,
                z = 0f
            };

            arrays.mUVArray[position] = new Vector2()
            {
                x = 0f,
                y = 1f,
            };

            //     arrays.mColorArray[position] = ChartCommon.White;

            position++;

            arrays.mPositionsArray[position] = new Vector3()
            {
                x = mappedToX,
                y = mappedBottomPosition,
                z = 0f
            };

            arrays.mUVArray[position] = new Vector2()
            {
                x = 1f,
                y = 1f,
            };

            position++;

            arrays.mPositionsArray[position] = new Vector3()
            {
                x = mappedToX,
                y = mappedToY,
                z = 0f
            };

            arrays.mUVArray[position] = new Vector2()
            {
                x = 1f,
                y = 0f,
            };
        }
    }
}
