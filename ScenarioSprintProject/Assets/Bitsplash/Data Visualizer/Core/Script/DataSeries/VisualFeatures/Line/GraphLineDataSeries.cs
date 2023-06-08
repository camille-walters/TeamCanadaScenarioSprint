using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    /// <summary>
    /// Data series of graph outline. This can be used in conjuction with graph fill and points to create a fully featured graph chart
    /// </summary>
    class GraphLineDataSeries : UvLengthDataSeries
    {
        public const string LineScaleableSetting = "scalesWithView";
        public const string LineCapSetting = "lineCap";
        public const string LineThicknessSetting = "lineThickness";
        public const string LineMaterital = "lineMaterial";
        public const string LineMateritalTiling = "lineMaterialTiling";

        IDataSeriesSettings mSettings;
        Material mMaterial;
        MaterialTiling mMaterialTiling;
        LineSeriesObject.LineCanvasGraphSettings mLineSettings = new LineSeriesObject.LineCanvasGraphSettings();

        public GraphLineDataSeries() : base(ArrayManagerType.Compact,8)
        {

        }

        protected override bool IsCanvas
        {
            get
            {
                return true;
            }
        }

        public override object GraphicSettingsObject { get { return mLineSettings; } }

        protected override bool ValidateSettings(IDataSeriesSettings settings, out string error)
        {
            
            if (base.ValidateSettings(settings, out error) == false)
                return false;

            mSettings = settings;
            error = null;

            UnboxSetting(ref mLineSettings.mThickness, mSettings, LineThicknessSetting, 1.0);
            UnboxSetting(ref mLineSettings.mScaleableLine, mSettings, LineScaleableSetting, false);

            UnboxSetting(ref mMaterialTiling, mSettings, LineMateritalTiling, new MaterialTiling(false, 1f));
            UnboxSetting(ref mMaterial, mSettings, LineMaterital, null);

            if (TilingMethod == UvLengthVisualFeature.MaterialTilingMethodOptions.NoTilingBestPerformance)
                UVMethod = SimpleUVRectMethod;
            else
                UVMethod = UvRectTile;

            if (mLineSettings.mThickness <= 0.00001)
            {
                error = "Line thickness must be larger then 0";
                return false;
            }

            if (mMaterial == null)
            {
                error = "Line material cannot be null";
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

        protected Rect UvRectTile(int index, SeriesObject obj)
        {
            double total = TotalLength;
            double length, accumilated;
            GetLengthForIndex(index, out length, out accumilated);
            accumilated /= total;
            length /= total;
            return new Rect((float)accumilated, 0f, (float)length, 1f);
        }

        void SetExtrusion()
        {
            IChartGraphic graphic = Graphic;
            if (graphic != null)
            {
                if (ViewDiagonalBase > 0)
                {
                    double thickness = mLineSettings.mThickness;
                    if (mLineSettings.mScaleableLine)
                        thickness *= ViewDiagonalRatio;
          //          Debug.Log("Set Extrusion " + thickness);
                    graphic.ExtrusionAmount = (float)thickness;
                }
            }
        }


        public override void FitInto(ViewPortion localView)
        {
            base.FitInto(localView);
            SetExtrusion();
        }

        protected override ArrayManagerType GetArrayType(InputType type)
        {
            if (type == InputType.Static)
                return ArrayManagerType.Static;
            if (type == InputType.Realtime)
                return ArrayManagerType.CompactRealtime;
            if(type == InputType.Streaming)
                return ArrayManagerType.StreamingCompact;
            return ArrayManagerType.Compact;
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

            SeriesObject line = null;
            if (TilingMethod == UvLengthVisualFeature.MaterialTilingMethodOptions.NoTilingBestPerformance)
            {
                if (HasColor)
                    line = new CappedOptimizedLineWithColorSeriesObject();
                else
                    line = new OptimizedLineSeriesObject();
            }
            else
            {
                if (HasColor)
                    line = new SimpleLineWithColor();
                else
                    line = new SimpleLineSeriesObject();
            }

            objects.Add(line);
        }
        protected override IDataViewerNotifier ModifyView(IDataViewerNotifier mainData)
        {
            return mainData.GetDataView(GraphDataType.ViewName);
        }
        protected override void GetConnectedInidices(int index, IList<int> related)
        {
            int add = index + 1;
            if(add < Count)
                related.Add(add);
            int prev = index - 1;
            if (prev >= 0)
                related.Add(prev);
        }
    }
}
