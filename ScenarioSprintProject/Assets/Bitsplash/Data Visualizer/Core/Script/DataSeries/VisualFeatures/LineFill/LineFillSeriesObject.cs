using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThetaList;
using UnityEngine;

namespace DataVisualizer{
    public class LineFillSeriesObject : TwoPointSeriesObject
    {

       // LineFillGraphSettings mSettings;
       // double mCurrentBottom = 0.0;
        public delegate void UvMethod(LineFillSeriesObject parent, int myIndex, int itemIndex, int position, DataToArrayAdapter arrays);

        public class LineFillGraphSettings
        {
            /// <summary>
            /// true to use a tringle strip instead of tringle list
            /// </summary>
            //public bool TringleStrip = false;
            /// <summary>
            /// if true the top uv is streched along the line 
            /// </summary>
            public UvMethod uvMethod = DrawFromToClampedNoColor;
            /// <summary>
            /// indicates where the minimum texture edge is mapped to
            /// </summary>
            public FillUvMapping MinimumUv;
            /// <summary>
            /// indicates where the maximum texture edge is mapped to
            /// </summary>
            public FillUvMapping MaximumUv;
            /// <summary>
            /// the current bottom position of the fill (this is used internally to grow the fill objects as the chart zooms out)
            /// </summary>
            public double BottomPosition = 0.0;
            /// <summary>
            /// the mapping function used for uv
            /// </summary>
            public MappingFunction UvMappingFunction = MappingFunction.One;

        }

        public LineFillSeriesObject()
        {

        }

        public override double SqaureDist(DataSeriesBase mapper, DoubleVector3 mouse)
        {
           // if (mVertices.Count < 2)
           //     return double.MaxValue;
            return double.PositiveInfinity;// ChartCommon.PointPolySquareDist(mouse, mVertices);
        }

        public static void DrawFromToFullStretch(LineFillSeriesObject parent, int myIndex, int itemIndex, int position, DataToArrayAdapter arrays)
        {
            //Rect uvRect = Mapper.GetUvRect(myIndex, parent);
            //Vector2 minUv = uvRect.min;
            //Vector2 maxUv = uvRect.max;
            //DoubleVector3 from = Mapper.RawData.RawPositionArray[myIndex].TrimZ();
            //DoubleVector3 to = Mapper.RawData.RawPositionArray[myIndex + 1].TrimZ();

            //var settings = (LineFillGraphSettings)Settings;
            //DoubleVector3 fromBottom = new DoubleVector3(from.x, settings.BottomPosition);
            //DoubleVector3 toBottom = new DoubleVector3(to.x, settings.BottomPosition);

            //PreMappedVertex v;
            //v.uv = minUv;
            //v.tangent = Vector4.zero;
            //v.color = ChartCommon.White ;
            //v.ExtrusionAngleInterpolator = 0f;
            //v.ExtrusionFactor = 0f;
            //v.preMapped = from;
            //Mapper.MapVertexOptimizedZeroTangent(ref v, reciver);
            //v.uv = new Vector2(minUv.x, maxUv.y);
            //v.preMapped = fromBottom;
            //Mapper.MapVertexOptimizedZeroTangent(ref v, reciver);
            //v.uv = maxUv;
            //v.preMapped = toBottom;
            //Mapper.MapVertexOptimizedZeroTangent(ref v, reciver);
            //v.uv = new Vector2(maxUv.x, minUv.y);
            //v.preMapped = to;
            //Mapper.MapVertexOptimizedZeroTangent(ref v, reciver);
        }
        
