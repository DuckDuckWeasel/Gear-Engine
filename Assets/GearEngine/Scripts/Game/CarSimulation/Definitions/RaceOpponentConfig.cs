using System;
using UnityEngine;

namespace GearEngine.CarSimulation.Definitions
{
    [Serializable]
    public sealed class RaceOpponentConfig
    {
        public RaceOpponentConfig(string displayName, float raceTimeSeconds)
        {
            this.displayName = displayName;
            this.raceTimeSeconds = raceTimeSeconds;
        }

        public string DisplayName => displayName;
        public float RaceTimeSeconds => raceTimeSeconds;
        [SerializeField] private string displayName;
        [SerializeField, Min(0.01f)] private float raceTimeSeconds;

    }
}
