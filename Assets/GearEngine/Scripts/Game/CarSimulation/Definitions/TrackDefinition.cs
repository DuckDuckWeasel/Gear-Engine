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

        public int TotalLaps => totalLaps;

        [SerializeField] private int totalLaps = 3;

        public float TimeToBeatSeconds => timeToBeatSeconds;

        [SerializeField] private float timeToBeatSeconds = 60f;

        public Spline Spline => spline;

        [SerializeField] private Spline spline = new Spline();

        public bool HasConfiguredScoreBands => scoreBands != null && scoreBands.Length > 0;

        public IReadOnlyList<TrackScoreBand> ScoreBands => scoreBands;

        [SerializeField] private TrackScoreBand[] scoreBands = Array.Empty<TrackScoreBand>();

        public string GetDisplayName()
        {
            return string.IsNullOrEmpty(trackName) ? name : trackName;
        }

        public int EvaluateRewardForTotalRaceTime(float totalRaceTimeSeconds)
        {
            if (!HasConfiguredScoreBands)
            {
                return 0;
            }

            TrackScoreBand[] ordered = new TrackScoreBand[scoreBands.Length];
            Array.Copy(scoreBands, ordered, scoreBands.Length);
            Array.Sort(ordered, (a, b) => a.MaxRaceTimeSeconds.CompareTo(b.MaxRaceTimeSeconds));

            foreach (TrackScoreBand band in ordered)
            {
                if (totalRaceTimeSeconds <= band.MaxRaceTimeSeconds)
                {
                    return band.RewardValue;
                }
            }

            return 0;
        }

        public int EvaluatePositionForTotalRaceTime(float totalRaceTimeSeconds)
        {
            if (!HasConfiguredScoreBands)
            {
                return 1;
            }

            TrackScoreBand[] ordered = new TrackScoreBand[scoreBands.Length];
            Array.Copy(scoreBands, ordered, scoreBands.Length);
            Array.Sort(ordered, (a, b) => a.MaxRaceTimeSeconds.CompareTo(b.MaxRaceTimeSeconds));

            for (int i = 0; i < ordered.Length; i++)
            {
                if (totalRaceTimeSeconds <= ordered[i].MaxRaceTimeSeconds)
                {
                    return i + 1;
                }
            }

            return ordered.Length + 1; // Last place if failed all bands
        }

        internal void SetTotalLapsForTests(int value)
        {
            totalLaps = value;
        }

        internal void SetScoreBandsForTests(TrackScoreBand[] bands)
        {
            scoreBands = bands ?? Array.Empty<TrackScoreBand>();
        }
    }
}
