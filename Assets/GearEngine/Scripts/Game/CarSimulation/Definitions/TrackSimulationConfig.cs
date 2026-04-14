using System;
using UnityEngine;

namespace GearEngine.CarSimulation.Definitions
{
    [Serializable]
    public sealed class TrackSimulationConfig
    {
        public CarVariableSet Variables => variables;

        public TrackSimulationTuning Tuning => tuning;

        [SerializeField]
        private CarVariableSet variables;

        [SerializeField]
        private TrackSimulationTuning tuning;
    }
}
