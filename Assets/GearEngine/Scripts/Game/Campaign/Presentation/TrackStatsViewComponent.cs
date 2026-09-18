using System;
using System.Collections.Generic;
using DG.Tweening;
using Scaffold.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.Campaign.Presentation
{
    public sealed class TrackStatsViewComponent : ViewComponent<TrackStatsViewModel>
    {
        private static readonly Color s_starScoreColor = new Color32(44, 57, 69, 255);

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

        [Header("Best Times")]
        [SerializeField] private ResultStandingsView standings;
        [SerializeField] private Image[] earnedStars = Array.Empty<Image>();
        [SerializeField] private Sprite earnedStar;
        [SerializeField] private Sprite emptyStar;

        private Sequence tiersSequence;
        private RectTransform earnedStarsContainer;
        private TextMeshProUGUI[] starTargetLabels = Array.Empty<TextMeshProUGUI>();

        protected override void OnBind()
        {
            base.OnBind();
            PositionEarnedStarsInTrackHeader();

            if (trackNameLabel != null)
            {
                trackNameLabel.text = viewModel.TrackName;
            }

            if (targetLapsLabel != null)
            {
                targetLapsLabel.gameObject.SetActive(false);
            }

            if (targetTimeLabel != null)
            {
                targetTimeLabel.gameObject.SetActive(false);
            }

            if (standings != null)
            {
                standings.Bind(viewModel.Standings);
                standings.ShowCurrentPlacement();
            }
            for (int i = 0; i < earnedStars.Length; i++)
            {
                earnedStars[i].sprite = i < viewModel.EarnedStars ? earnedStar : emptyStar;
            }
            UpdateStarTargetScores();
            RebuildTierSlots();
        }

        public void SetLocked(bool _)
        {
            if (viewModel == null)
            {
                return;
            }

            if (trackNameLabel != null)
            {
                trackNameLabel.text = viewModel.TrackName;
            }

            for (int i = 0; i < earnedStars.Length; i++)
            {
                earnedStars[i].sprite = i < viewModel.EarnedStars ? earnedStar : emptyStar;
            }

            UpdateStarTargetScores();
        }

        private void PositionEarnedStarsInTrackHeader()
        {
            if (earnedStars.Length == 0 || earnedStars[0] == null)
            {
                return;
            }

            earnedStarsContainer ??= earnedStars[0].rectTransform.parent as RectTransform;
            RectTransform headerRoot = transform as RectTransform;
            if (earnedStarsContainer == null || headerRoot == null)
            {
                return;
            }

            if (earnedStarsContainer.parent != headerRoot)
            {
                earnedStarsContainer.SetParent(headerRoot, false);
            }

            earnedStarsContainer.anchorMin = new Vector2(0.5f, 1f);
            earnedStarsContainer.anchorMax = earnedStarsContainer.anchorMin;
            earnedStarsContainer.anchoredPosition = new Vector2(0f, -450f);
            earnedStarsContainer.sizeDelta = new Vector2(360f, 140f);
            earnedStarsContainer.localScale = Vector3.one;
            earnedStarsContainer.SetAsLastSibling();

            if (trackNameLabel != null)
            {
                RectTransform title = trackNameLabel.rectTransform;
                title.anchoredPosition = new Vector2(title.anchoredPosition.x, -100f);
            }
        }

        private void UpdateStarTargetScores()
        {
            EnsureStarTargetLabels();
            for (int i = 0; i < starTargetLabels.Length; i++)
            {
                bool hasTarget = i < viewModel.StarTargetScores.Count;
                starTargetLabels[i].gameObject.SetActive(hasTarget);
                starTargetLabels[i].text = hasTarget ? viewModel.StarTargetScores[i].ToString() : string.Empty;
            }
        }

        private void EnsureStarTargetLabels()
        {
            if (starTargetLabels.Length == earnedStars.Length)
            {
                return;
            }

            starTargetLabels = new TextMeshProUGUI[earnedStars.Length];
            for (int i = 0; i < earnedStars.Length; i++)
            {
                starTargetLabels[i] = CreateStarTargetLabel(earnedStars[i], i + 1);
            }
        }

        private TextMeshProUGUI CreateStarTargetLabel(Image star, int starNumber)
        {
            GameObject labelObject = new GameObject(
                $"Star{starNumber}TargetScore",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));
            RectTransform rect = labelObject.GetComponent<RectTransform>();
            rect.SetParent(star.rectTransform, false);
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = rect.anchorMin;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, -36f);
            rect.sizeDelta = new Vector2(120f, 40f);

            TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
            label.font = trackNameLabel.font;
            label.fontSize = 24f;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.color = s_starScoreColor;
            label.raycastTarget = false;
            return label;
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
                foreach (TierSlotPrefabConfig config in tierSlotPrefabs)
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
