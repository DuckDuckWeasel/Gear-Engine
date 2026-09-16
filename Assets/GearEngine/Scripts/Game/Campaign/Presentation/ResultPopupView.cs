using System;
using DG.Tweening;
using Scaffold.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.Campaign.Presentation
{
    public sealed class ResultPopupView : View<ResultPopupViewModel>
    {
        [Header("Stages")]
        [SerializeField] private GameObject victoryStage;
        [SerializeField] private GameObject rewardStage;
        [SerializeField] private GameObject progressStage;

        [Header("Victory")]
        [SerializeField] private TMP_Text victoryEyebrowText;
        [SerializeField] private TMP_Text victoryTitleText;
        [SerializeField] private TMP_Text victoryMessageText;
        [SerializeField] private TMP_Text victoryRaceTimeText;
        [SerializeField] private TMP_Text victoryScoreText;
        [SerializeField] private TMP_Text victoryLapText;
        [SerializeField] private TMP_Text victoryRewardText;
        [SerializeField] private Button victoryContinueButton;
        [SerializeField] private Button victoryUpgradeButton;

        [Header("Reward")]
        [SerializeField] private TMP_Text rewardNameText;
        [SerializeField] private TMP_Text rewardCountText;
        [SerializeField] private Image rewardIconImage;
        [SerializeField] private Button rewardContinueButton;

        [Header("Progress")]
        [SerializeField] private TMP_Text progressTitleText;
        [SerializeField] private TMP_Text progressTrackText;
        [SerializeField] private TMP_Text progressSummaryText;
        [SerializeField] private TMP_Text progressScoreText;
        [SerializeField] private TMP_Text progressTimeText;
        [SerializeField] private Image[] progressStars;
        [SerializeField] private Button progressContinueButton;

        [Header("Animation")]
        [SerializeField] private float stageDuration = 0.25f;
        [SerializeField] private Ease stageEase = Ease.OutBack;

        private Sequence stageSequence;

        protected override void OnBind()
        {
            ValidateHierarchy();
            viewModel.StageChanged += ShowStage;
            victoryContinueButton.onClick.AddListener(OnContinueClicked);
            victoryUpgradeButton.onClick.AddListener(OnUpgradeClicked);
            rewardContinueButton.onClick.AddListener(OnContinueClicked);
            progressContinueButton.onClick.AddListener(OnContinueClicked);
            ShowStage(viewModel.CurrentStage);
        }

        protected override void OnUnbind()
        {
            viewModel.StageChanged -= ShowStage;
            victoryContinueButton.onClick.RemoveListener(OnContinueClicked);
            victoryUpgradeButton.onClick.RemoveListener(OnUpgradeClicked);
            rewardContinueButton.onClick.RemoveListener(OnContinueClicked);
            progressContinueButton.onClick.RemoveListener(OnContinueClicked);
            KillStageSequence();
            base.OnUnbind();
        }

        private void OnDisable()
        {
            KillStageSequence();
        }

        private void ShowStage(ResultFlowStage stage)
        {
            ApplyRuntimeData();
            victoryStage.SetActive(stage == ResultFlowStage.Victory);
            rewardStage.SetActive(stage == ResultFlowStage.Reward);
            progressStage.SetActive(stage == ResultFlowStage.Progress);

            GameObject activeStage = stage switch
            {
                ResultFlowStage.Victory => victoryStage,
                ResultFlowStage.Reward => rewardStage,
                ResultFlowStage.Progress => progressStage,
                _ => throw new ArgumentOutOfRangeException(nameof(stage), stage, null),
            };
            PlayStageAnimation(activeStage);
        }

        private void ApplyRuntimeData()
        {
            victoryEyebrowText.text = viewModel.VictoryEyebrow;
            victoryTitleText.text = viewModel.VictoryTitle;
            victoryMessageText.text = viewModel.VictoryMessage;
            victoryRaceTimeText.text = viewModel.FormattedRaceTime;
            victoryScoreText.text = viewModel.Score.ToString("N0");
            victoryLapText.text = viewModel.LapCount.ToString();
            victoryRewardText.text = $"+{viewModel.GoldAmount} GOLD";

            rewardNameText.text = viewModel.RewardName;
            rewardCountText.text = viewModel.RewardCountText;
            rewardIconImage.sprite = viewModel.RewardIcon;
            rewardIconImage.gameObject.SetActive(viewModel.RewardIcon != null);

            progressTitleText.text = viewModel.ProgressTitle;
            progressTrackText.text = viewModel.ProgressTrackName;
            progressSummaryText.text = viewModel.ProgressSummary;
            progressScoreText.text = $"SCORE\n{viewModel.Score:N0}";
            progressTimeText.text = $"RACE TIME\n{viewModel.FormattedRaceTime}";
            ApplyProgressStars();
        }

        private void ApplyProgressStars()
        {
            Color earnedColor = new Color32(255, 210, 63, 255);
            Color unearnedColor = new Color32(73, 92, 108, 255);
            for (int i = 0; i < progressStars.Length; i++)
            {
                progressStars[i].color = i < viewModel.HighestAchievedTier ? earnedColor : unearnedColor;
            }
        }

        private void PlayStageAnimation(GameObject activeStage)
        {
            KillStageSequence();
            CanvasGroup canvasGroup = activeStage.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = activeStage.AddComponent<CanvasGroup>();
            }

            RectTransform rectTransform = activeStage.transform as RectTransform;
            canvasGroup.alpha = 0f;
            rectTransform.localScale = Vector3.one * 0.96f;
            stageSequence = DOTween.Sequence();
            stageSequence.Join(DOTween.To(() => canvasGroup.alpha, value => canvasGroup.alpha = value, 1f, stageDuration));
            stageSequence.Join(rectTransform.DOScale(Vector3.one, stageDuration).SetEase(stageEase));
        }

        private void KillStageSequence()
        {
            if (stageSequence != null && stageSequence.IsActive())
            {
                stageSequence.Kill();
            }

            stageSequence = null;
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
            RequireReference(victoryStage, nameof(victoryStage));
            RequireReference(rewardStage, nameof(rewardStage));
            RequireReference(progressStage, nameof(progressStage));
            RequireReference(victoryContinueButton, nameof(victoryContinueButton));
            RequireReference(victoryUpgradeButton, nameof(victoryUpgradeButton));
            RequireReference(rewardContinueButton, nameof(rewardContinueButton));
            RequireReference(progressContinueButton, nameof(progressContinueButton));
        }

        private static void RequireReference(UnityEngine.Object field, string name)
        {
            if (field == null)
            {
                throw new InvalidOperationException($"[ResultPopupView] {name} reference is missing.");
            }
        }
    }
}
