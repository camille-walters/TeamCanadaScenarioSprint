using System;
using System.Collections.Generic;
using UnityEngine;

namespace DataVisualizer{
    public class GraphLineFillDataSeries : UvLengthDataSeries
    {
        public const string FillMaterialSetting = "fillMaterial";
        public const string FillUvTypeSetting = "uvMapping";
        public const string FillMinimumUvSetting = "minimumUv";
        public const string FillMaximumUvSetting = "maximumUv";
        
        IDataSeriesSettings mSettings;
        LineFillSeriesObject.LineFillGraphSettings mFillSettings = new LineFillSeriesObject.LineFillGraphSettings();
        MappingFunction? mOriginalMapping = null;
        MappingFunction? mCurrentMapping = null;
        FillUv mFillUvSetting;
        double mUvMin, mUvMax;
        double? mCurrentBottom = null;
        Material mMaterial;

        public GraphLineFillDataSeries() 
            : base(ArrayManagerType.Compact, 4)
        {

        }

        protected override bool IsCanvas
        { 
            get { return true; }
        }
        
        public override object GraphicSettingsObject { get { return mFillSettings; } }

        protected override bool HasTangent { get { return false; } }

        protected Rect UVRectTile(int index, SeriesObject obj)
        {
            double total = TotalLength;
            double length, accumilated;

            GetLengthForIndex(index, out length, out accumilated);

            ChartCommon.DevLog(LogOptions.UvDataSeries, GetType().Name, "uv rect calculation", "length:",length, "acummilated:", accumilated);
            double divider = 1.0;
            if (UvDivider.HasValue)
                divider = UvDivider.Value;
            accumilated /= divider;
            length /= divider;

            if(Parent.Axis.View.YAxisDirection == AxisSystemView.AxisOrientaion.Default)
                return new Rect((float)accumilated, 0f, (float)length, 1f);
            return new Rect((float)accumilated, 1f, (float)length, -1f); // if the axis direction is inverted then y coords are inverted also
        }

        protected override void OnTotalLengthChanged()
        {
            base.OnTotalLengthChanged();
        }

        double GetUvPosition(FillUvMapping mapping,double defaultValue,bool forceAutomatic)
        {
            if (mapping.Automatic || forceAutomatic)
                return defaultValue;
            return mapping.Value;
        }
        
        protected override bool ValidateSettings(IDataSeriesSettings settings, out string error)
        {
            if (base.ValidateSettings(settings, out error) == false)
                return false;
            mSettings = settings;
            UnboxSetting(ref mFillUvSetting, mSettings, FillUvTypeSetting, FillUv.Strech);
            if(HasColor)
                mFillSettings.uvMethod = (mFillUvSetting == FillUv.Strech) ? ((LineFillSeriesObject.UvMethod)LineFillSeriesObject.DrawFromToStretch) : ((LineFillSeriesObject.UvMethod)LineFillSeriesObject.DrawFromToClamped);
            else
                mFillSettings.uvMethod = (mFillUvSetting == FillUv.Strech) ? ((LineFillSeriesObject.UvMethod)LineFillSeriesObject.DrawFromToStretchNoColor) : ((LineFillSeriesObject.UvMethod)LineFillSeriesObject.DrawFromToClampedNoColor);
            UnboxSetting(ref mFillSettings.MinimumUv, mSettings, FillMinimumUvSetting, FillUvMapping.Auto);
            UnboxSetting(ref mFillSettings.MaximumUv, mSettings, FillMaximumUvSetting, FillUvMapping.Auto);
            UnboxSetting(ref mMaterial, mSettings, FillMaterialSetting, null);

            //mFillSettings.TringleStrip = false;

            if (mMaterial == null)
            {
                error = "Line material cannot be null";
                return false;
            }

            var graphic = Graphic;

            if (graphic != null)
            {
                graphic.SetMaterial(mMaterial, false);
                graphic.TringleStrip = false;
            }
            if (TilingMethod == UvLengthVisualFeature.MaterialTilingMethodOptions.NoTilingBestPerformance)
                UVMethod = SimpleUVRectMethod;
            else
                UVMethod = UVRectTile;

            error = null;
            ChartCommon.PerformAtEndOfFrameOnCondition(this, EntitesGenerated, SetMappingFunction);
            CheckSetGraphBottom();
            return true;
        }

        public override void UniformUpdate()
        {
            base.UniformUpdate();
            //MakeAllDirty(false);
            ChartCommon.PerformAtEndOfFrameOnCondition(this, EntitesGenerated, SetMappingFunction);
            CheckSetGraphBottom();
        }
        
        void SetOriginalMapping(MappingFunction mapping)
        {
            ChartCommon.DevLog("fill uv", "set original");
            mFillSettings.UvMappingFunction = mapping;
            mOriginalMapping = mapping;
            Graphic.UvMapping = Graphic.UvMapping.ModifyY(0.0,1.0);
            MakeAllDirty(true);                             // make all uv's dirty
            Invalidate();
        }

