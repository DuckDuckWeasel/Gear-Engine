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
            base.OnUnbind();
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
