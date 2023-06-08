using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DataVisualizer
{
    public class CanvasInteractionManager : Graphic,IPointerEnterHandler,IPointerExitHandler,IPointerDownHandler,IPointerUpHandler,ICancelHandler,IPointerClickHandler
    {
        public Vector2 PointerPosition { get; private set; }
        public bool IsPointerInside { get; private set; }
        public bool IsPointerDown { get; private set; }

        public event Action OnPointerClick;

        public void OnCancel(BaseEventData eventData)
        {
            
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            IsPointerDown = true;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            IsPointerInside = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            IsPointerInside = false;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            IsPointerDown = false;
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
        public override bool Raycast(Vector2 sp, Camera eventCamera)
        {
            Vector2 position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, sp, eventCamera, out position);
            PointerPosition = position;            
            return true;
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (OnPointerClick != null)
                OnPointerClick();
        }
    }
}
