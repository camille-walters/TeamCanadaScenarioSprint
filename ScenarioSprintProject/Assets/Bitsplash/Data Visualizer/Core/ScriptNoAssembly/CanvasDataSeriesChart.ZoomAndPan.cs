using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif
using UnityEngine.UI;

namespace DataVisualizer
{
    public partial class CanvasDataSeriesChart
    {
        Vector2 mZoomBasePosition;
        DoubleVector2 mZoomBaseChartSpace;
        DoubleVector3 InitalScrolling;
        DoubleVector3 InitalViewSize;
        DoubleVector3 InitalViewDirection;
        DoubleVector3 InitialOrigin;
        float totalZoom = 0;
        public float ZoomSpeed = 20f;
        Vector2 GetPointerPosition()
        {
#if ENABLE_INPUT_SYSTEM 
            if(Pointer.current == null)
                return new Vector2();
            return Pointer.current.position.ReadValue();
#else
            return Input.mousePosition;
#endif
        }
        float GetMouseDelta()
        {
#if ENABLE_INPUT_SYSTEM
            if(Mouse.current == null)
                return 0.0f;
            return Mouse.current.scroll.y.ReadValue()/100f;
#else
            return Input.mouseScrollDelta.y;
#endif
        }
        bool IsPointerDown()
        {
#if ENABLE_INPUT_SYSTEM
            if(Pointer.current == null)
                return false;
            return Pointer.current.press.isPressed;
#else
            return Input.GetMouseButton(0);
#endif
        }

        bool CompareWithError(Vector3 a, Vector3 b)
        {
            const float errorMargin = 5f;
            if (Mathf.Abs(a.x - b.x) > errorMargin)
                return false;
            if (Mathf.Abs(a.y - b.y) > errorMargin)
                return false;
            return true;
        }

        private void HandleDrag()
        {
            if (Axis.View.VerticalPanning == false && Axis.View.HorizontalPanning == false)
                return;
            mCaster = GetComponentInParent<GraphicRaycaster>();
            if (mCaster == null)
                return;
            Vector2 mousePos;
            Vector2 checkMousePos = GetPointerPosition();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, checkMousePos, mCaster.eventCamera, out mousePos);
            var cam = mCaster.eventCamera;
            bool mouseIn = RectTransformUtility.RectangleContainsScreenPoint(transform as RectTransform, checkMousePos, cam);
            if (IsPointerDown() && mouseIn)
            {
                if (mLastPosition.HasValue)
                {
                    Vector2 delta = mousePos - mLastPosition.Value;
                    MouseDraged(delta);
                }
                mLastPosition = mousePos;
            }
            else
                mLastPosition = null;
        }
        private void MouseDraged(Vector2 delta)
        {

            if (Axis.View.VerticalPanning)
            {
                double range = Axis.ChartSpaceView.Height;
                Axis.View.VerticalScrolling -= (delta.y / LocalFitPortion.Height) * range;
            }

            if (Axis.View.HorizontalPanning)
            {
                double range = Axis.ChartSpaceView.Width;
                Axis.View.HorizontalScrolling -= (delta.x / LocalFitPortion.Width) * range;
            }
        }
        void ResetZoomAnchor()
        {
            totalZoom = 0;
            InitalScrolling = new DoubleVector3(Axis.View.HorizontalScrolling, Axis.View.VerticalScrolling);
            InitalViewSize = new DoubleVector3(Axis.View.HorizontalViewSize, Axis.View.VerticalViewSize);
            InitalViewDirection = new DoubleVector3(Math.Sign(InitalViewSize.x), Math.Sign(InitalViewSize.y));
            InitialOrigin = new DoubleVector3(Axis.View.HorizontalViewOrigin, Axis.View.VerticalViewOrigin);

        }

        private void HandleZoom()
        {
            mCaster = GetComponentInParent<GraphicRaycaster>();
            if (mCaster == null)
                return;
            Vector2 mousePos;
            Vector2 checkMousePos = GetPointerPosition();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, checkMousePos, mCaster.eventCamera, out mousePos);
            var cam = mCaster.eventCamera;
            bool mouseIn = RectTransformUtility.RectangleContainsScreenPoint(transform as RectTransform, checkMousePos, cam);
            if (mouseIn == false)
                return;
            DoubleVector2 chartPos = RectTransformSpaceToChartSpace(mousePos);
            if (CompareWithError(mousePos, mZoomBasePosition) == false) // the mouse has moved beyond the erroo
            {
                mZoomBasePosition = mousePos;
                mZoomBaseChartSpace = chartPos;
                ResetZoomAnchor();
            }
            else
            {
                mousePos = mZoomBasePosition;
            }
            float delta = GetMouseDelta();
            totalZoom += delta;    //accumilate the delta change for the currnet positions

            if (delta != 0)
            {
                DoubleVector3 ViewCenter = InitialOrigin + InitalScrolling;
                DoubleVector3 trans = new DoubleVector3((mZoomBaseChartSpace.x - ViewCenter.x), (mZoomBaseChartSpace.y - ViewCenter.y));
                float growFactor = Mathf.Pow(2, totalZoom / ZoomSpeed);
                double hSize = InitalViewSize.x * growFactor;
                double vSize = InitalViewSize.y * growFactor;
                //if (hSize * InitalViewDirection.x < MaxViewSize && hSize * InitalViewDirection.x > MinViewSize && vSize * InitalViewDirection.y < MaxViewSize && vSize * InitalViewDirection.y > MinViewSize)
                //{
                if (Axis.View.VerticalZooming)
                {
                    Axis.View.VerticalScrolling = InitalScrolling.y + trans.y - (trans.y * growFactor);
                    Axis.View.VerticalViewSize = vSize;
                }
                if (Axis.View.HorizontalZooming)
                {
                    Axis.View.HorizontalScrolling = InitalScrolling.x + trans.x - (trans.x * growFactor);
                    Axis.View.HorizontalViewSize = hSize;
                }
            }
        }
    }
}
