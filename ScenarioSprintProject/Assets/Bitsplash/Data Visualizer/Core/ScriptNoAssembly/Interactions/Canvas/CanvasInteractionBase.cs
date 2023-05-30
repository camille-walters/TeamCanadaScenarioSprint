using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DataVisualizer
{
    [RequireComponent(typeof(RectTransform))]
    public class CanvasInteractionBase : MonoBehaviour
    {
        protected CanvasInteractionManager InteractionManager { get; private set; }
        protected CanvasDataSeriesChart Chart { get; private set; }
        protected virtual void Start()
        {
            InteractionManager = GetComponentInParent<CanvasInteractionManager>();
            Chart = GetComponentInParent<CanvasDataSeriesChart>();
            if (Chart == null)
            {
                ChartCommon.RuntimeWarning("Canvas Interaction must have a CanvasDataSeriesChart parent");
                enabled = false;
            }
            if (InteractionManager == null)
            {
                ChartCommon.RuntimeWarning("Canvas Interaction must have an CanvasInteractionManager parent");
                enabled = false;
            }
            CanvasDataSeriesChart.SetRectTransformToFill(gameObject);            
        }
        /// <summary>
        /// this methods picks the point cloeset to the input pointer , distance is calculated along the x-axis only
        /// </summary>
        /// <param name="category">the category of the picked point , or null if no point was picked</param>boo
        /// <param name="index">the index of the picked point , or -1 if no point was picked</param>
        /// <returns>true if a point was picked , false otherwise</returns>
        protected bool PickWithPointer(out string category, out int index)
        {
            index = -1;
            category = null;
            if (InteractionManager.IsPointerInside == false)                          
                return false;
            Vector3 mousePosition = InteractionManager.PointerPosition;
            DoubleVector3 chartPosition = Chart.Axis.LocalSpaceToChartSpace(mousePosition);
            double minSqrDist = double.PositiveInfinity;
            foreach (DataSeriesCategory cat in Chart.DataSource.Categories)
            {
                int pickedIndex = cat.Data.Pick(chartPosition);
                DoubleVector3 pickedPoint = cat.Data.GetPointAt(pickedIndex);
                double sqrDist = (pickedPoint - chartPosition).sqrMagnitude;
                if(sqrDist < minSqrDist)
                {
                    minSqrDist = sqrDist;
                    index = pickedIndex;
                    category = cat.Name;
                }
            }
            return true;
        }

        public Vector3 GetPointLocalSpace(string category, int index)
        {
            DoubleVector3 dummy;
            return GetPointLocalSpace(category, index, out dummy);
        }
        public Vector3 GetPointLocalSpace(string category,int index,out DoubleVector3 chartSpace)
        {
            chartSpace = Chart.DataSource.GetCategory(category).Data.GetPointAt(index);
            return Chart.Axis.ChartSpaceToLocalSpace(chartSpace);
        }

        /// <summary>
        /// returns the distance in pixels between the poniter and a point in chart space
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        protected double PointDistanceFromPointerInPixels(DoubleVector3 point)
        {
            Vector3 mousePosition = InteractionManager.PointerPosition;
            Vector3 localPoint = Chart.Axis.ChartSpaceToLocalSpace(point);
            return (mousePosition - localPoint).magnitude;
        }
    }
}
