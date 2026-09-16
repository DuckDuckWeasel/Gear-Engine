using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiveOps.Modules.DTO.ModuleRequests;
using GearEngine.CarSimulation.Definitions;
using UnityEngine;

namespace GearEngine.Campaign
{
    public sealed class RaceResultModel
    {
        private static int ScoreThresholdToAdvance => 500;
        private static int LegacyGoldPerScorePoint => 5;

        public RaceResultModel(float raceTime, int lapCount, TrackDefinition track, int driftScore = 0, float? previousRaceTime = null)
        {
            if (raceTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(raceTime));
            }

            RaceTime = raceTime;
            LapCount = lapCount;
            Score = driftScore; // Score is now strictly drift score
            TrackName = track != null ? track.GetDisplayName() : string.Empty;
            Tiers = track != null ? track.Tiers.Where(tier => tier != null).OrderBy(tier => tier.TargetScore).ToArray() : Array.Empty<TrackTierConfig>();
            Standings = new RaceStandingsModel(raceTime, track, previousRaceTime);

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
                Gold = new GoldReward(legacyScore * LegacyGoldPerScorePoint);
                IsGoodResult = legacyScore >= ScoreThresholdToAdvance;
            }
        }

        public RaceStandingsModel Standings { get; }
        public bool HasGearReward => Standings.PlayerPosition == 1 || HighestAchievedTier > 0;

        public float RaceTime { get; }
        public int LapCount { get; }
        public int Score { get; }
        public int HighestAchievedTier { get; }
        public GoldReward Gold { get; }
        public bool IsGoodResult { get; }

        public string TrackName { get; }
        public IReadOnlyList<TrackTierConfig> Tiers { get; }

        public Task PersistenceCompleted { get; private set; } = Task.CompletedTask;

        public RecordRaceResultResponse ServerOutcome { get; set; }

        private TaskCompletionSource<bool> persistenceCompletion;



        internal void BeginPersistence()
        {
            persistenceCompletion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            PersistenceCompleted = persistenceCompletion.Task;
        }

        internal void CompletePersistence()
        {
            persistenceCompletion?.TrySetResult(true);
        }
        private int ComputeLegacyScore(float raceTime, int lapCount)
        {
            float perLap = lapCount > 0 ? raceTime / lapCount : raceTime;
            return Mathf.Max(0, 1000 - Mathf.RoundToInt(perLap * 10f));
        }
    }
}
