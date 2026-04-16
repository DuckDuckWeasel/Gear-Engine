using System;
using UnityEngine;

namespace GearEngine.CarSimulation.Definitions
{
    [Serializable]
    public struct RaceScoreBracket : IEquatable<RaceScoreBracket>
    {
        [Tooltip("Display / LiveOps tier id (e.g. Gold).")]
        [SerializeField]
        private string rankId;

        [Tooltip("Player must finish at or under this time (seconds) to qualify for this tier.")]
        [SerializeField]
        private float timeToBeatSeconds;

        [SerializeField]
        private int goldReward;

        public RaceScoreBracket(string rankId, float timeToBeatSeconds, int goldReward)
        {
            this.rankId = rankId;
            this.timeToBeatSeconds = timeToBeatSeconds;
            this.goldReward = goldReward;
        }

        public string RankId => rankId;

        public float TimeToBeatSeconds => timeToBeatSeconds;

        public int GoldReward => goldReward;

        public bool Equals(RaceScoreBracket other)
        {
            return string.Equals(rankId, other.rankId, StringComparison.Ordinal)
                   && timeToBeatSeconds.Equals(other.timeToBeatSeconds)
                   && goldReward == other.goldReward;
        }

        public override bool Equals(object obj)
        {
            return obj is RaceScoreBracket other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = rankId != null ? StringComparer.Ordinal.GetHashCode(rankId) : 0;
                hashCode = (hashCode * 397) ^ timeToBeatSeconds.GetHashCode();
                hashCode = (hashCode * 397) ^ goldReward;
                return hashCode;
            }
        }
    }
}
