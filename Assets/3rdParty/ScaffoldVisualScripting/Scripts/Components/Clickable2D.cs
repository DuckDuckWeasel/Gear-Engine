using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Scaffold
{
    public sealed class Clickable2D :
        MonoBehaviour,
        IPointerClickHandler,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        [SerializeField] private bool clickEnabled = true;
        [SerializeField] private Texture2D hoverCursor;
        [SerializeField] private UnityEvent clicked = new UnityEvent();

        public event Action Clicked;

        public bool ClickEnabled
        {
            get => clickEnabled;
            set => clickEnabled = value;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!clickEnabled)
            {
                return;
            }

            clicked.Invoke();
            Clicked?.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (clickEnabled)
            {
                Cursor.SetCursor(hoverCursor, Vector2.zero, CursorMode.Auto);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }
}
