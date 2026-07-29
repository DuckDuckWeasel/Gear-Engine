using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Scaffold
{
    public sealed class Draggable2D :
        MonoBehaviour,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        [SerializeField] private bool dragEnabled = true;
        [FormerlySerializedAs("returnToStartPos")]
        [SerializeField] private bool returnOnCancelled = true;
        [SerializeField] private bool returnOnCompleted = true;
        [SerializeField] private float returnDuration = 1f;
        [SerializeField] private Texture2D hoverCursor;
        [SerializeField] private UnityEvent dragStarted = new UnityEvent();
        [SerializeField] private UnityEvent dragCompleted = new UnityEvent();

        public event Action DragStarted;
        public event Action DragCompleted;

        public bool BeingDragged { get; private set; }

        public bool DragEnabled
        {
            get => dragEnabled;
            set => dragEnabled = value;
        }

        private Vector3 startingPosition;
        private Vector3 pointerOffset;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!dragEnabled)
            {
                return;
            }

            BeingDragged = true;
            startingPosition = transform.position;
            pointerOffset = GetPointerWorldPosition(eventData) - transform.position;
            dragStarted.Invoke();
            DragStarted?.Invoke();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragEnabled && BeingDragged)
            {
                Vector3 nextPosition = GetPointerWorldPosition(eventData) - pointerOffset;
                nextPosition.z = transform.position.z;
                transform.position = nextPosition;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!BeingDragged)
            {
                return;
            }

            BeingDragged = false;
            dragCompleted.Invoke();
            DragCompleted?.Invoke();
            if (returnOnCompleted || returnOnCancelled)
            {
                LeanTween.move(gameObject, startingPosition, returnDuration)
                    .setEase(LeanTweenType.easeOutExpo);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (dragEnabled)
            {
                Cursor.SetCursor(hoverCursor, Vector2.zero, CursorMode.Auto);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        private static Vector3 GetPointerWorldPosition(PointerEventData eventData)
        {
            Camera eventCamera = eventData.pressEventCamera ?? Camera.main;
            if (eventCamera == null)
            {
                throw new InvalidOperationException(
                    "Draggable2D requires a camera to convert pointer positions.");
            }

            return eventCamera.ScreenToWorldPoint(
                new Vector3(
                    eventData.position.x,
                    eventData.position.y,
                    Math.Abs(eventCamera.transform.position.z)));
        }
    }
}
