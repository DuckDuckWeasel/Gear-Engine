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
        [SerializeField] private TMPro.TMP_Text raceTimeText;
        [SerializeField] private RawImage celebrationPattern;
        [SerializeField] private Vector2 celebrationPatternSpeed = new Vector2(0.08f, 0.04f);
        [SerializeField] private ParticleSystem celebrationParticles;

        private Sequence statsSequence;
        private bool celebrationActive;

        protected override void OnBind()
        {
            ValidateHierarchy();
            ResetCelebrationPattern();
            PlayCelebration();
            celebrationActive = true;

            if (raceTimeText != null)
            {
                raceTimeText.text = viewModel.FormattedRaceTime;
            }

            RebuildStatSlots();
            upgradeButton.onClick.AddListener(OnUpgradeClicked);
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        protected override void OnUnbind()
        {
            upgradeButton.onClick.RemoveListener(OnUpgradeClicked);
            continueButton.onClick.RemoveListener(OnContinueClicked);
            KillStatsSequence();
            ClearStatSlots();
            StopCelebration();
            celebrationActive = false;
            base.OnUnbind();
        }

        private void OnDisable()
        {
            KillStatsSequence();
            StopCelebration();
            celebrationActive = false;
        }

        private void Update()
        {
            if (!celebrationActive)
            {
                return;
            }

            Rect uvRect = celebrationPattern.uvRect;
            Vector2 offset = uvRect.position + celebrationPatternSpeed * Time.unscaledDeltaTime;
            uvRect.position = new Vector2(Mathf.Repeat(offset.x, 1f), Mathf.Repeat(offset.y, 1f));
            celebrationPattern.uvRect = uvRect;
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

        private void ResetCelebrationPattern()
        {
            Rect uvRect = celebrationPattern.uvRect;
            uvRect.position = Vector2.zero;
            celebrationPattern.uvRect = uvRect;
        }

        private void PlayCelebration()
        {
            celebrationParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            celebrationParticles.Play(true);
        }

        private void StopCelebration()
        {
            if (celebrationParticles == null)
            {
                return;
            }

            celebrationParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void ValidateHierarchy()
        {
            RequireReference(upgradeButton, nameof(upgradeButton));
            RequireReference(continueButton, nameof(continueButton));
            RequireReference(celebrationPattern, nameof(celebrationPattern));
            RequireReference(celebrationParticles, nameof(celebrationParticles));
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
