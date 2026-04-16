using System;

namespace GearEngine.CarSimulation.Presentation
{
    [Serializable]
    public sealed class CarVisualConfig
    {
        public float CornerResponse = 4f;

        public float DriftStrength = 1f;

        public float DriftRecoverRate = 2f;

        public float MaxVisualOffset = 1.2f;

        public float MaxSlipAngle = 12f;

        public float DriftThreshold = 0.05f;
    }
}
