using System;
using DG.Tweening;
using Scaffold.MVVM;
using UnityEngine;
using UnityEngine.UI;
namespace GearEngine.Campaign.Presentation
{
    public sealed class ResultPopupView : View<ResultPopupViewModel>
    {
        [SerializeField] private RectTransform statsContainer;
        [SerializeField] private ResultStatSlotView statSlotPrefab;
        [SerializeField] private float popDuration = 0.25f;
        [SerializeField] private float stagger = 0.08f;
        [SerializeField] private Ease popEase = Ease.OutBack;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private Button continueButton;

        [Header("Direct UI Binding")]
        [SerializeField] private TMPro.TMP_Text positionText;
        [SerializeField] private TMPro.TMP_Text totalTimeText;
        [SerializeField] private TMPro.TMP_Text rewardText;

        private Sequence statsSequence;

        protected override void OnBind()
        {
            ValidateHierarchy();
            RebuildStatSlots();
            upgradeButton.onClick.AddListener(OnUpgradeClicked);
            continueButton.onClick.AddListener(OnContinueClicked);

            if (positionText != null)
            {
                positionText.text = viewModel.PositionLabel;
            }

            if (totalTimeText != null)
            {
                totalTimeText.text = viewModel.FormattedRaceTime;
            }

            if (rewardText != null)
            {
                rewardText.text = $"{(viewModel.GoldAmount > 0 ? "+" : "")}{viewModel.GoldAmount} Cogs";
            }
        }

        protected override void OnUnbind()
        {
            upgradeButton.onClick.RemoveListener(OnUpgradeClicked);
            continueButton.onClick.RemoveListener(OnContinueClicked);
            KillStatsSequence();
            ClearStatSlots();
            base.OnUnbind();
        }

        private void OnDisable()
        {
            KillStatsSequence();
        }

        private void RebuildStatSlots()
        {
            if (statsContainer == null || statSlotPrefab == null)
            {
                return;
            }

            ClearStatSlots();
            KillStatsSequence();
            statsSequence = DOTween.Sequence();
            RunSpawnStatTweens();
        }

        private void ClearStatSlots()
        {
            if (statsContainer == null) return;

            for (int i = statsContainer.childCount - 1; i >= 0; i--)
            {
                Transform child = statsContainer.GetChild(i);
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }

        private void KillStatsSequence()
        {
            if (statsSequence != null && statsSequence.IsActive())
            {
                statsSequence.Kill();
                statsSequence = null;
            }
        }

        private void RunSpawnStatTweens()
        {
            int index = 0;
            foreach (ResultStatSlotViewModel row in viewModel.Stats)
            {
                AddStatSlotTween(row, index++);
            }
        }

        private void AddStatSlotTween(ResultStatSlotViewModel rowVm, int slotIndex)
        {
            ResultStatSlotView slot = Instantiate(statSlotPrefab, statsContainer);
            slot.gameObject.name = $"StatSlot_{slotIndex}";
            slot.transform.localScale = Vector3.zero;
            slot.Bind(rowVm);
            statsSequence.Insert(
                slotIndex * stagger,
                slot.transform.DOScale(Vector3.one, popDuration).SetEase(popEase));
        }

        private void OnUpgradeClicked()
        {
            try
            {
                viewModel?.Upgrade();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ResultPopupView] OnUpgradeClicked failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void OnContinueClicked()
        {
            try
            {
                viewModel?.Continue();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ResultPopupView] OnContinueClicked failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void ValidateHierarchy()
        {
            RequireReference(upgradeButton, nameof(upgradeButton));
            RequireReference(continueButton, nameof(continueButton));
        }

        private void RequireReference(UnityEngine.Object field, string name)
        {
            if (field == null)
            {
                throw new InvalidOperationException($"[ResultPopupView] {name} reference is missing.");
            }
        }
    }
}
