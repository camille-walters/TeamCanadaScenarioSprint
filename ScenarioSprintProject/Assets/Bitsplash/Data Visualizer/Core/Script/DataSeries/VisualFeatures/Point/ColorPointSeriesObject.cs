using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    class ColorPointSeriesObject : PointSeriesObject
    {
        public override void WriteItemVertices(int itemIndex, int position, DataToArrayAdapter arrays)
        {
            // var settings = (RectCanvasGraphicSettings)arrays.mSettingsObject;
            DoubleVector3 center = arrays.RawPositionArray.Get(mMyIndex);
            Color32 color = arrays.RawColorArray.Get(mMyIndex);
            Vector3 positionMapped = new Vector3()
            {
                x = (float)(center.x * arrays.mMultX + arrays.mAddX),
                y = (float)(center.y * arrays.mMultY + arrays.mAddY),
                z = 0f
            };

            arrays.mPositionsArray[position] = positionMapped;
            arrays.mTangentArray[position] = mTangent1;
            arrays.mUVArray[position] = mUV1;
            arrays.mColorArray[position] = color;
            ++position;
            arrays.mPositionsArray[position] = positionMapped;
            arrays.mTangentArray[position] = mTangent2;
            arrays.mUVArray[position] = mUV2;
            arrays.mColorArray[position] = color;
            ++position;
            arrays.mPositionsArray[position] = positionMapped;
            arrays.mTangentArray[position] = mTangent3;
            arrays.mUVArray[position] = mUV3;
            arrays.mColorArray[position] = color;
            ++position;
            arrays.mPositionsArray[position] = positionMapped;
            arrays.mTangentArray[position] = mTangent4;
            arrays.mUVArray[position] = mUV4;
            arrays.mColorArray[position] = color;
        }
    }
}
