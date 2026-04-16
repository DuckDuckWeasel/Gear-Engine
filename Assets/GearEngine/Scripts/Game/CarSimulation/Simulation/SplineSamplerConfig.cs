using System;

namespace GearEngine.CarSimulation.Simulation
{
    [Serializable]
    public sealed class SplineSamplerConfig
    {
        public float CurveLookAheadStep = 0.02f;

        public float MaxCurveAngle = 90f;
    }
}
