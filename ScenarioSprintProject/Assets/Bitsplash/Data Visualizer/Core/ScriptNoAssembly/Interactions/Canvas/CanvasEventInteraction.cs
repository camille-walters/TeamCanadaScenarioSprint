using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DataVisualizer
{
    public class CanvasEventInteraction : CanvasInteractionBase
    {
        public ChartClickEvent ClickEvent = new ChartClickEvent();
        public double PixelThreshold = 15;
        public string TextFormat = "(<?x>,<?y>)";
        Dictionary<string, object> mArguments = new Dictionary<string, object>();

        protected override void Start()
        {
            base.Start();
            HookEvents();
        }
        void HookEvents()
        {
            if (InteractionManager == null)
                return;
            InteractionManager.OnPointerClick -= InteractionManager_OnPointerClick;
            InteractionManager.OnPointerClick += InteractionManager_OnPointerClick;
        }
        private void InteractionManager_OnPointerClick()
        {
            string category;
            int index;
            if (PickWithPointer(out category, out index))
                Pick(category, index);
        }

        private void OnEnable()
        {
            HookEvents();
        }
        private void OnDisable()
        {
            InteractionManager.OnPointerClick -= InteractionManager_OnPointerClick;
        }

        void Pick(string category, int index)
        {
            DoubleVector3 chartPoint;
            Vector3 localPoint = GetPointLocalSpace(category, index, out chartPoint);
            if((((Vector2)localPoint) - InteractionManager.PointerPosition).sqrMagnitude < PixelThreshold * PixelThreshold)
            {
                var format = StringFormatter.GetFormat(TextFormat);
                mArguments.Clear();
                mArguments[StringFormatter.ParameterXValue] = chartPoint.x;
                mArguments[StringFormatter.ParameterYValue] = chartPoint.y;
                string text = format.FormatValues(mArguments, Chart);
                if (ClickEvent != null)
                {
                    ClickEvent.Invoke(new ChartClickEventArgs(category, index, chartPoint, localPoint, text));
                }
            }
        }
    }
}
