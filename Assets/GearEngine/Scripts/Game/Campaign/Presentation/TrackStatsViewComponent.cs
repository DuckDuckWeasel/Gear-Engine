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

        [Header("Tiers")]
        [SerializeField] private int maxDisplayTiers = 3;
        [SerializeField] private RectTransform tiersContainer;
        [SerializeField] private List<TierSlotPrefabConfig> tierSlotPrefabs = new List<TierSlotPrefabConfig>();
        [SerializeField] private TrackTierSlotView defaultTierSlotPrefab;
        [SerializeField] private float popDuration = 0.25f;
        [SerializeField] private float stagger = 0.08f;
        [SerializeField] private Ease popEase = Ease.OutBack;

        private Sequence tiersSequence;

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

            RebuildTierSlots();
        }

        private void OnDisable()
        {
            if (tiersSequence != null && tiersSequence.IsActive())
            {
                tiersSequence.Kill();
                tiersSequence = null;
            }
        }

        private void RebuildTierSlots()
        {
            if (tiersContainer == null)
            {
                Debug.LogError("[TrackStatsView] tiersContainer missing; cannot render tiers.");
                return;
            }

            ClearTierSlots();
            TryKillTiersSequence();
            tiersSequence = DOTween.Sequence();
            RunSpawnTierTweens();
        }

        private void ClearTierSlots()
        {
            for (int i = tiersContainer.childCount - 1; i >= 0; i--)
            {
                Transform child = tiersContainer.GetChild(i);
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

        private void TryKillTiersSequence()
        {
            if (tiersSequence != null && tiersSequence.IsActive())
            {
                tiersSequence.Kill();
            }
        }

        private void RunSpawnTierTweens()
        {
            int slotIndex = 0;
            foreach (TrackTierViewModel tierVm in viewModel.Tiers)
            {
                if (maxDisplayTiers > 0 && slotIndex >= maxDisplayTiers)
                {
                    break;
                }
                AddTierSlotTween(tierVm, slotIndex++);
            }
        }

        private void AddTierSlotTween(TrackTierViewModel tierVm, int slotIndex)
        {
            TrackTierSlotView prefabToInstantiate = defaultTierSlotPrefab;
            if (tierSlotPrefabs != null)
            {
                foreach (var config in tierSlotPrefabs)
                {
                    if (config.Contains(tierVm.TierNumber))
                    {
                        prefabToInstantiate = config.Prefab;
                        break;
                    }
                }
            }

            if (prefabToInstantiate == null)
            {
                Debug.LogError($"[TrackStatsView] No prefab configured for tier {tierVm.TierNumber} and default prefab is missing.");
                return;
            }

            TrackTierSlotView slot = Instantiate(prefabToInstantiate, tiersContainer);
            slot.gameObject.name = $"TierSlot_{tierVm.TierNumber}";
            slot.transform.localScale = Vector3.zero;
            slot.Bind(tierVm);
            tiersSequence.Insert(slotIndex * stagger, slot.transform.DOScale(Vector3.one, popDuration).SetEase(popEase));
        }
    }
}
