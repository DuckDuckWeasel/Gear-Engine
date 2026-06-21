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

        public RaceResultModel(float raceTime, int lapCount, TrackDefinition track)
        {
            if (raceTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(raceTime));
            }

            RaceTime = raceTime;
            LapCount = lapCount;

            if (track != null && track.HasConfiguredScoreBands)
            {
                Score = track.EvaluateRewardForTotalRaceTime(raceTime);
                Position = track.EvaluatePositionForTotalRaceTime(raceTime);
                Gold = new GoldReward(Score);
            }
            else
            {
                Score = ComputeLegacyScore(raceTime, lapCount);
                Position = 1;
                Gold = new GoldReward(Score * legacyGoldPerScorePoint);
            }

            IsGoodResult = Score >= scoreThresholdToAdvance;
        }

        public float RaceTime { get; }
        public int LapCount { get; }
        public int Score { get; }
        public int Position { get; }
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
