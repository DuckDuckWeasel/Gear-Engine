using System;
using UnityEngine;

namespace GearEngine.CarSimulation.Definitions
{
    [Serializable]
    public sealed class RaceSessionConfig
    {
        public CarVariableSet Variables => variables;

        public int TotalLaps => totalLaps;

        public RoguelikeCarStats RoguelikeStats => roguelikeStats;

        [SerializeField] private CarVariableSet variables;

        [SerializeField] private int totalLaps = 3;

        [SerializeField] private RoguelikeCarStats roguelikeStats = RoguelikeCarStats.Default;

        internal void SetVariablesForTests(CarVariableSet value)
        {
            variables = value;
        }

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
            copy.variables = variables;
            copy.totalLaps = totalLaps;
            copy.roguelikeStats = roguelikeStats;
            return copy;
        }
    }
}
