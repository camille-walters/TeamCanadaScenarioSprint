using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    [Serializable]
    public class AxisSystemView
    {
        public event Action OnViewParametersChanged;

        protected void OnParametersChanged()
        {
            if (OnViewParametersChanged != null)
                OnViewParametersChanged();
        }

        protected void OnOriginChanged()
        {
            OnParametersChanged();
        }

        protected void OnDirectionChanged()
        {
            OnParametersChanged();
        }
        protected void OnSizeChanged()
        {
            OnParametersChanged();
        }

        protected void OnScrollChange()
        {
            OnParametersChanged();
        }

        public enum AxisOrientaion
        {
            Default,
            Inverted
        }

        public double HorizontalViewStart
        {
            get
            {
                return HorizontalViewOrigin + HorizontalScrolling;
            }
        }

        public double VerticalViewStart
        {
            get { return VerticalViewOrigin + VerticalScrolling; }
        }

        public double HorizontalViewEnd
        {
            get { return HorizontalViewOrigin + HorizontalScrolling + HorizontalViewSize; }
        }
        public double VerticalViewEnd
        {
            get { return VerticalViewOrigin + VerticalScrolling + VerticalViewSize; }
        }

        [SerializeField]
        private AxisDirection horizontalDirection;

        public AxisDirection HorizontalDirection
        {
            get
            {
                return horizontalDirection;
            }
            set
            {
                horizontalDirection = value;
                OnDirectionChanged();
            }
        }
        [SerializeField]
        private AxisDirection verticalDirection;

        public AxisDirection VerticalDirection
        {
            get
            {
                return verticalDirection;
            }
            set
            {
                verticalDirection = value;
                OnDirectionChanged();
            }
        }

        [SerializeField]
        private bool horizontalPanning;

        public bool HorizontalPanning
        {
            get { return horizontalPanning; ; }
            set
            {
                horizontalPanning = value;
                OnParametersChanged();
            }
        }


        [SerializeField]
        private bool horizontalZooming;

        public bool HorizontalZooming
        {
            get { return horizontalZooming; ; }
            set
            {
                horizontalZooming = value;
                OnParametersChanged();
            }
        }


        [SerializeField]
        private bool verticalPanning;

        public bool VerticalPanning
        {
            get { return verticalPanning; ; }
            set
            {
                verticalPanning = value;
                OnParametersChanged();
            }
        }

        [SerializeField]
        private bool verticalZooming;
        public bool VerticalZooming
        {
            get { return verticalZooming; ; }
            set
            {
                verticalZooming = value;
                OnParametersChanged();
            }
        }

        public TimeSpan VerticalScrollingTimeSpan
        {
            get
            {
                return DateUtility.ValueToTimeSpan(VerticalScrolling);
            }
            set
            {
                VerticalScrolling = DateUtility.TimeSpanToValue(value);
            }
        }

        [SerializeField]
        protected SpanningValue verticalScrolling = new SpanningValue(0);

        public bool VerticalIsDateTime
        {
            get { return verticalViewOrigin.Type == 1; }
            set
            {
                verticalViewOrigin.Type = value ? 1 : 0;
            }
        }

        public double VerticalScrolling
        { 
            get { return verticalScrolling.Value; }
            set
            {
                verticalScrolling.Value = value;
                OnScrollChange();
            }
        }

        public TimeSpan HorizontalScrollingTimeSpan
        {
            get
            {
                return DateUtility.ValueToTimeSpan(HorizontalScrolling);
            }
            set
            {
                HorizontalScrolling = DateUtility.TimeSpanToValue(value);
            }
        }

        [SerializeField]
        protected SpanningValue horizontalScrolling = new SpanningValue(0);

        public bool HorizontalIsDateTime
        {
            get { return horizontalViewOrigin.Type == 1; }
            set
            {
                horizontalViewOrigin.Type = value ? 1 : 0;
            }
        }
        public double HorizontalScrolling
        {
            get { return horizontalScrolling.Value; }
            set
            {
                horizontalScrolling.Value = value;
                OnScrollChange();
            }
        }

        [SerializeField]
        [Tooltip("determines the ideal size of the dispaly. All line thickness and point sizes (etc) are scaled considering this value as the ideal view size")]
        private double thicknessBaseDiagonal = 10.0;

        /// <summary>
        /// determines the ideal size of the dispaly. All line thickness and point sizes (etc) are scaled considering this value as the ideal view size
        /// </summary>
        public double ThicknessBaseDiagonal
        {
            get { return thicknessBaseDiagonal; }
            set
            {
                thicknessBaseDiagonal = value;
                OnParametersChanged();
            }
        }

        [SerializeField]
        protected bool autoScrollHorizontally = false;

        public bool AutoScrollHorizontally
        {
            get { return autoScrollHorizontally; }
            set
            {
                autoScrollHorizontally = value;
                OnScrollChange();
            }
        }

        [SerializeField]
        protected bool autoScrollVertically = false;

        public bool AutoScrollVertically
        {
            get { return autoScrollVertically; }
            set
            {
                autoScrollVertically = value;
                OnScrollChange();
            }
        }

        /// <summary>
        /// set this to true to automatically detect the horizontal size of the graph chart
        /// </summary>
        [SerializeField]
        private bool automaticVerticallView = true;

        /// <summary>
        /// set this to true to automatically detect the horizontal size of the graph chart
        /// </summary>
        public bool AutomaticVerticallView
        {
            get { return automaticVerticallView; }
            set
            {
                automaticVerticallView = value;
                OnParametersChanged();
            }
        }


        /// <summary>
        /// set this to true to automatically detect the horizontal size of the graph chart
        /// </summary>
        [SerializeField]
        private bool automaticHorizontalView = true;

        /// <summary>
        /// set this to true to automatically detect the horizontal size of the graph chart
        /// </summary>
        public bool AutomaticHorizontalView
        {
            get { return automaticHorizontalView; }
            set
            {
                automaticHorizontalView = value;
                OnParametersChanged();
            }
        }

        public TimeSpan HorizontalViewSizeTimeSpan
        {
            get
            {
                return DateUtility.ValueToTimeSpan(HorizontalViewSize);
            }
            set
            {
                HorizontalViewSize = DateUtility.TimeSpanToValue(value);
            }
        }
        [SerializeField]
        private SpanningValue horizontalViewSize = new SpanningValue(100);

        /// <summary>
        /// Mannualy set the horizontal view size.
        /// </summary>
        public double HorizontalViewSize
        {
            get { return horizontalViewSize.Value; }
            set
            {
                horizontalViewSize.Value = value;
                OnSizeChanged();
            }
        }


        [SerializeField]
        private ScrollingValue horizontalViewOrigin = new ScrollingValue(0);

        /// <summary>
        /// Mannualy set the horizontal view origin. 
        /// </summary>
        public double HorizontalViewOrigin
        {
            get { return horizontalViewOrigin.Value; }
            set
            {
                horizontalViewOrigin.Value = value;
                OnOriginChanged();
            }
        }


        public DateTime HorizontalViewOriginDateTime
        {
            get { return DateUtility.ValueToDate(HorizontalViewOrigin); }
            set
            {
                HorizontalViewOrigin = DateUtility.DateToValue(value);
            }
        }
        public TimeSpan VerticalViewSizeTimeSpan
        {
            get
            {
                return DateUtility.ValueToTimeSpan(VerticalViewSize);
            }
            set
            {
                VerticalViewSize = DateUtility.TimeSpanToValue(value);
            }
        }

        [SerializeField]
        private SpanningValue verticalViewSize = new SpanningValue(100);

        /// <summary>
        /// Mannualy set the horizontal view size.
        /// </summary>
        public double VerticalViewSize
        {
            get { return verticalViewSize.Value; }
            set
            {
                verticalViewSize.Value = value;
                OnSizeChanged();
            }
        }
        /// <summary>
        /// for internal use only. Do not call this method
        /// </summary>
        public void RestoreValues(int axis, double origin, double size)
        {
            if (axis == 0)
            {
                if (AutomaticHorizontalView)
                {
                    if (HorizontalIsDateTime)
                    {
                        horizontalViewOrigin.Value = origin;
                        horizontalScrolling.Value = 0;
                    }
                    else
                    {
                        horizontalViewOrigin.Value = 0;
                        horizontalScrolling.Value = origin;
                    }
                    horizontalViewSize.Value = size;
                }
            }
            else
            {
                if (AutomaticVerticallView)
                {
                    if (VerticalIsDateTime)
                    {
                        verticalViewOrigin.Value = origin;
                        verticalScrolling.Value = 0;
                    }
                    else
                    {
                        verticalViewOrigin.Value = 0;
                        verticalScrolling.Value = origin;
                    }
                    verticalViewSize.Value = size;
                }
            }
        }

        [SerializeField]
        private ScrollingValue verticalViewOrigin = new ScrollingValue(0);

        /// <summary>
        /// Mannualy set the horizontal view origin. 
        /// </summary>
        public double VerticalViewOrigin
        {
            get { return verticalViewOrigin.Value; }
            set
            {
                verticalViewOrigin.Value = value;
                OnOriginChanged();
            }
        }

        public DateTime VerticalViewOriginDateTime
        {
            get { return DateUtility.ValueToDate(VerticalViewOrigin); }
            set
            {
                VerticalViewOrigin = DateUtility.DateToValue(value);
            }
        }
        [HideInInspector]
        [SerializeField]
        private AxisOrientaion xAxisDirection;


        public AxisOrientaion XAxisDirection
        {
            get { return xAxisDirection; }
            set
            {
                xAxisDirection = value;
                OnParametersChanged();
            }
        }

        [HideInInspector]
        [SerializeField]
        private AxisOrientaion yAxisDirection;

        public AxisOrientaion YAxisDirection
        {
            get { return yAxisDirection; }
            set
            {
                yAxisDirection = value;
                OnParametersChanged();
            }
        }

        [HideInInspector]
        [SerializeField]
        private AxisOrientaion zAxisDirection;

        public AxisOrientaion ZAxisDirection
        {
            get { return ZAxisDirection; }
            set
            {
                ZAxisDirection = value;
                OnParametersChanged();
            }
        }

        public void SetDataBoundries(double minX, double minY, double sizeX, double sizeY)
        {
            automaticHorizontalView = false;
            automaticVerticallView = false;
            horizontalViewOrigin.Value = minX;
            verticalViewOrigin.Value = minY;
            horizontalViewSize.Value = sizeX;
            verticalViewSize.Value = sizeY;
            OnParametersChanged();
        }

    }
}
