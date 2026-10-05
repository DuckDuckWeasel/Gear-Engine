using System;
using System.Linq;
using DG.Tweening;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.Campaign.Presentation
{
    /// <summary>Production HUD binding. Race/session objects remain the only gameplay authorities.</summary>
    public sealed class ClubSportHudView : MonoBehaviour
    {
        private static readonly Color32 s_normalLapColor = new Color32(248, 239, 220, 255);
        private static readonly Color32 s_finalLapColor = new Color32(255, 204, 0, 255);
        private static readonly Color32 s_finishedLapColor = new Color32(239, 70, 78, 255);
        private static readonly Color32 s_inkColor = new Color32(53, 65, 74, 255);
        [SerializeField] private ClubSportHudLayout layout;
        [SerializeField] private TMP_Text positionText;
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text lapsText;
        [SerializeField] private TMP_Text totalText;
        [SerializeField] private TMP_Text pointsText;
        [SerializeField] private TMP_Text multiplierText;
        [SerializeField] private Image multiplierBadge;
        [SerializeField] private CanvasGroup combo;
        [SerializeField] private CanvasGroup overtake;
        [SerializeField] private ClubSportStarView[] stars = Array.Empty<ClubSportStarView>();
        [SerializeField] private Button pulseButton;
        [SerializeField] private Button turboButton;
        [SerializeField] private bool reducedMotion;
        private static readonly Color32[] s_tiers =
        {
            s_inkColor,
            new Color32(255, 102, 105, 255),
            new Color32(255, 213, 64, 255),
            new Color32(79, 119, 112, 255),
            s_finishedLapColor
        };
        private RaceState session;
        private RaceDriftScoreViewModel drift;
        private ClubSportHudViewModel state;
        private float displayedScore;
        private Tween scoreTween;
        private Tween numberPunch;
        private Tween multiplierPunch;
        private int lastScore = -1;
        private int lastMultiplier = -1;
        private int lastPoints = -1;
        private int lastTime = -1;
        private int lastLap = -1;
        private int lastPosition = -1;
        private int racerCount = 4;

        internal ClubSportHudLayout Layout => layout;
        internal bool ReducedMotion => reducedMotion;
        internal ClubSportHudViewModel State => state;
        internal bool ControlsDisabled => !pulseButton.interactable && !turboButton.interactable;

        internal void Bind(RaceState race, RaceDriftScoreViewModel driftScore)
        {
            Unbind();
            session = race ?? throw new ArgumentNullException(nameof(race));
            drift = driftScore ?? throw new ArgumentNullException(nameof(driftScore));
            int[] targets = race.Track.Tiers.Where(tier => tier != null).OrderBy(tier => tier.TargetScore)
                .Take(stars.Length).Select(tier => tier.TargetScore).ToArray();
            state = new ClubSportHudViewModel(targets, race.TotalDriftScore);
            displayedScore = race.TotalDriftScore;
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].gameObject.SetActive(i < targets.Length);
            }
            pulseButton.interactable = turboButton.interactable = false;
            layout.Refresh(true);
            RenderScore(false);
            Tick(0f);
        }

        internal void Tick(float deltaTime)
        {
            if (session == null)
            {
                return;
            }
            UpdateClockAndLap();
            UpdateCombo();
            UpdateBankedScore();
            overtake.alpha = state.OvertakeAlpha;
        }

        internal void UpdateStanding(int position, int count, bool inGrid, bool finished, float deltaTime)
        {
            if (state == null)
            {
                return;
            }
            racerCount = count;
            state.UpdateStanding(position, session.Phase == SimulationLifecycleState.Running, inGrid, finished, deltaTime);
            if (lastPosition != position)
            {
                positionText.text = $"{position}<size=50%>/{racerCount}</size>";
                lastPosition = position;
            }
        }

        private void UpdateClockAndLap()
        {
            int centiseconds = Mathf.Max(0, Mathf.FloorToInt(session.RaceTime * 100f));
            if (lastTime != centiseconds)
            {
                timeText.text = $"{centiseconds / 6000:00}:{centiseconds / 100 % 60:00}.{centiseconds % 100:00}";
                lastTime = centiseconds;
            }
            int totalLaps = Mathf.Max(1, session.TotalLaps);
            int lap = Mathf.Clamp(session.CurrentLap + 1, 1, totalLaps);
            if (lastLap != lap)
            {
                lapsText.text = $"{lap}<size=60%>/{session.TotalLaps}</size>";
                lastLap = lap;
            }

            lapsText.color = session.CurrentLap >= totalLaps
                ? s_finishedLapColor
                : lap >= totalLaps ? s_finalLapColor : s_normalLapColor;
        }

        private void UpdateCombo()
        {
            combo.alpha = drift.IsDisplayingScore ? 1f : 0f;
            if (lastPoints != drift.DisplayPoints)
            {
                pointsText.text = $"+{drift.DisplayPoints}";
                lastPoints = drift.DisplayPoints;
            }
            if (lastMultiplier == drift.CurrentMultiplier)
            {
                return;
            }
            bool initialize = lastMultiplier < 0;
            lastMultiplier = drift.CurrentMultiplier;
            multiplierText.text = $"{lastMultiplier}x";
            int tier = Mathf.Clamp(lastMultiplier - 1, 0, s_tiers.Length - 1);
            Color tierColor = s_tiers[tier];
            multiplierBadge.color = tierColor;
            multiplierText.color = tier == 2 ? s_inkColor : s_normalLapColor;
            if (!initialize && !reducedMotion && Application.isPlaying)
            {
                PlayMultiplierFeedback(tierColor);
            }
        }

        private void PlayMultiplierFeedback(Color tierColor)
        {
            multiplierPunch?.Kill();
            Transform badgeTransform = multiplierBadge.transform;
            badgeTransform.localScale = Vector3.one;
            multiplierBadge.color = tierColor;
            Sequence sequence = DOTween.Sequence().SetTarget(this);
            sequence.Join(badgeTransform.DOPunchScale(Vector3.one * 0.18f, 0.34f, 6, 0.65f));
            sequence.Join(DOTween.To(
                    () => multiplierBadge.color,
                    value => multiplierBadge.color = value,
                    Color.Lerp(tierColor, Color.white, 0.3f),
                    0.1f)
                .SetLoops(2, LoopType.Yoyo));
            multiplierPunch = sequence;
        }

        private void UpdateBankedScore()
        {
            int banked = session.TotalDriftScore;
            state.SetBankedScore(banked);
            bool finished = session.Phase == SimulationLifecycleState.Completed;
            if (finished || reducedMotion)
            {
                scoreTween?.Kill();
                displayedScore = banked;
                RenderScore(!finished);
                lastScore = banked;
                return;
            }
            if (lastScore == banked)
            {
                return;
            }
            bool initialize = lastScore < 0;
            lastScore = banked;
            scoreTween?.Kill();
            if (initialize || !Application.isPlaying)
            {
                displayedScore = banked;
                RenderScore(false);
                return;
            }
            numberPunch?.Kill();
            totalText.transform.localScale = Vector3.one;
            numberPunch = totalText.transform.DOPunchScale(Vector3.one * .07f, .28f, 1, .5f);
            scoreTween = DOTween.To(() => displayedScore, value =>
            {
                displayedScore = value;
                RenderScore(true);
            }, banked, .68f).SetEase(Ease.OutCubic).SetTarget(this);
        }

        private void RenderScore(bool celebrate)
        {
            totalText.text = Mathf.RoundToInt(displayedScore).ToString("N0", System.Globalization.CultureInfo.InvariantCulture);
            int count = 0;
            for (int i = 0; i < state.StarCount; i++)
            {
                stars[i].SetFill(state.Fill(i, displayedScore));
                if (celebrate && state.ConsumeCelebration(i, displayedScore))
                {
                    stars[i].Celebrate(count++ * .09f, reducedMotion);
                }
            }
        }

        internal void SetReducedMotion(bool enabled)
        {
            reducedMotion = enabled;
            if (enabled)
            {
                scoreTween?.Kill();
                if (state != null) { displayedScore = state.BankedScore; RenderScore(true); }
                numberPunch?.Kill();
                multiplierPunch?.Kill();
                totalText.transform.localScale = Vector3.one;
                multiplierBadge.transform.localScale = Vector3.one;
                foreach (ClubSportStarView star in stars)
                {
                    star.ResetEffect();
                }
            }
        }

        internal void Unbind()
        {
            scoreTween?.Kill();
            numberPunch?.Kill();
            multiplierPunch?.Kill();
            scoreTween = numberPunch = multiplierPunch = null;
            session = null;
            drift = null;
            state = null;
            lastScore = lastMultiplier = lastPoints = lastTime = lastLap = lastPosition = -1;
            if (totalText != null)
            {
                totalText.transform.localScale = Vector3.one;
            }
            if (multiplierBadge != null)
            {
                multiplierBadge.transform.localScale = Vector3.one;
            }
            if (overtake != null)
            {
                overtake.alpha = 0f;
            }
            foreach (ClubSportStarView star in stars)
            {
                star.ResetEffect();
            }
        }

        private void OnDisable() => Unbind();
    }
}
