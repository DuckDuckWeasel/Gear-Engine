using System;
using Scaffold.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.Campaign.Presentation
{
    public sealed class ResultPopupView : View<ResultPopupViewModel>
    {
        [SerializeField] private TextMeshProUGUI raceTimeLabel;
        [SerializeField] private TextMeshProUGUI lapCountLabel;
        [SerializeField] private TextMeshProUGUI scoreLabel;
        [SerializeField] private TextMeshProUGUI goldLabel;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private Button continueButton;

        protected override void OnBind()
        {
            ValidateHierarchy();
            ApplyResultLabels();
            upgradeButton.onClick.AddListener(OnUpgradeClicked);
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        protected override void OnUnbind()
        {
            upgradeButton.onClick.RemoveListener(OnUpgradeClicked);
            continueButton.onClick.RemoveListener(OnContinueClicked);
            base.OnUnbind();
        }

        private void ApplyResultLabels()
        {
            ApplyRaceTimingLabels();
            ApplyScoreAndGoldLabels();
        }

        private void ApplyRaceTimingLabels()
        {
            if (raceTimeLabel != null)
            {
                raceTimeLabel.text = $"{viewModel.RaceTime:F2}s";
            }

            if (lapCountLabel != null)
            {
                lapCountLabel.text = $"Laps: {viewModel.LapCount}";
            }
        }

        private void ApplyScoreAndGoldLabels()
        {
            if (scoreLabel != null)
            {
                scoreLabel.text = $"Score: {viewModel.Score}";
            }

            if (goldLabel != null)
            {
                goldLabel.text = $"+{viewModel.GoldAmount} gold (total: {viewModel.CurrentGold})";
            }
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