        public static void DrawFromToStretch(LineFillSeriesObject parent, int myIndex, int itemIndex, int position, DataToArrayAdapter arrays)
        {
            Rect uvRect = arrays.mMapper.GetUvRect(myIndex, parent);
            Vector2 minUv = uvRect.min;
            Vector2 maxUv = uvRect.max;
            DoubleVector3 from = arrays.RawPositionArray.Get(myIndex);
            DoubleVector3 to = arrays.RawPositionArray.Get(myIndex + 1);
            Color32 colorFrom = ChartCommon.White;
            Color32 colorTo = ChartCommon.White;
            if (arrays.RawColorArray.IsNull == false)
            {
                colorFrom = arrays.RawColorArray.Get(myIndex);
                colorTo = arrays.RawColorArray.Get(myIndex + 1);
            }
            var settings = (LineFillGraphSettings)arrays.mSettingsObject;

            float mappedFromX = (float)(from.x * arrays.mMultX + arrays.mAddX);
            float mappedToX = (float)(to.x * arrays.mMultX + arrays.mAddX);
            float mappedFromY = (float)(from.y * arrays.mMultY + arrays.mAddY);
            float mappedToY = (float)(to.y * arrays.mMultY + arrays.mAddY);
            float mappedBottomPosition = (float)(settings.BottomPosition * arrays.mMultY + arrays.mAddY);

            float bottomV = (float)(settings.BottomPosition * arrays.mUvMultY + arrays.mUvAddY);

            arrays.mPositionsArray[position] = new Vector3()
            {
                x = mappedFromX,
                y = mappedFromY,
                z = 0f
            };

            arrays.mUVArray[position] = minUv;

            arrays.mColorArray[position] = colorFrom;

            position++;

            arrays.mPositionsArray[position] = new Vector3()
            {
                x = mappedFromX,
                y = mappedBottomPosition,
                z = 0f
            };

            arrays.mUVArray[position] = new Vector2()
            {
                x = minUv.x,
                y = bottomV,
            };

            arrays.mColorArray[position] = colorFrom;

            position++;

            arrays.mPositionsArray[position] = new Vector3()
            {
                x = mappedToX,
                y = mappedBottomPosition,
                z = 0f
            };

            arrays.mUVArray[position] = new Vector2()
            {
                x = maxUv.x,
                y = bottomV,
            };

            arrays.mColorArray[position] = colorTo;

            position++;

            arrays.mPositionsArray[position] = new Vector3()
            {
                x = mappedToX,
                y = mappedToY,
                z = 0f
            };

            arrays.mUVArray[position] = new Vector2()
            {
                x = maxUv.x,
                y = minUv.y,
            };

            arrays.mColorArray[position] = colorTo;
        }
        public static void DrawFromToStretchNoColor(LineFillSeriesObject parent, int myIndex, int itemIndex, int position, DataToArrayAdapter arrays)
        {
            Rect uvRect = arrays.mMapper.GetUvRect(myIndex, parent);
            Vector2 minUv = uvRect.min;
            Vector2 maxUv = uvRect.max;
            DoubleVector3 from = arrays.RawPositionArray.Get(myIndex);
            DoubleVector3 to = arrays.RawPositionArray.Get(myIndex + 1);
            var settings = (LineFillGraphSettings)arrays.mSettingsObject;

            float mappedFromX = (float)(from.x * arrays.mMultX + arrays.mAddX);
            float mappedToX = (float)(to.x * arrays.mMultX + arrays.mAddX);
            float mappedFromY = (float)(from.y * arrays.mMultY + arrays.mAddY);
            float mappedToY = (float)(to.y * arrays.mMultY + arrays.mAddY);
            float mappedBottomPosition = (float)(settings.BottomPosition * arrays.mMultY + arrays.mAddY);

            float bottomV = (float)(settings.BottomPosition * arrays.mUvMultY + arrays.mUvAddY);

            arrays.mPositionsArray[position] = new Vector3()
            {
                x = mappedFromX,
                y = mappedFromY,
                z = 0f
            };

            arrays.mUVArray[position] = minUv;

            position++;

            arrays.mPositionsArray[position] = new Vector3()
            {
                x = mappedFromX,
                y = mappedBottomPosition,
                z = 0f
            };

            arrays.mUVArray[position] = new Vector2()
            {
                x = minUv.x,
                y = bottomV,
            };

            position++;

            arrays.mPositionsArray[position] = new Vector3()
            {
                x = mappedToX,
                y = mappedBottomPosition,
                z = 0f
            };

            arrays.mUVArray[position] = new Vector2()
            {
                x = maxUv.x,
                y = bottomV,
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
                x = maxUv.x,
                y = minUv.y,
            };
        }
        public static void DrawFromToClamped(LineFillSeriesObject parent, int myIndex, int itemIndex, int position, DataToArrayAdapter arrays)
        {
            Rect uvRect = arrays.mMapper.GetUvRect(myIndex, parent);
            float minUvX = uvRect.xMin;
            float maxUvX = uvRect.xMax;
            DoubleVector3 from = arrays.RawPositionArray.Get(myIndex);
            DoubleVector3 to = arrays.RawPositionArray.Get(myIndex + 1);
            Color32 colorFrom = ChartCommon.White;
            Color32 colorTo = ChartCommon.White;
            if (arrays.RawColorArray.IsNull == false)
            {
                colorFrom = arrays.RawColorArray.Get(myIndex);
                colorTo = arrays.RawColorArray.Get(myIndex + 1);
            }
            var settings = (LineFillGraphSettings)arrays.mSettingsObject;

            float mappedFromX = (float)(from.x *arrays.mMultX + arrays.mAddX);
            float mappedToX = (float)(to.x * arrays.mMultX + arrays.mAddX);
            float mappedFromY = (float)(from.y * arrays.mMultY + arrays.mAddY);
            float mappedToY = (float)(to.y * arrays.mMultY + arrays.mAddY);
            float mappedBottomPosition = (float)(settings.BottomPosition * arrays.mMultY + arrays.mAddY);

            float fromV = (float)(from.y * arrays.mUvMultY + arrays.mUvAddY);
            float toV = (float)(to.y * arrays.mUvMultY + arrays.mUvAddY);
            float bottomV = (float)(settings.BottomPosition * arrays.mUvMultY + arrays.mUvAddY);

            arrays.mPositionsArray[position] = new Vector3()
            {
                x = mappedFromX,
                y = mappedFromY,
                z = 0f
            };

            arrays.mUVArray[position] = new Vector2()
            {
                x = minUvX,
                y = fromV,
            };

            arrays.mColorArray[position] = colorFrom;

            position++;

            arrays.mPositionsArray[position] = new Vector3()
            {
                x = mappedFromX,
                y = mappedBottomPosition,
                z = 0f
            };

            arrays.mUVArray[position] = new Vector2()
            {
                x = minUvX,
                y = bottomV,
            };

            arrays.mColorArray[position] = colorFrom;

            position++;

            arrays.mPositionsArray[position] = new Vector3()
            {
                x = mappedToX,
                y = mappedBottomPosition,
                z = 0f
            };

            arrays.mUVArray[position] = new Vector2()
            {
                x = maxUvX,
                y = bottomV,
            };

            arrays.mColorArray[position] = colorTo;

            position++;

            arrays.mPositionsArray[position] = new Vector3()
            {
                x = mappedToX,
                y = mappedToY,
                z = 0f
            };

            arrays.mUVArray[position] = new Vector2()
            {
                x = maxUvX,
                y = toV,
            };

            arrays.mColorArray[position] = colorTo;
        }
        public static void DrawFromToClampedNoColor(LineFillSeriesObject parent, int myIndex, int itemIndex, int position, DataToArrayAdapter arrays)
        {
            Rect uvRect = arrays.mMapper.GetUvRect(myIndex, parent);
            float minUvX = uvRect.xMin;
            float maxUvX = uvRect.xMax;
            DoubleVector3 from = arrays.RawPositionArray.Get(myIndex);
            DoubleVector3 to = arrays.RawPositionArray.Get(myIndex + 1);
            var settings = (LineFillGraphSettings)arrays.mSettingsObject;

            float mappedFromX = (float)(from.x * arrays.mMultX + arrays.mAddX);
            float mappedToX = (float)(to.x * arrays.mMultX + arrays.mAddX);
            float mappedFromY = (float)(from.y * arrays.mMultY + arrays.mAddY);
            float mappedToY = (float)(to.y * arrays.mMultY + arrays.mAddY);
            float mappedBottomPosition = (float)(settings.BottomPosition * arrays.mMultY + arrays.mAddY);

            float fromV = (float)(from.y * arrays.mUvMultY + arrays.mUvAddY);
            float toV = (float)(to.y * arrays.mUvMultY + arrays.mUvAddY);
            float bottomV = (float)(settings.BottomPosition * arrays.mUvMultY + arrays.mUvAddY);

            arrays.mPositionsArray[position] = new Vector3()
            {
                x = mappedFromX,
                y = mappedFromY,
                z = 0f
            };

            arrays.mUVArray[position] = new Vector2()
            {
                x = minUvX,
                y = fromV,
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
                x = minUvX,
                y = bottomV,
            };

            position++;

            arrays.mPositionsArray[position] = new Vector3()
            {
                x = mappedToX,
                y = mappedBottomPosition,
                z = 0f
            };

            arrays.mUVArray[position] = new Vector2()
            {
                x = maxUvX,
                y = bottomV,
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
                x = maxUvX,
                y = toV,
            };

        }

