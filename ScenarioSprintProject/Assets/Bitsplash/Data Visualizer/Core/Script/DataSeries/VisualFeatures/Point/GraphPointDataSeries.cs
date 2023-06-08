using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    public class GraphPointDataSeries : DataSeriesBase
    {
        public const string PointSizeSetting = "pointSize";
        public const string PointMaterital = "pointMaterial";
        public const string PointScaleable = "scalesWithView";

        IDataSeriesSettings mSettings;
        Material mMaterial;
        PointSeriesObject.RectCanvasGraphicSettings mPointSettings = new PointSeriesObject.RectCanvasGraphicSettings();

        public GraphPointDataSeries() : base(ArrayManagerType.Compact, 4)
        {
        }
        
        protected override bool IsCanvas
        {
            get
            {
                return true;
            }
        }

        public override object GraphicSettingsObject {  get { return mPointSettings; } }

        protected override bool ValidateSettings(IDataSeriesSettings settings, out string error)
        {
            if (base.ValidateSettings(settings, out error) == false)
                return false;

            error = null;
            mSettings = settings;

            if (UnboxSetting(ref mPointSettings.mSize, mSettings, PointSizeSetting, 1.0))
                mPointSettings.mHalfSize = mPointSettings.mSize * 0.5f;
            UnboxSetting(ref mPointSettings.mScalable, mSettings, PointScaleable, false);
            UnboxSetting(ref mMaterial, mSettings, PointMaterital, null);

            if (mPointSettings.mSize <= 0.00001)
            {
                error = "Point size must be larger then 0";
                return false;
            }


            if (mMaterial == null)
            {
                error = "Point material cannot be null";
                return false;
            }

            var graphic = Graphic;

            SetExtrusion();
            if (graphic != null)
            {
                graphic.SetMaterial(mMaterial, false);
            }

            return true;
        }

        public override Rect GetUvRect(int index, SeriesObject obj)
        {
            return new Rect(0f,0f,1f,1f);
        }

        void SetExtrusion()
        {
            IChartGraphic graphic = Graphic;
            if (graphic != null)
            {
                double viewDiagonal = new DoubleVector2(CurrentView.Width, CurrentView.Height).ToVector2().magnitude;
                if (viewDiagonal > 0)
                {
                    double size = mPointSettings.mSize;
                    if (mPointSettings.mScalable)
                        size *= ViewDiagonalRatio;
                    graphic.ExtrusionAmount = (float)size;
                }
            }
        }

        protected override ArrayManagerType GetArrayType(InputType type)
        {
            if (type == InputType.Static)
                return ArrayManagerType.Static;
            if (type == InputType.Realtime)
                return ArrayManagerType.CompactRealtime;
            if (type == InputType.Streaming)
                return ArrayManagerType.StreamingCompact;
            return ArrayManagerType.Compact;
        }

        public override void FitInto(ViewPortion localView)
        {
            base.FitInto(localView);
            SetExtrusion();
        }

        public override void OnSetView(ViewPortion chartSpaceView)
        {
            base.OnSetView(chartSpaceView);
            SetExtrusion();
        }

        protected override void GenerateObjectsForIndex(int index, IList<SeriesObject> objects)
        {
            if (mSettings == null)
                return;
            PointSeriesObject point = null;
            if (HasColor)
                point = new ColorPointSeriesObject();
            else
                point = new PointSeriesObject();
            objects.Add(point);
        }

        protected override void GetConnectedInidices(int index, IList<int> related)
        {
            // no connected indices only the current one
        }
    }
}
