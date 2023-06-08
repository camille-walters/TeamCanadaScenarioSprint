using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    public class AxisLabels2DDataGenerator : AxisLabelsDataGenerator
    {
        ViewPortion mAbsolutePortion;

        public AxisLabels2DDataGenerator(string name, GameObject obj, TextDataHolder holder) : base(name, obj, holder)
        {

        }

        public override bool ValidateSettings(IDataSeriesSettings settings, out string error)
        {
            error = null;
            DataSeries.ApplySettings(settings, "Axis", VisualFeatureName);
            return true;
        }

        protected override ViewPortion DataSeriesView { get { return mAbsolutePortion; } }

        public override void OnSetView(ViewPortion view)
        {
            if(Holder.Direction == AxisDimension.X)
                mAbsolutePortion = new ViewPortion(view.From.x, 0, view.Width, 1.0, new Vector2(1f, 1f).magnitude,view.OppositeX,view.OppositeY);
            else
                mAbsolutePortion = new ViewPortion(0, view.From.y, 1 , view.Height, new Vector2(1f, 1f).magnitude, view.OppositeX, view.OppositeY);
            base.OnSetView(view);
        }
        protected override DataSeriesBase GenerateSeries(GameObject obj)
        { 
            return obj.AddComponent<ItemLabelsDataSeries>();
        }

    }
}