        public override void WriteItemVertices(int itemIndex, int position, DataToArrayAdapter arrays)
        {
            var settings = (LineFillGraphSettings)arrays.mMapper.GraphicSettingsObject;
            settings.uvMethod(this, MyIndex, itemIndex,position, arrays);
        }

        protected override bool MapTangents
        {
            get { return false; }
        }

        public override bool Is3D
        {
            get { return false; }
        }


        protected override double CalculateLength(DataSeriesBase mapper)
        {
            DoubleVector3? fromRef = null;
            DoubleVector3? toRef = null;
            AssignFromTo(mapper,ref fromRef, ref toRef);
            if (fromRef.HasValue == false || toRef.HasValue == false)
                return 0.0;
            return Math.Abs(toRef.Value.x - fromRef.Value.x);
        }

        protected override DoubleRect? CalcBoundingBox(DataSeriesBase mapper)
        {
            int next = MyIndex + 1;

            if (mapper.Count <= next) // no next point so there's no line
                return null;
            var settings = (LineFillGraphSettings)mapper.GraphicSettingsObject;
            DoubleVector2 from, to;
            StackDataViewer.IDataArray<DoubleVector3> arr = mapper.RawData.RawPositionArray;
            from = arr.Get(MyIndex).ToDoubleVector2();
            to = arr.Get(next).ToDoubleVector2();
            DoubleRect r= DoubleRect.FromTwoPoints(from, to);
            r.Height = settings.BottomPosition - r.Y;
            return r;
        }

        public override int ItemCount { get { return 1; } }

        public override void DiscardData()
        {
           
        }
    }
}
