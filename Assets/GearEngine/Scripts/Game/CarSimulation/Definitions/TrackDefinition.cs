using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.CarSimulation.Definitions
{
    [CreateAssetMenu(menuName = "Game/Track/Track Definition", fileName = "TrackDefinition")]
    public sealed class TrackDefinition : ScriptableObject
    {
        public string TrackName => trackName;

        [SerializeField] private string trackName;

        public TrackThemeDefinition Theme => theme;

        [SerializeField] private TrackThemeDefinition theme;

        public IReadOnlyList<RaceOpponentConfig> Opponents => opponents;
        [SerializeField] private RaceOpponentConfig[] opponents = Array.Empty<RaceOpponentConfig>();

        public float Scale => scale;

        [SerializeField] private float scale = 1f;

        public Vector3 Offset => offset;

        [SerializeField] private Vector3 offset = Vector3.zero;

        public int TotalLaps => totalLaps;

        [SerializeField] private int totalLaps = 3;

        public float TimeToBeatSeconds => timeToBeatSeconds;

        [SerializeField] private float timeToBeatSeconds = 60f;

        public Spline Spline => spline;

        [SerializeField] private Spline spline = new Spline();

        public int BaseGoldReward => baseGoldReward;

        [SerializeField] private int baseGoldReward = 10;

        public bool HasConfiguredTiers => tiers != null && tiers.Length > 0;

        public IReadOnlyList<TrackTierConfig> Tiers => tiers;

        [SerializeField] private TrackTierConfig[] tiers = Array.Empty<TrackTierConfig>();

        public string GetDisplayName()
        {
            return string.IsNullOrEmpty(trackName) ? name : trackName;
        }

        public int EvaluateTotalGoldReward(float finalRaceTimeSeconds, int finalScore)
        {
            int gold = BaseGoldReward;
            int highestTier = EvaluateHighestAchievedTier(finalRaceTimeSeconds, finalScore);

            if (highestTier > 0 && highestTier <= tiers.Length)
            {
                gold += tiers.Where(tier => tier != null).OrderBy(tier => tier.TargetScore).ElementAt(highestTier - 1).GoldReward;
            }

            return gold;
        }

        public int EvaluateHighestAchievedTier(float finalRaceTimeSeconds, int finalScore)
        {
            if (!HasConfiguredTiers)
            {
                return 0;
            }

            // Score alone awards stars. Sort thresholds because older assets store them in reverse order.
            int highestTier = tiers.Where(tier => tier != null).Count(tier => finalScore >= tier.TargetScore);

            return highestTier;
        }

        internal void SetTotalLapsForTests(int value)
        {
            totalLaps = value;
        }

        internal void SetTiersForTests(TrackTierConfig[] testTiers)
        {
            tiers = testTiers ?? Array.Empty<TrackTierConfig>();
        }

    }
}
