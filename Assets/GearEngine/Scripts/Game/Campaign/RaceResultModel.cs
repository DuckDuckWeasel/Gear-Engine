using System;
using UnityEngine;

namespace GearEngine.Campaign
{
    public sealed class RaceResultModel
    {
        private const int scoreThresholdToAdvance = 500;

        public RaceResultModel(float raceTime, int lapCount)
        {
            if (raceTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(raceTime));
            }

            RaceTime = raceTime;
            LapCount = lapCount;
            Score = ComputeScore(raceTime, lapCount);
            Gold = new GoldReward(Score);
            IsGoodResult = Score >= scoreThresholdToAdvance;
        }

        public float RaceTime { get; }
        public int LapCount { get; }
        public int Score { get; }
        public GoldReward Gold { get; }
        public bool IsGoodResult { get; }

        private int ComputeScore(float raceTime, int lapCount)
        {
            float perLap = lapCount > 0 ? raceTime / lapCount : raceTime;
            return Mathf.Max(0, 1000 - Mathf.RoundToInt(perLap * 10f));
        }
    }
}
