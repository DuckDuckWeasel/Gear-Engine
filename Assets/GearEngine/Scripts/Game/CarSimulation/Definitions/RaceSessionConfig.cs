using System;
using UnityEngine;

namespace GearEngine.CarSimulation.Definitions
{
    [Serializable]
    public sealed class RaceSessionConfig
    {
        public int TotalLaps => totalLaps;

        public RoguelikeCarStats RoguelikeStats => roguelikeStats;

        [SerializeField] private int totalLaps = 3;

        [SerializeField] private RoguelikeCarStats roguelikeStats = RoguelikeCarStats.Default;

        internal void SetTotalLapsForTests(int value)
        {
            totalLaps = value;
        }

        /// <summary>Copies lap count from the track when the track defines it.</summary>
        public void ApplyFromTrack(TrackDefinition track)
        {
            if (track == null)
            {
                return;
            }

            totalLaps = track.TotalLaps;
        }

        /// <summary>Creates a copy for a new race session (template values from campaign scope, etc.).</summary>
        public RaceSessionConfig CloneForNewRace()
        {
            var copy = new RaceSessionConfig();
            copy.totalLaps = totalLaps;
            copy.roguelikeStats = roguelikeStats;
            return copy;
        }
    }
}
