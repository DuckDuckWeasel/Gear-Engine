using System;
using LiveOps.Modules.DTO.ModuleRequests;
using GearEngine.CarSimulation.Definitions;
using UnityEngine;

namespace GearEngine.Campaign
{
    public sealed class RaceResultModel
    {
        private const int scoreThresholdToAdvance = 500;
        private const int legacyGoldPerScorePoint = 5;

        public RaceResultModel(float raceTime, int lapCount, TrackDefinition track, int driftScore = 0)
        {
            if (raceTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(raceTime));
            }

            RaceTime = raceTime;
            LapCount = lapCount;
            Score = driftScore; // Score is now strictly drift score

            if (track != null && track.HasConfiguredTiers)
            {
                HighestAchievedTier = track.EvaluateHighestAchievedTier(raceTime, driftScore);
                int totalGold = track.EvaluateTotalGoldReward(raceTime, driftScore);
                Gold = new GoldReward(totalGold);
                IsGoodResult = HighestAchievedTier > 0;
            }
            else
            {
                HighestAchievedTier = 0;
                int legacyScore = ComputeLegacyScore(raceTime, lapCount) + driftScore;
                Gold = new GoldReward(legacyScore * legacyGoldPerScorePoint);
                IsGoodResult = legacyScore >= scoreThresholdToAdvance;
            }
        }

        public float RaceTime { get; }
        public int LapCount { get; }
        public int Score { get; }
        public int HighestAchievedTier { get; }
        public GoldReward Gold { get; }
        public bool IsGoodResult { get; }

        /// <summary>Populated after <see cref="Services.ITrackService.RecordResultAsync"/> when using LiveOps.</summary>
        public RecordRaceResultResponse ServerOutcome { get; set; }

        private static int ComputeLegacyScore(float raceTime, int lapCount)
        {
            float perLap = lapCount > 0 ? raceTime / lapCount : raceTime;
            return Mathf.Max(0, 1000 - Mathf.RoundToInt(perLap * 10f));
        }
    }
}
