using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    public class FixedDivisionAxisGenerator<T> : AxisLineDataGenerator where T : DataSeriesBase
    {
        const int GenerateFactor = 10;
        SpanningValue mGapUnits = new SpanningValue(-1.0);
        double mModifiedGapUnits = -1.0;
        uint mCurrentDivisionBase = 0;
        uint mMaxDivisions;
        double mMaxDivisionsBase;
        TextDataHolder mHolder = new TextDataHolder();
        ViewPortion mAbsolutePortion;
        ViewPortion mGenerationView;

        double mMinTextValue, mMaxTextValue;
        double mOrigin = 0;
        int mCurrentAmount = 0;
        float smallMargin = 0.001f;

        public FixedDivisionAxisGenerator(string name, GameObject obj) 
            : base(name, obj)
        {

        }

        public override bool ValidateSettings(IDataSeriesSettings settings, out string error)
        {
            DataSeries.ApplySettings(settings, "Axis", VisualFeatureName);        
            UnboxSetting<SpanningValue>(ref mGapUnits, settings, "gapUnits", new SpanningValue(1.0), DataSeriesRefreshType.FullRefresh);
            UnboxSetting<uint>(ref mMaxDivisions, settings, "maxDivisions", 20, DataSeriesRefreshType.FullRefresh);
            UnboxSetting<double>(ref mMaxDivisionsBase, settings, "maxDivisionsBase", 5, DataSeriesRefreshType.FullRefresh);
            if (mGapUnits.Value <= 0.0)
            {
                error = "Gap units muse be larger then zero";
                return false;
            }
            mModifiedGapUnits = mGapUnits.Value;
            bool res = base.ValidateSettings(settings, out error);
            mHolder.Direction = Direction;
            Generate();
            return res;
        }

        protected override void OnRefresh()
        {
            base.OnRefresh();
            Generate(); // generate the data when refreshing the view
        }

        double GetViewSize()
        {
            
            if (Direction == AxisDimension.X)
                return OriginalView.Width;
            if (Direction == AxisDimension.Y)
                return OriginalView.Height;
            return OriginalView.Height; // depth
        }

        double NormalizedGap()
        {
            if (Direction == AxisDimension.X)
                return OriginalView.NormlizeWidth(mModifiedGapUnits);
            if (Direction == AxisDimension.Y)
                return OriginalView.NormlizeHeight(mModifiedGapUnits);
            return OriginalView.NormlizeHeight(mModifiedGapUnits); // depth
        }

        public override TextDataHolder LabelData { get { return mHolder; } }

        void AddLines()
        {
            double position = 0.0;
            if (Direction == AxisDimension.X)
            {
                for (int i = 0; i < mCurrentAmount; i++)
                {
                    //         ChartCommon.DevLog(LogOptions.Axis, GetType().Name, "division number", i, "position", currentPosition);
                    AddLine(new DoubleVector3(position, 0.0), new DoubleVector3(position, 1.0));
                    position += mModifiedGapUnits;
                }
            }
            else
            {
                if (Direction == AxisDimension.Y)
                {
                    for (int i = 0; i < mCurrentAmount; i++)
                    {
                        //         ChartCommon.DevLog(LogOptions.Axis, GetType().Name, "division number", i, "position", currentPosition);
                        AddLine(new DoubleVector3(0.0, position), new DoubleVector3(1.0, position));
                        position += mModifiedGapUnits;
                    }
                }
            }
        }

        void GenerateText()
        {
            mHolder.ClearTexts();
            mOrigin = GetViewOrigin();
            if (IsActive == false)
                return;
            if (Direction == AxisDimension.X)
            {
                double totalSize = OriginalView.Width ;
                double factorSize = totalSize * GenerateFactor;

                double firstPosition = getNearestDivisionFloor(OriginalView.From.x - (factorSize * 0.5));
                double lastPosition = getNearestDivisionCeiling(OriginalView.From.x + (factorSize * 0.5));

                mMinTextValue = firstPosition;
                mMaxTextValue = lastPosition;

                int total = (int)((lastPosition - firstPosition) / mModifiedGapUnits);
                if (total > 0)
                {
                    for (int i = 0; i <= total; i++)
                    {
                        double factor = ((double)i) / (double)total;
                        double position = factor * lastPosition + (1 - factor) * firstPosition;

                        mHolder.AddText(new DoubleVector3(position, smallMargin), position);
                    }
                }
                else
                    mHolder.AddText(new DoubleVector3(lastPosition, smallMargin), lastPosition);
            }
            else
            {
                double totalSize = OriginalView.Height ;
                double factorSize = totalSize * GenerateFactor;

                double firstPosition = getNearestDivisionFloor(OriginalView.From.y - (factorSize * 0.5));
                double lastPosition = getNearestDivisionCeiling(OriginalView.From.y + (factorSize * 0.5));

                mMinTextValue = firstPosition;
                mMaxTextValue = lastPosition;
                int total = (int)((lastPosition - firstPosition) / mModifiedGapUnits);
                if (total > 0)
                {
                    for (int i = 0; i <= total; i++)
                    {
                        double factor = ((double)i) / (double)total;
                        double position = factor * lastPosition + (1 - factor) * firstPosition;
                        mHolder.AddText(new DoubleVector3(smallMargin, position), position);
                    }
                }
                else
                    mHolder.AddText(new DoubleVector3(smallMargin, lastPosition), lastPosition);
            }
        }
        uint CalculateDivisionBase()
        {
            return (uint)Math.Max(1, Math.Pow(mMaxDivisionsBase, Math.Ceiling(Math.Log(GetViewSize() / (mMaxDivisions * mGapUnits.Value), mMaxDivisionsBase))));
        }
        void Generate()
        {
            mCurrentAmount=0;
            ClearLines();
            if (mGapUnits.Value <= 0.00001)
                return;
            if (OriginalView.Width <= 0.000001 || OriginalView.Height <= 0.00001)
                return;
 
            mGenerationView = OriginalView;
            ChartCommon.DevLog(LogOptions.Axis, GetType().Name, "generating divisions","view:", OriginalView);
            //int totalItems = ((int)Math.Round(GetViewSize() / mGapUnits)) + 1;
            mCurrentDivisionBase = CalculateDivisionBase();
            mModifiedGapUnits = mGapUnits.Value * mCurrentDivisionBase;
            int totalItems = ((int)Math.Round(GetViewSize() / mModifiedGapUnits));

            mCurrentAmount = totalItems * GenerateFactor; // recreate the i
         //   Debug.Log("modified " + mModifiedGapUnits);
          //  Debug.Log("total " + totalItems);
            ChartCommon.DevLog(LogOptions.Axis, GetType().Name, "modified items", mModifiedGapUnits);
            ChartCommon.DevLog(LogOptions.Axis, GetType().Name, "fit items", totalItems);       
            ChartCommon.DevLog(LogOptions.Axis, GetType().Name, "total items", mCurrentAmount);

            AddLines();
            GenerateText();
            DataSeries.Invalidate();
        }

        double GetViewOrigin()
        {
            if (Direction == AxisDimension.X)
                return Parent.Axis.View.HorizontalViewOrigin;
            if (Direction == AxisDimension.Y)
                return Parent.Axis.View.VerticalViewOrigin;
            return Parent.Axis.View.VerticalViewOrigin;
        }

        double GetViewStart()
        {
            if (Direction == AxisDimension.X)
                return Parent.Axis.View.HorizontalViewOrigin + OriginalView.From.x;
            if (Direction == AxisDimension.Y)
                return Parent.Axis.View.VerticalViewOrigin + OriginalView.From.y;
            return Parent.Axis.View.VerticalViewOrigin + OriginalView.From.y;
        }

        double GetOffset()
        {
            if(Direction == AxisDimension.X)
                return Parent.Axis.View.HorizontalViewOrigin - OriginalView.From.x;
            if(Direction == AxisDimension.Y)
                return Parent.Axis.View.VerticalViewOrigin - OriginalView.From.y;
            return Parent.Axis.View.VerticalViewOrigin - OriginalView.From.y;
        }

        double InflateOffset(double value)
        {
            if(Direction == AxisDimension.X)
                return LocalRect.Width* OriginalView.NormlizeWidth(value);
            if(Direction == AxisDimension.Y)
                return LocalRect.Height * OriginalView.NormlizeHeight(value);
            return LocalRect.Height * OriginalView.NormlizeHeight(value);
        }

        double LocalSize()
        {
            if (Direction == AxisDimension.X)
                return LocalRect.Width;
            if (Direction == AxisDimension.Y)
                return LocalRect.Height;
            return LocalRect.Height;
        }

        double GetScale()
        {
            if(Direction == AxisDimension.X)
                return mGenerationView.Width/ OriginalView.Width;
            if (Direction == AxisDimension.Y)
                return mGenerationView.Height/OriginalView.Height;
            return mGenerationView.Height / OriginalView.Height;
        }

        AxisSystemView.AxisOrientaion GetAxisDirection()
        {
            switch(Direction)
            {
                case AxisDimension.X:
                    return Parent.Axis.View.XAxisDirection;
                case AxisDimension.Y:
                    return Parent.Axis.View.YAxisDirection;
                case AxisDimension.Z:
                    return Parent.Axis.View.ZAxisDirection;
            }
            return AxisSystemView.AxisOrientaion.Default;
        }

        public override void FitInto(ViewPortion localRect)
        {
            base.FitInto(localRect);
            CheckDivisionAmount();
        }

        void CheckDivisionAmount()
        {
            int totalItems = ((int)Math.Round(GetViewSize() / mModifiedGapUnits));
            ChartCommon.DevLog(LogOptions.Axis, GetType().Name, "check division amount", "total:", totalItems,"current:", mCurrentAmount,"view:",OriginalView);
            uint currentBase = CalculateDivisionBase();
            
            if (mCurrentAmount < totalItems || totalItems > mMaxDivisions || mCurrentDivisionBase!=currentBase) //  || (totalItems < mMaxDivisionsBase && mModifiedGapUnits > mGapUnits))
                Generate();

            if (mOrigin != GetViewOrigin() || (mMinTextValue + mModifiedGapUnits > GetViewStart()) || mMaxTextValue - mModifiedGapUnits < GetViewStart() + GetViewSize()) // we are reaching the end of the drawn texts
            {
                //if (Name == "XDiv")
                //{
                //    Debug.Log("mMinTextValue " + mMinTextValue);
                //    Debug.Log("mMaxTextValue " + mMaxTextValue);
                //    Debug.Log("mModifiedGapUnits " + mModifiedGapUnits);
                //    Debug.Log("GetViewStart() " + GetViewStart());
                //    Debug.Log("GetViewSize() " + GetViewSize());
                //}
                GenerateText();
            }
        }

        protected override void OnHasView()
        {
            base.OnHasView();
            Generate(); 
        }

        protected override ViewPortion DataSeriesView { get { return mAbsolutePortion; } }

        double getNearestDivisionFloor(double position)
        {
            double offset = position - GetViewOrigin();
            offset = (Math.Floor(offset / mModifiedGapUnits) -1 ) * mModifiedGapUnits + GetViewOrigin();
            return offset;
        }
        double getNearestDivisionCeiling(double position)
        {
            double offset = position - GetViewOrigin();
            offset = (Math.Ceiling(offset / mModifiedGapUnits)+1) * mModifiedGapUnits + GetViewOrigin();
            return offset;
        }
        void ModifyXView(ViewPortion view)
        {
            double offset = view.From.x - Parent.Axis.View.HorizontalViewOrigin;
            offset -= Math.Floor(offset / mModifiedGapUnits) * mModifiedGapUnits;
            mAbsolutePortion = new ViewPortion(offset, 0, view.Width, 1.0, view.ViewDiagonalBase, view.OppositeX, view.OppositeY);
        }

        void ModifyYView(ViewPortion view)
        {
            double offset = view.From.y - Parent.Axis.View.VerticalViewOrigin;
            offset -= Math.Floor(offset / mModifiedGapUnits) * mModifiedGapUnits;
            mAbsolutePortion = new ViewPortion(0, offset, 1.0, view.Height, view.ViewDiagonalBase, view.OppositeX, view.OppositeY);
        }

        public override void OnSetView(ViewPortion view)
        {        
            if (Direction == AxisDimension.X)
                ModifyXView(view);
            else
                ModifyYView(view);
            base.OnSetView(view);
            CheckDivisionAmount();
        }

        protected override DataSeriesBase GenerateSeries(GameObject obj)
        {
            return obj.AddComponent<T>();
        }

    }
}
