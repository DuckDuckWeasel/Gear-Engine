using System;

namespace GearEngine.CarSimulation.Simulation
{
    [Serializable]
    public sealed class LapSimulationConfig
    {
        public float CurveSlowdown = 0.6f;

        public int TotalLaps = 3;

        public float HandlingNormalizationScale = 100f;
    }
}
