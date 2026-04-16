using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Scaffold.MVVM;
using UnityEngine.Assertions;

namespace GearEngine.GearEngine.Presentation.UI
{
    public sealed class TrashDropZoneViewComponent : ViewComponent<TrashZoneViewModel>, IDropHandler, IPointerEnterHandler, IPointerExitHandler
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

        protected override void OnBind()
        {
            Assert.IsNotNull(viewModel, "[TrashDropZone] ViewModel is missing.");
            Assert.IsNotNull(rootPanel, "[TrashDropZone] rootPanel is missing.");
            Assert.IsNotNull(trashIcon, "[TrashDropZone] trashIcon is missing.");
            Assert.IsNotNull(rewardLabel, "[TrashDropZone] rewardLabel is missing.");
            Assert.IsNotNull(canvasGroup, "[TrashDropZone] canvasGroup is missing.");
            
            Bind<bool, bool>(() => viewModel.IsActive, OnIsActiveChanged);
            Bind<string, string>(() => viewModel.RewardText, OnRewardTextChanged);
        }

        private void OnIsActiveChanged(bool active)
        {
            if (active)
            {
                Show();
            }
            else
            {
                isHovered = false;
                Hide(immediate: false);
            }
        }

        private void OnRewardTextChanged(string text)
        {
            rewardLabel.text = text;
        }

        public void SetHovered(bool hovered)
        {
            isHovered = hovered;
            trashIcon.color = hovered ? new Color(1f, 0.4f, 0.4f, 1f) : Color.white;
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
        /// Fires HandleGearDropped on the viewModel and starts the hide animation.
        /// </summary>
        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null)
            {
                return;
            }

            var dragHandler = eventData.pointerDrag.GetComponent<DragHandler>();

            if (dragHandler != null)
            {
                dragHandler.ForceGhostCleanup();
            }

            viewModel?.HandleGearDropped();
        }

        private void Show()
        {
            isShowing = true;
            rootPanel.gameObject.SetActive(true);
        }

        private void Hide(bool immediate)
        {
            isShowing = false;
            isHovered = false;

            if (immediate)
            {
                animationProgress = 0f;
                canvasGroup.alpha = 0f;
                rootPanel.gameObject.SetActive(false);
            }
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

            canvasGroup.alpha = animationProgress;
        }

        private void TickScaleAnimation()
        {
            float scaleFactor = isHovered ? hoverScaleMultiplier : 1f;
            Vector3 targetScale = baseScale * Mathf.Lerp(1f, scaleFactor, animationProgress);
            rootPanel.localScale = Vector3.Lerp(rootPanel.localScale, targetScale, Time.unscaledDeltaTime * 12f);
        }

        private void TickAutoHideWhenFaded()
        {
            if (!isShowing && animationProgress <= 0f && rootPanel.gameObject.activeSelf)
            {
                rootPanel.gameObject.SetActive(false);
            }
        }
    }
}
