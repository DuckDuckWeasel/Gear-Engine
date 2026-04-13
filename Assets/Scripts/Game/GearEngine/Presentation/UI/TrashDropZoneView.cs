using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace Game.GearEngine.Presentation
{
    /// <summary>
    /// Trash drop zone anchored to the top-right of the overlay Canvas.
    /// Appears when a deletable gear is being dragged and shows the reward amount.
    /// UI hierarchy is built by <see cref="TrashDropZoneFactory"/>.
    /// </summary>
    public sealed class TrashDropZoneView : MonoBehaviour, IDropHandler
    {
        public event Action<GearConfigData> OnInventoryGearDropped;
        [Header("References")]
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

        /// <summary>The RectTransform used for overlap detection by the drag handler.</summary>
        public RectTransform ZoneRect => rootPanel;

        /// <summary>
        /// Called by <see cref="TrashDropZoneFactory"/> to wire serialized fields after creation.
        /// </summary>
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

            // Initialize visual state only — don't call Hide() here because Awake
            // fires on the first SetActive(true) from Show(), and calling
            // SetActive(false) inside Awake would immediately undo the Show().
            animationProgress = 0f;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
        }

        /// <summary>
        /// Called when a drag starts. Shows the zone if the gear is deletable.
        /// </summary>
        public void OnDragStarted(GearConfigData gearData)
        {
            if (gearData == null || !gearData.IsDeletable)
            {
                Hide(immediate: false);
                return;
            }

            UpdateRewardLabel(gearData.DeleteRewardAmount);
            Show();
        }

        /// <summary>
        /// Called when a drag ends (drop, cancel, etc). Hides the zone.
        /// </summary>
        public void OnDragEnded()
        {
            isHovered = false;
            Hide(immediate: false);
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
        }

        private void Update()
        {
            float target = isShowing ? 1f : 0f;
            float speed = isShowing ? (1f / Mathf.Max(fadeInDuration, 0.01f)) : (1f / Mathf.Max(fadeOutDuration, 0.01f));
            animationProgress = Mathf.MoveTowards(animationProgress, target, Time.unscaledDeltaTime * speed);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = animationProgress;
            }

            // Scale for hover feedback
            float scaleFactor = isHovered ? hoverScaleMultiplier : 1f;
            Vector3 targetScale = baseScale * Mathf.Lerp(1f, scaleFactor, animationProgress);
            if (rootPanel != null)
            {
                rootPanel.localScale = Vector3.Lerp(rootPanel.localScale, targetScale, Time.unscaledDeltaTime * 12f);
            }

            // Auto-hide when fully faded out
            if (!isShowing && animationProgress <= 0f)
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>Called externally when the pointer enters the trash zone during a drag.</summary>
        public void SetHovered(bool hovered)
        {
            isHovered = hovered;

            if (trashIcon != null)
            {
                trashIcon.color = hovered
                    ? new Color(1f, 0.4f, 0.4f, 1f)
                    : Color.white;
            }
        }

        private void UpdateRewardLabel(int rewardAmount)
        {
            if (rewardLabel != null)
            {
                rewardLabel.text = $"+{rewardAmount}";
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag != null)
            {
                var slotView = eventData.pointerDrag.GetComponent<GearInventorySlotView>();
                var dragHandler = eventData.pointerDrag.GetComponent<DragHandler>();

                // Force ghost cleanup immediately before the UI list rebuild destroys the DragHandler.
                // This resolves EventSystem race conditions deterministically without needing a frame delay.
                if (dragHandler != null)
                {
                    dragHandler.ForceGhostCleanup();
                }

                if (slotView != null && slotView.BoundGearData != null)
                {
                    OnInventoryGearDropped?.Invoke(slotView.BoundGearData);
                }
            }
        }
    }
}