        void CheckSetGraphBottom()
        {
            if (CurrentView.Height <= 0)
                return;
            bool invalidate = false;
            if (Parent.Axis.View.YAxisDirection == AxisSystemView.AxisOrientaion.Default)
            {
                double minValue = Math.Min(CurrentView.Min.y, Parent.Axis.ChartSpaceView.From.y);
                if(mCurrentBottom.HasValue == false || mCurrentBottom.Value > minValue)
                {
                    mCurrentBottom = minValue - CurrentView.Height * 2;
                    invalidate = true;
                }
            }
            else
            {
                double maxValue = Math.Min(CurrentView.Max.y, Parent.Axis.ChartSpaceView.To.y);
                if (mCurrentBottom.HasValue == false || mCurrentBottom.Value < maxValue)
                {
                    mCurrentBottom = maxValue + CurrentView.Height * 2;
                    invalidate = true;
                }
            }
            if(invalidate)
            {
                mFillSettings.BottomPosition = mCurrentBottom.Value;
                ChartCommon.DevLog("Fill Bottom",mFillSettings.BottomPosition);
                MakeAllDirty(false);            // the bottom of all vertex objects should be changed
                Invalidate();
            }
        }
        
        public override double GetDoubleArgument(int index)
        {
            return mFillSettings.BottomPosition;
        }

        /// <summary>
        /// return either the value of strech or false , depending on the current y axis orientation.
        /// this is used to determine which side of the fill is up (and therefore the part that is streched)
        /// </summary>
        /// <param name="isMax"></param>
        /// <param name="strech"></param>
        /// <returns></returns>
        bool CheckStretch(bool isMax,bool strech)
        {
            if(Parent.Axis.View.YAxisDirection == AxisSystemView.AxisOrientaion.Default)
            {
                if (isMax)
                    return strech;
                return false;
            }
            if (isMax)
                return false;
            return strech;
        }

        DoubleRect MappingUvRect()
        {
            if (Parent.Axis.View.YAxisDirection == AxisSystemView.AxisOrientaion.Default)
                return new DoubleRect(0.0, 0.0, 1.0, 1.0);
            return new DoubleRect(0.0, 1.0, 1.0, -1.0);
        }

        bool IsStrech
        {
            get { return mFillUvSetting == FillUv.Strech; }
        }

        public override MappingFunction UVMappingFunction()
        {
            return mFillSettings.UvMappingFunction;
        }

        void SetMappingFunction()
        {
            double min = GetUvPosition(mFillSettings.MinimumUv, Parent.Axis.ChartSpaceView.From.y, CheckStretch(false, IsStrech));
            double max = GetUvPosition(mFillSettings.MaximumUv, Parent.Axis.ChartSpaceView.To.y, CheckStretch(true, IsStrech));

            if (mUvMin == min && mUvMax == max) // nothing changed since last time ,no need to update the mapping function
                return;

            mUvMin = min;
            mUvMax = max;
        
            DoubleRect mappingToRect = MappingUvRect();
            DoubleRect mappingFromRect = new DoubleRect(0.0, max, 1.0, min - max);
            //DoubleRect mappingFromRect = new DoubleRect(0.0, min, 1.0, max - min);
            mCurrentMapping = new MappingFunction(mappingFromRect, mappingToRect);
          //  ChartCommon.DevLog("current Map", mCurrentMapping);
            if (mOriginalMapping == null)
            {
                // optimized uv is not supported , or no mapping is set
                SetOriginalMapping(mCurrentMapping.Value);
            }
            else
            {
               // ChartCommon.DevLog("uv", "check diff");
                MappingFunction? diff = mOriginalMapping.Value.CreateDifferenceMapping(mappingFromRect, mappingToRect, MultiplyThreshold, AddThreshold);
                if (diff == null) //diffrence is to big , reassign mapping function and make all uv dirty
                    SetOriginalMapping(mCurrentMapping.Value);
                else
                {
               //     ChartCommon.DevLog("uv", "diff");
                    Graphic.UvMapping = Graphic.UvMapping.ModifyY(diff.Value); // assign the difference mapping to the underlying graphic
            //        ChartCommon.DevLog("original", mOriginalMapping.Value);
            //        ChartCommon.DevLog("UvMapping", Graphic.UvMapping);
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
            CheckSetGraphBottom();
            ChartCommon.PerformAtEndOfFrameOnCondition(this, EntitesGenerated, SetMappingFunction);
        }
        protected override IDataViewerNotifier ModifyView(IDataViewerNotifier mainData)
        {
            return mainData.GetDataView(GraphDataType.ViewName);
        }
        public override void OnSetView(ViewPortion chartSpaceView)
        {
            base.OnSetView(chartSpaceView);
            if (chartSpaceView.Width <= 0 || chartSpaceView.Height <= 0)
                mCurrentBottom = null;
            else
                CheckSetGraphBottom();
            ChartCommon.PerformAtEndOfFrameOnCondition(this, EntitesGenerated, SetMappingFunction);
        }

        protected override void GenerateObjectsForIndex(int index, IList<SeriesObject> objects)
        {
            if (mSettings == null)
                return;

            LineFillSeriesObject line = null;
            if (TilingMethod == UvLengthVisualFeature.MaterialTilingMethodOptions.NoTilingBestPerformance)
            {
                if (HasColor)
                {
                    line = new OptiimzedLineFillColorSeriesObject();
                }
                else
                    line = new OptimiziedLineFillSeriesObject();
            }
            else
                line = new LineFillSeriesObject();
            objects.Add(line);
        }

        protected override void GetConnectedInidices(int index, IList<int> related)
        {
            int add = index + 1;
            if (add < Count && add>=0)
                related.Add(add);
            int prev = index - 1;
            if (prev >= 0 && prev<Count)
                related.Add(prev);
        }
    }
}
