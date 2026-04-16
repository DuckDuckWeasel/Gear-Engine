using System;
using GearEngine.CarSimulation.Definitions;

namespace GearEngine.Race.Rewards
{
    public readonly struct RaceRewardEvaluation : IEquatable<RaceRewardEvaluation>
    {
        public RaceRewardEvaluation(bool matchedBracket, string rankId, int goldReward, float finishTimeSeconds, int lapsCompleted, int lapsRequired)
        {
            MatchedBracket = matchedBracket;
            RankId = rankId ?? string.Empty;
            GoldReward = goldReward;
            FinishTimeSeconds = finishTimeSeconds;
            LapsCompleted = lapsCompleted;
            LapsRequired = lapsRequired;
        }

        public bool MatchedBracket { get; }

        public string RankId { get; }

        public int GoldReward { get; }

        public float FinishTimeSeconds { get; }

        public int LapsCompleted { get; }

        public int LapsRequired { get; }

        public bool Equals(RaceRewardEvaluation other)
        {
            return MatchedBracket == other.MatchedBracket
                   && string.Equals(RankId, other.RankId, StringComparison.Ordinal)
                   && GoldReward == other.GoldReward
                   && FinishTimeSeconds.Equals(other.FinishTimeSeconds)
                   && LapsCompleted == other.LapsCompleted
                   && LapsRequired == other.LapsRequired;
        }

        public override bool Equals(object obj)
        {
            return obj is RaceRewardEvaluation other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = MatchedBracket.GetHashCode();
                hashCode = (hashCode * 397) ^ (RankId != null ? StringComparer.Ordinal.GetHashCode(RankId) : 0);
                hashCode = (hashCode * 397) ^ GoldReward;
                hashCode = (hashCode * 397) ^ FinishTimeSeconds.GetHashCode();
                hashCode = (hashCode * 397) ^ LapsCompleted;
                hashCode = (hashCode * 397) ^ LapsRequired;
                return hashCode;
            }
        }
    }

    public static class RaceRewardEvaluator
    {
        public static RaceRewardEvaluation Evaluate(TrackDefinition track, float finishTimeSeconds, int lapsCompleted)
        {
            if (track == null)
            {
                throw new ArgumentNullException(nameof(track));
            }

            int lapsRequired = Math.Max(1, track.TotalLaps);
            if (lapsCompleted < lapsRequired)
            {
                return new RaceRewardEvaluation(false, string.Empty, 0, finishTimeSeconds, lapsCompleted, lapsRequired);
            }

            RaceScoreBracket[] brackets = track.ScoreBrackets;
            if (brackets == null || brackets.Length == 0)
            {
                return new RaceRewardEvaluation(false, string.Empty, 0, finishTimeSeconds, lapsCompleted, lapsRequired);
            }

            RaceScoreBracket? best = null;
            float bestTime = float.NegativeInfinity;

            for (int i = 0; i < brackets.Length; i++)
            {
                RaceScoreBracket b = brackets[i];
                if (finishTimeSeconds <= b.TimeToBeatSeconds && b.TimeToBeatSeconds >= bestTime)
                {
                    bestTime = b.TimeToBeatSeconds;
                    best = b;
                }
            }

            if (!best.HasValue)
            {
                return new RaceRewardEvaluation(false, string.Empty, 0, finishTimeSeconds, lapsCompleted, lapsRequired);
            }

            RaceScoreBracket chosen = best.Value;
            string rank = string.IsNullOrEmpty(chosen.RankId) ? "Tier" : chosen.RankId;
            return new RaceRewardEvaluation(true, rank, chosen.GoldReward, finishTimeSeconds, lapsCompleted, lapsRequired);
        }
    }
}
