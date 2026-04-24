using System;
using UnityEngine;

namespace GearEngine.CarSimulation.Definitions
{
    [Serializable]
    public sealed class RaceSessionConfig
    {
        public RoguelikeCarStats RoguelikeStats => roguelikeStats;

        [SerializeField] private RoguelikeCarStats roguelikeStats = RoguelikeCarStats.Default;

        public void SetRoguelikeStats(RoguelikeCarStats stats) { roguelikeStats = stats; }

        public RaceSessionConfig CloneForNewRace()
        {
            var copy = new RaceSessionConfig();
            copy.roguelikeStats = roguelikeStats;
            return copy;
        }
    }
}
