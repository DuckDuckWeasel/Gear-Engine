using System;
using UnityEngine;

namespace GearEngine.CarSimulation.Definitions
{
    [Serializable]
    public sealed class TrackScoreBand
    {
        public TrackScoreBand()
        {
        }

        public TrackScoreBand(float maxRaceTimeSeconds, int rewardValue)
        {
            this.maxRaceTimeSeconds = maxRaceTimeSeconds;
            this.rewardValue = rewardValue;
        }

        public float MaxRaceTimeSeconds => maxRaceTimeSeconds;

        [SerializeField] private float maxRaceTimeSeconds;

        public int RewardValue => rewardValue;

        [SerializeField] private int rewardValue;
    }
}
