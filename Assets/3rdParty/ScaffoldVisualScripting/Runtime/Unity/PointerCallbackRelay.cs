using UnityEngine;
using UnityEngine.EventSystems;

namespace Scaffold.VisualScripting.Unity
{
    public sealed class PointerCallbackRelay : BlackboardCallbackRelay, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private string clickMessage = "PointerClick";
        [SerializeField] private string downMessage = "PointerDown";
        [SerializeField] private string upMessage = "PointerUp";
        [SerializeField] private string enterMessage = "PointerEnter";
        [SerializeField] private string exitMessage = "PointerExit";
        [SerializeField] private string beginDragMessage = "BeginDrag";
        [SerializeField] private string dragMessage = "Drag";
        [SerializeField] private string endDragMessage = "EndDrag";

        public void OnPointerClick(PointerEventData eventData)
        {
            Forward(clickMessage, eventData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Forward(downMessage, eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Forward(upMessage, eventData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Forward(enterMessage, eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Forward(exitMessage, eventData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Forward(beginDragMessage, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Forward(dragMessage, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Forward(endDragMessage, eventData);
        }
    }
}
