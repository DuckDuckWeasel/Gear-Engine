using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.CarSimulation.Definitions
{
    [CreateAssetMenu(menuName = "Game/Track/Track Definition", fileName = "TrackDefinition")]
    public sealed class TrackDefinition : ScriptableObject
    {
        public string TrackName => trackName;

        [SerializeField] private string trackName;

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

        public int EvaluateHighestAchievedTier(float finalRaceTimeSeconds, int finalScore)
        {
            if (!HasConfiguredTiers)
            {
                return 0;
            }

            // Tiers should be ordered 1 to N, assume index 0 is Tier 1, index 1 is Tier 2, etc.
            int highestTier = 0;
            for (int i = 0; i < tiers.Length; i++)
            {
                TrackTierConfig tier = tiers[i];
                if (finalRaceTimeSeconds <= tier.TargetTimeSeconds || finalScore >= tier.TargetScore)
                {
                    highestTier = i + 1;
                }
                else
                {
                    // If we didn't achieve this tier, we don't achieve higher tiers either
                    break;
                }
            }

            return highestTier;
        }

        public int EvaluateTotalGoldReward(float finalRaceTimeSeconds, int finalScore)
        {
            int gold = BaseGoldReward;
            int highestTier = EvaluateHighestAchievedTier(finalRaceTimeSeconds, finalScore);
            
            if (highestTier > 0 && highestTier <= tiers.Length)
            {
                gold += tiers[highestTier - 1].GoldReward;
            }

            return gold;
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
