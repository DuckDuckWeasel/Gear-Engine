using System;
using UnityEngine;

namespace GearEngine.CarSimulation.Definitions
{
    [Serializable]
    public sealed class TrackTierConfig
    {
        public TrackTierConfig()
        {
        }

        public TrackTierConfig(float targetTimeSeconds, int targetScore, int goldReward)
        {
            this.targetTimeSeconds = targetTimeSeconds;
            this.targetScore = targetScore;
            this.goldReward = goldReward;
        }

        public float TargetTimeSeconds => targetTimeSeconds;
        [SerializeField] private float targetTimeSeconds;

        public int TargetScore => targetScore;
        [SerializeField] private int targetScore;

        public int GoldReward => goldReward;
        [SerializeField] private int goldReward;
    }
}
