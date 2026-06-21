using System;
using System.Collections.Generic;
using DG.Tweening;
using Scaffold.MVVM;
using TMPro;
using UnityEngine;

namespace GearEngine.Campaign.Presentation
{
    public sealed class TrackStatsViewComponent : ViewComponent<TrackStatsViewModel>
    {
        [SerializeField] private TextMeshProUGUI trackNameLabel;
        [SerializeField] private TextMeshProUGUI targetLapsLabel;
        [SerializeField] private TextMeshProUGUI targetTimeLabel;

        [Header("Score Bands")]
        [SerializeField] private int maxDisplayScores = 3;
        [SerializeField] private RectTransform bandsContainer;
        [SerializeField] private List<BandSlotPrefabConfig> bandSlotPrefabs = new List<BandSlotPrefabConfig>();
        [SerializeField] private TrackScoreBandSlotView defaultBandSlotPrefab;
        [SerializeField] private float popDuration = 0.25f;
        [SerializeField] private float stagger = 0.08f;
        [SerializeField] private Ease popEase = Ease.OutBack;

        private Sequence bandsSequence;

        protected override void OnBind()
        {
            base.OnBind();

            if (trackNameLabel != null)
            {
                trackNameLabel.text = viewModel.TrackName;
            }

            if (targetLapsLabel != null)
            {
                string lapsText = viewModel.TargetLaps < 0 ? "Laps: —" : $"Laps: {viewModel.TargetLaps}";
                targetLapsLabel.text = lapsText;
            }

            if (targetTimeLabel != null)
            {
                System.TimeSpan time = System.TimeSpan.FromSeconds(viewModel.TargetTime);
                targetTimeLabel.text = $"Target: {(int)time.TotalSeconds:00}:{time:ff}";
            }

            RebuildBandSlots();
        }

        private void OnDisable()
        {
            if (bandsSequence != null && bandsSequence.IsActive())
            {
                bandsSequence.Kill();
                bandsSequence = null;
            }
        }

        private void RebuildBandSlots()
        {
            if (bandsContainer == null)
            {
                Debug.LogError("[TrackStatsView] bandsContainer missing; cannot render score bands.");
                return;
            }

            ClearBandSlots();
            TryKillBandsSequence();
            bandsSequence = DOTween.Sequence();
            RunSpawnBandTweens();
        }

        private void ClearBandSlots()
        {
            for (int i = bandsContainer.childCount - 1; i >= 0; i--)
            {
                Transform child = bandsContainer.GetChild(i);
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

        private void TryKillBandsSequence()
        {
            if (bandsSequence != null && bandsSequence.IsActive())
            {
                bandsSequence.Kill();
            }
        }

        private void RunSpawnBandTweens()
        {
            int slotIndex = 0;
            foreach (TrackScoreBandViewModel bandVm in viewModel.ScoreBands)
            {
                if (maxDisplayScores > 0 && slotIndex >= maxDisplayScores)
                {
                    break;
                }
                AddBandSlotTween(bandVm, slotIndex++);
            }
        }

        private void AddBandSlotTween(TrackScoreBandViewModel bandVm, int slotIndex)
        {
            TrackScoreBandSlotView prefabToInstantiate = defaultBandSlotPrefab;
            if (bandSlotPrefabs != null)
            {
                foreach (var config in bandSlotPrefabs)
                {
                    if (config.Contains(bandVm.Position))
                    {
                        prefabToInstantiate = config.Prefab;
                        break;
                    }
                }
            }

            if (prefabToInstantiate == null)
            {
                Debug.LogError($"[TrackStatsView] No prefab configured for position {bandVm.Position} and default prefab is missing.");
                return;
            }

            TrackScoreBandSlotView slot = Instantiate(prefabToInstantiate, bandsContainer);
            slot.gameObject.name = $"BandSlot_{bandVm.Position}";
            slot.transform.localScale = Vector3.zero;
            slot.Bind(bandVm);
            bandsSequence.Insert(slotIndex * stagger, slot.transform.DOScale(Vector3.one, popDuration).SetEase(popEase));
        }
    }
}
