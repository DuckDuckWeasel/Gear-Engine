using System;
using System.Collections.Generic;

namespace GearEngine.Campaign.Presentation
{
    internal static class StarProgressionMath
    {
        internal static float Fill(IReadOnlyList<int> targets, int index, float score)
        {
            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            if (index < 0 || index >= targets.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            int intervalStart = index == 0 ? 0 : targets[index - 1];
            int intervalEnd = targets[index];
            if (intervalEnd <= intervalStart)
            {
                throw new ArgumentException("Star targets must be positive and strictly increasing.", nameof(targets));
            }

            float progress = (score - intervalStart) / (intervalEnd - intervalStart);
            return Math.Min(1f, Math.Max(0f, progress));
        }
    }

    /// <summary>Race-local presentation state. It never awards points or changes standings.</summary>
    internal sealed class ClubSportHudViewModel
    {
        private readonly int[] targets;
        private readonly bool[] celebrated;
        private int confirmedPosition;
        private int pendingPosition;
        private float confirmationTime;
        private float overtakeTime;

        internal ClubSportHudViewModel(int[] starTargets, int bankedScore)
        {
            targets = (int[])(starTargets ?? throw new ArgumentNullException(nameof(starTargets))).Clone();
            celebrated = new bool[targets.Length];
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] <= 0 || (i > 0 && targets[i] <= targets[i - 1]))
                {
                    throw new ArgumentException("Star targets must be positive and strictly increasing.", nameof(starTargets));
                }
                celebrated[i] = bankedScore >= targets[i];
            }
            BankedScore = Math.Max(0, bankedScore);
        }

        internal int BankedScore { get; private set; }
        internal int StarCount => targets.Length;
        internal int Position { get; private set; }
        internal float OvertakeAlpha => Math.Min(1f, Math.Max(0f, overtakeTime / .2f));

        internal void SetBankedScore(int score) => BankedScore = Math.Max(0, score);

        internal float Fill(int index, float displayedScore)
        {
            return StarProgressionMath.Fill(targets, index, displayedScore);
        }

        internal bool ConsumeCelebration(int index, float displayedScore)
        {
            if (celebrated[index] || BankedScore < targets[index] || displayedScore < targets[index])
            {
                return false;
            }
            celebrated[index] = true;
            return true;
        }

        internal void UpdateStanding(int position, bool running, bool inGrid, bool finished, float deltaTime)
        {
            float elapsed = Math.Max(0f, deltaTime);
            overtakeTime = Math.Max(0f, overtakeTime - elapsed);
            Position = position;
            if (!running || inGrid || finished || position <= 0)
            {
                confirmedPosition = 0;
                pendingPosition = 0;
                confirmationTime = 0f;
                overtakeTime = 0f;
                return;
            }
            if (confirmedPosition == 0 || position >= confirmedPosition)
            {
                confirmedPosition = position;
                pendingPosition = 0;
                confirmationTime = 0f;
                return;
            }
            if (pendingPosition != position)
            {
                pendingPosition = position;
                confirmationTime = 0f;
            }
            confirmationTime += elapsed;
            if (confirmationTime >= .2f)
            {
                confirmedPosition = position;
                pendingPosition = 0;
                confirmationTime = 0f;
                overtakeTime = 1.7f;
            }
        }
    }
}
