using System;
using GearEngine.CarSimulation.Presentation;
using GearEngine.CarSimulation.Simulation;
using UnityEngine;

namespace GearEngine.CarSimulation.Definitions
{
    [Serializable]
    public sealed class RaceSessionConfig
    {
        public CarVariableSet Variables => variables;

        public LapSimulationConfig Lap => lap;

        public SplineSamplerConfig Sampler => sampler;

        public CarVisualConfig Visual => visual;

        [SerializeField]
        private CarVariableSet variables;

        [SerializeField]
        private LapSimulationConfig lap = new LapSimulationConfig();

        [SerializeField]
        private SplineSamplerConfig sampler = new SplineSamplerConfig();

        [SerializeField]
        private CarVisualConfig visual = new CarVisualConfig();

        internal void SetVariablesForTests(CarVariableSet value)
        {
            variables = value;
        }
    }
}
