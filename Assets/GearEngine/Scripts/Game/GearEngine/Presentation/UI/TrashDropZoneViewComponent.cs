using GearEngine.GearEngine;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Services;
using Scaffold.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GearEngine.GearEngine.Presentation.UI
{
    public sealed class TrashDropZoneViewComponent : ViewComponent<TrashZoneViewModel>, IDragTarget, IDragLifecycleListener, IPointerEnterHandler, IPointerExitHandler
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

        private IDragService dragService;

        public void SetDragService(IDragService service)
        {
            dragService = service;
        }

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
            Assert.IsNotNull(dragService, "[TrashDropZone] IDragService is missing. Call SetDragService before Bind.");

            dragService.Register((IDragLifecycleListener)this);

            Bind<bool, bool>(() => viewModel.IsActive, OnIsActiveChanged);
            Bind<string, string>(() => viewModel.RewardText, OnRewardTextChanged);
        }

        protected override void OnUnbind()
        {
            dragService?.Unregister((IDragLifecycleListener)this);

            base.OnUnbind();
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

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (isShowing)
            {
                SetHovered(true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetHovered(false);
        }

        public void SetHovered(bool hovered)
        {
            isHovered = hovered;
            trashIcon.color = hovered ? new Color(1f, 0.4f, 0.4f, 1f) : Color.white;
        }

        void IDragLifecycleListener.OnDragStarted(DragPayload payload)
        {
            viewModel?.HandleDragStarted(payload);

            if (viewModel != null && viewModel.IsActive)
            {
                Show();
            }
            else
            {
                isHovered = false;
                Hide(immediate: false);
            }
        }

        void IDragLifecycleListener.OnDragEnded()
        {
            viewModel?.HandleDragEnded();
            if (viewModel == null || !viewModel.IsActive)
            {
                isHovered = false;
                Hide(immediate: false);
            }
        }

        public bool CanAccept(DragPayload payload)
        {
            GearItemData gear = payload.GetData<GearItemData>() ?? payload.GetData<IGridNode>()?.ConfigData;
            return gear != null && viewModel != null && viewModel.CanTrashAcceptGear(gear);
        }

        public bool OnDrop(DragPayload payload)
        {
            IGridNode node = payload.GetData<IGridNode>();
            GearItemData gear = payload.GetData<GearItemData>();
            if (node != null)
            {
                return viewModel != null && viewModel.HandleBoardGearDropped(node);
            }

            if (gear != null)
            {
                return viewModel != null && viewModel.HandleInventoryGearDropped(gear);
            }

            return false;
        }

        private void Show()
        {
            isShowing = true;
            rootPanel.gameObject.SetActive(true);
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
                canvasGroup.alpha = 1f;
                animationProgress = 1f;
            }
        }

        private void Update()
        {
            TickFadeAnimation();
            TickScaleAnimation();
        }

        private void TickFadeAnimation()
        {
            if (isShowing)
            {
                animationProgress = 1f;
                canvasGroup.alpha = 1f;
            }
            else
            {
                float target = 0f;
                float speed = 1f / Mathf.Max(fadeOutDuration, 0.01f);
                animationProgress = Mathf.MoveTowards(animationProgress, target, Time.unscaledDeltaTime * speed);
                canvasGroup.alpha = animationProgress;
            }
        }

        private void TickScaleAnimation()
        {
            float scaleFactor = isHovered ? hoverScaleMultiplier : 1f;
            Vector3 targetScale = baseScale * Mathf.Lerp(1f, scaleFactor, animationProgress);
            rootPanel.localScale = Vector3.Lerp(rootPanel.localScale, targetScale, Time.unscaledDeltaTime * 12f);
        }

        public void ApplyInitialState()
        {
            if (viewModel == null)
            {
                Debug.LogError("[TrashDropZone] ApplyInitialState called before Bind.");
                return;
            }

            if (IsTrashDisabled())
            {
                return;
            }
            Assert.IsNotNull(rootPanel, "[TrashDropZone] rootPanel is not assigned. Trash deletion will not work.");
            Hide(immediate: true);
        }

        private bool IsTrashDisabled()
        {
            GearEngineFeatureToggleSO featureToggle = viewModel.FeatureToggleForTrashPlacement;
            if (featureToggle == null || featureToggle.EnableTrashDeletion)
            {
                return false;
            }
            if (rootPanel != null)
            {
                rootPanel.gameObject.SetActive(false);
            }
            return true;
        }

        private void Hide(bool immediate)
        {
            isShowing = false;
            isHovered = false;
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
            if (immediate)
            {
                HideImmediately();
            }
        }

        private void HideImmediately()
        {
            animationProgress = 0f;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
        }
    }
}
