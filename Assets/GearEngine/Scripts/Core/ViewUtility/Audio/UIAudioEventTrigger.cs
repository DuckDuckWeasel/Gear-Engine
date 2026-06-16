using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Ami.BroAudio;

namespace GearEngine.Core.ViewUtility.Audio
{
    /// <summary>
    /// A unified UI Audio Trigger that works similarly to Unity's EventTrigger but is optimized for BroAudio.
    /// It automatically respects the interactable state if a Button is present.
    /// </summary>
    [AddComponentMenu("UI/Audio/UI Audio Event Trigger")]
    public class UIAudioEventTrigger : MonoBehaviour, 
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler,
        IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler,
        IScrollHandler, IUpdateSelectedHandler, ISelectHandler, IDeselectHandler, IMoveHandler,
        ISubmitHandler, ICancelHandler
    {
        [SerializeField] private List<AudioEventTriggerEntry> _triggers = new List<AudioEventTriggerEntry>();

        private Button _button;

        private void Awake()
        {
            // Cache the button if it exists to respect its interactable state
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(OnButtonClick);
            }
        }

        private void OnDisable()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnButtonClick);
            }
        }

        private void OnButtonClick()
        {
            PlaySound(EventTriggerType.PointerClick);
        }

        private void PlaySound(EventTriggerType eventID)
        {
            // If we have a button, only play the click sound if the button is interactable.
            // OnButtonClick only fires if interactable, but we want to ensure other events
            // don't bypass logic if needed. For now, just handling PointerClick interactable state.
            if (_button != null && !_button.interactable && eventID == EventTriggerType.PointerClick)
            {
                return;
            }

            foreach (var entry in _triggers)
            {
                if (entry.eventID == eventID && entry.sound.IsValid())
                {
                    BroAudio.Play(entry.sound);
                }
            }
        }

        // --- Event System Interface Implementations ---

        public void OnPointerEnter(PointerEventData eventData) => PlaySound(EventTriggerType.PointerEnter);
        public void OnPointerExit(PointerEventData eventData) => PlaySound(EventTriggerType.PointerExit);
        public void OnPointerDown(PointerEventData eventData) => PlaySound(EventTriggerType.PointerDown);
        public void OnPointerUp(PointerEventData eventData) => PlaySound(EventTriggerType.PointerUp);
        
        public void OnPointerClick(PointerEventData eventData)
        {
            // If there's a Button, we ignore this because the Button's onClick will trigger OnButtonClick
            if (_button == null)
            {
                PlaySound(EventTriggerType.PointerClick);
            }
        }

        public void OnInitializePotentialDrag(PointerEventData eventData) => PlaySound(EventTriggerType.InitializePotentialDrag);
        public void OnBeginDrag(PointerEventData eventData) => PlaySound(EventTriggerType.BeginDrag);
        public void OnDrag(PointerEventData eventData) => PlaySound(EventTriggerType.Drag);
        public void OnEndDrag(PointerEventData eventData) => PlaySound(EventTriggerType.EndDrag);
        public void OnDrop(PointerEventData eventData) => PlaySound(EventTriggerType.Drop);
        public void OnScroll(PointerEventData eventData) => PlaySound(EventTriggerType.Scroll);
        public void OnUpdateSelected(BaseEventData eventData) => PlaySound(EventTriggerType.UpdateSelected);
        public void OnSelect(BaseEventData eventData) => PlaySound(EventTriggerType.Select);
        public void OnDeselect(BaseEventData eventData) => PlaySound(EventTriggerType.Deselect);
        public void OnMove(AxisEventData eventData) => PlaySound(EventTriggerType.Move);
        public void OnSubmit(BaseEventData eventData) => PlaySound(EventTriggerType.Submit);
        public void OnCancel(BaseEventData eventData) => PlaySound(EventTriggerType.Cancel);
    }
}
