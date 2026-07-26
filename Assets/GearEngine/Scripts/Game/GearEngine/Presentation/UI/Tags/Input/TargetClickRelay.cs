using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GearEngine.Actions.Input
{
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    public sealed class TargetClickRelay : MonoBehaviour, IPointerDownHandler, IPointerClickHandler
    {
        private event Action clicked;

        public void AddListener(Action listener)
        {
            if (listener == null)
            {
                throw new ArgumentNullException(nameof(listener));
            }

            clicked -= listener;
            clicked += listener;
        }

        public void RemoveListener(Action listener)
        {
            if (listener != null)
            {
                clicked -= listener;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            NotifyClicked(eventData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            NotifyClicked(eventData);
        }

        private void NotifyClicked(PointerEventData eventData)
        {
            if (eventData == null)
            {
                throw new ArgumentNullException(nameof(eventData));
            }

            clicked?.Invoke();
        }
    }
}
