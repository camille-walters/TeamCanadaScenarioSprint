using DataVisualizer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DataVisualizer
{
    public class CanvasIndicatorInteraction : CanvasInteractionBase
    {
        public RectTransform Indicator;
        public RectTransform Line;
        public Text IndicatorText;
        public string TextFormat = "(<?x>,<?y>)";

        Dictionary<string, object> mArguments = new Dictionary<string, object>();

        protected override void Start()
        {
            base.Start();
        }

        void PickNone()
        {
            Indicator.gameObject.SetActive(false);
            Line.gameObject.SetActive(false);
            IndicatorText.gameObject.SetActive(false);
        }

        void Pick(string category,int index)
        {
            Line.gameObject.SetActive(true);
            IndicatorText.gameObject.SetActive(true);
            DoubleVector3 chartPoint;
            Vector3 localPoint = GetPointLocalSpace(category, index,out chartPoint);
            Indicator.anchoredPosition = localPoint;
            Line.GetComponent<RectTransform>().anchoredPosition = new Vector2(localPoint.x,0);
            var format = StringFormatter.GetFormat(TextFormat);
            mArguments.Clear();
            mArguments[StringFormatter.ParameterXValue] = chartPoint.x;
            mArguments[StringFormatter.ParameterYValue] = chartPoint.y;
            IndicatorText.text = format.FormatValues(mArguments,Chart);
            if (Chart.Axis.LocalViewContains(localPoint))
                Indicator.gameObject.SetActive(true);
            else
                Indicator.gameObject.SetActive(false);
        }

        void Update()
        {
            string category;
            int index;
            if (PickWithPointer(out category, out index))
                Pick(category, index);
            else
                PickNone();
        }

    }
}