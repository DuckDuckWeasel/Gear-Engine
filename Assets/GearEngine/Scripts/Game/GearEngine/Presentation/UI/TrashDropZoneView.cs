using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace GearEngine.GearEngine.Presentation.UI
{
    public sealed class TrashDropZoneView : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("References")]
        public RectTransform ZoneRect => rootPanel;

        [SerializeField] private RectTransform rootPanel;
        [SerializeField] private Image trashIcon;
        [SerializeField] private TextMeshProUGUI rewardLabel;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Animation")]
        [SerializeField] private float fadeInDuration = 0.2f;
        [SerializeField] private float fadeOutDuration = 0.15f;
        [SerializeField] private float hoverScaleMultiplier = 1.2f;

        private bool isShowing;
        private bool isHovered;
        private float animationProgress;
        private Vector3 baseScale;

        public event Action<GearConfigData> OnInventoryGearDropped;

        internal void SetReferences(RectTransform root, Image icon, TextMeshProUGUI label, CanvasGroup cg)
        {
            rootPanel = root;
            trashIcon = icon;
            rewardLabel = label;
            canvasGroup = cg;
        }

        private void Awake()
        {
            if (rootPanel == null)
            {
                rootPanel = GetComponent<RectTransform>();
            }

            baseScale = rootPanel != null ? rootPanel.localScale : Vector3.one;
            animationProgress = 0f;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
        }

        public void OnDragStarted(GearConfigData gearData)
        {
            if (gearData == null)
            {
                Debug.LogWarning("[TrashDropZone] OnDragStarted called with null gearData. Hiding zone.");
                Hide(immediate: false);
                return;
            }

            if (!gearData.IsDeletable)
            {
                Debug.Log($"<color=#ff9900>[TrashDropZone]</color> Gear '{gearData.Id}' is NOT deletable. Hiding trash zone.");
                Hide(immediate: false);
                return;
            }

            Debug.Log($"<color=#00ffff>[TrashDropZone]</color> Gear '{gearData.Id}' IS deletable. Showing trash zone! Reward: {gearData.DeleteRewardAmount}");
            UpdateRewardLabel(gearData.DeleteRewardAmount);
            Show();
        }

        public void OnDragEnded()
        {
            isHovered = false;
            Hide(immediate: false);
        }

        public void SetHovered(bool hovered)
        {
            isHovered = hovered;

            if (trashIcon != null)
            {
                trashIcon.color = hovered ? new Color(1f, 0.4f, 0.4f, 1f) : Color.white;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (isShowing) SetHovered(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetHovered(false);
        }

        /// <summary>
        /// Unity IDropHandler — called when an inventory DragHandler drops onto this zone.
        /// Fires OnInventoryGearDropped and then starts the hide animation.
        /// </summary>
        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null)
            {
                return;
            }

            var slotView = eventData.pointerDrag.GetComponent<GearInventorySlotView>();
            var dragHandler = eventData.pointerDrag.GetComponent<DragHandler>();

            if (dragHandler != null)
            {
                dragHandler.ForceGhostCleanup();
            }

            if (slotView != null && slotView.BoundGearData != null)
            {
                Debug.Log($"<color=#ff5555>[TrashDropZone]</color> Inventory gear '{slotView.BoundGearData.Id}' dropped on trash! Firing event.");
                OnInventoryGearDropped?.Invoke(slotView.BoundGearData);

                // Hide immediately after a successful inventory trash drop
                isHovered = false;
                Hide(immediate: false);
            }
        }

        private void Show()
        {
            isShowing = true;
            gameObject.SetActive(true);
        }

        private void Hide(bool immediate)
        {
            isShowing = false;
            isHovered = false;

            if (immediate)
            {
                animationProgress = 0f;
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 0f;
                }

                gameObject.SetActive(false);
            }
            // When not immediate, TickAutoHideWhenFaded() will SetActive(false) once fade completes.
        }

        private void Update()
        {
            TickFadeAnimation();
            TickScaleAnimation();
            TickAutoHideWhenFaded();
        }

        private void TickFadeAnimation()
        {
            float target = isShowing ? 1f : 0f;
            float speed = isShowing ? (1f / Mathf.Max(fadeInDuration, 0.01f)) : (1f / Mathf.Max(fadeOutDuration, 0.01f));
            animationProgress = Mathf.MoveTowards(animationProgress, target, Time.unscaledDeltaTime * speed);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = animationProgress;
            }
        }

        private void TickScaleAnimation()
        {
            float scaleFactor = isHovered ? hoverScaleMultiplier : 1f;
            Vector3 targetScale = baseScale * Mathf.Lerp(1f, scaleFactor, animationProgress);
            if (rootPanel != null)
            {
                rootPanel.localScale = Vector3.Lerp(rootPanel.localScale, targetScale, Time.unscaledDeltaTime * 12f);
            }
        }

        private void TickAutoHideWhenFaded()
        {
            if (!isShowing && animationProgress <= 0f)
            {
                gameObject.SetActive(false);
            }
        }

        private void UpdateRewardLabel(int rewardAmount)
        {
            if (rewardLabel != null)
            {
                rewardLabel.text = $"+{rewardAmount}";
            }
        }
    }
}
