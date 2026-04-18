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

        internal void ApplyFromTrack(TrackDefinition track)
        {
            if (track == null)
            {
                return;
            }

            totalLaps = track.TotalLaps;
        }
    }
}
