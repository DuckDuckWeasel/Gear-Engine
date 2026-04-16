using System;
using UnityEngine;

namespace GearEngine.CarSimulation.Definitions
{
    [Serializable]
    public sealed class TrackSimulationConfig
    {
        public CarVariableSet Variables => variables;

        public SimpleTrackDriverTuning Driver => driver;

        [SerializeField]
        private CarVariableSet variables;

        [SerializeField]
        private SimpleTrackDriverTuning driver = new SimpleTrackDriverTuning();
    }
}
