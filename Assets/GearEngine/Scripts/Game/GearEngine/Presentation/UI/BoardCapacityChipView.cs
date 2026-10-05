using System;
using DG.Tweening;
using Scaffold.MVVM;
using TMPro;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI
{
    public sealed class BoardCapacityChipView : ViewComponent<BoardViewModel>
    {
        public TMP_Text CapacityLabel => ResolveCapacityLabel();

        [SerializeField] private TMP_Text capacityLabel;
        [SerializeField] private float punchDuration = 0.3f;
        [SerializeField] private float punchScale = 0.3f;

        private bool isInitializingBindings;
        private Transform animationTarget;
        private Vector3 baseScale;
        private RectTransform chipRect;
        private Transform originalParent;
        private int originalSiblingIndex;
        private Vector2 originalAnchorMin;
        private Vector2 originalAnchorMax;
        private Vector2 originalPivot;
        private Vector2 originalAnchoredPosition;
        private Vector2 originalSizeDelta;

        public void PlaceAtGridTopLeft(RectTransform gridRect)
        {
            if (gridRect == null)
            {
                throw new ArgumentNullException(nameof(gridRect));
            }

            chipRect = CapacityLabel?.transform.parent as RectTransform;
            if (chipRect == null)
            {
                throw new InvalidOperationException("[BoardCapacityChipView] Capacity chip is missing.");
            }

            if (originalParent == null)
            {
                originalParent = chipRect.parent;
                originalSiblingIndex = chipRect.GetSiblingIndex();
                originalAnchorMin = chipRect.anchorMin;
                originalAnchorMax = chipRect.anchorMax;
                originalPivot = chipRect.pivot;
                originalAnchoredPosition = chipRect.anchoredPosition;
                originalSizeDelta = chipRect.sizeDelta;
            }

            chipRect.SetParent(gridRect, false);
            chipRect.SetAsLastSibling();
            chipRect.anchorMin = new Vector2(0f, 1f);
            chipRect.anchorMax = chipRect.anchorMin;
            chipRect.pivot = new Vector2(0f, 0f);
            chipRect.anchoredPosition = new Vector2(24f, 12f);
        }

        public void SetVisible(bool visible)
        {
            RectTransform target = ResolveCapacityLabel()?.transform.parent as RectTransform;
            if (target != null)
            {
                target.gameObject.SetActive(visible);
            }
        }

        public new void Unbind()
        {
            base.Unbind();
        }

        protected override void OnBind()
        {
            capacityLabel = ResolveCapacityLabel();
            if (capacityLabel == null)
            {
                Debug.LogError("[BoardCapacityChipView] Capacity label is missing.");
                return;
            }

            animationTarget = capacityLabel.transform.parent;
            baseScale = animationTarget.localScale;
            isInitializingBindings = true;
            Bind<string, string>(() => viewModel.BoardCapacityText, UpdateCapacityText);
            Bind<int, int>(() => viewModel.CapacityFeedbackRevision, OnCapacityFeedbackChanged);
            isInitializingBindings = false;
            UpdateCapacityText(viewModel.BoardCapacityText);
        }

        protected override void OnUnbind()
        {
            animationTarget?.DOKill(complete: true);
            if (animationTarget != null)
            {
                animationTarget.localScale = baseScale;
            }

            SetVisible(false);
            RestoreHeaderPlacement();
            base.OnUnbind();
        }

        private void RestoreHeaderPlacement()
        {
            if (chipRect == null || originalParent == null)
            {
                return;
            }

            chipRect.SetParent(originalParent, false);
            chipRect.SetSiblingIndex(originalSiblingIndex);
            chipRect.anchorMin = originalAnchorMin;
            chipRect.anchorMax = originalAnchorMax;
            chipRect.pivot = originalPivot;
            chipRect.anchoredPosition = originalAnchoredPosition;
            chipRect.sizeDelta = originalSizeDelta;
            originalParent = null;
            chipRect = null;
        }

        private void UpdateCapacityText(string value)
        {
            if (capacityLabel != null)
            {
                capacityLabel.text = value;
            }
        }

        private void OnCapacityFeedbackChanged(int _)
        {
            if (isInitializingBindings)
            {
                return;
            }

            animationTarget.DOKill(complete: true);
            animationTarget.localScale = baseScale;
            animationTarget
                .DOPunchScale(Vector3.one * punchScale, punchDuration, 10, 1f)
                .SetUpdate(isIndependentUpdate: true);
        }

        private TMP_Text ResolveCapacityLabel()
        {
            if (capacityLabel != null)
            {
                return capacityLabel;
            }

            TMP_Text[] labels = GetComponentsInChildren<TMP_Text>(includeInactive: true);
            for (int i = 0; i < labels.Length; i++)
            {
                TMP_Text candidate = labels[i];
                if (IsCogCapacityLabel(candidate))
                {
                    return candidate;
                }
            }

            return labels.Length == 1 ? labels[0] : null;
        }

        private bool IsCogCapacityLabel(TMP_Text candidate)
        {
            return candidate != null &&
                candidate.transform.parent != null &&
                candidate.transform.parent.name == "chips_cogs";
        }
    }
}
