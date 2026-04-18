using System;

namespace GearEngine.CarSimulation.Simulation
{
    [Serializable]
    public struct CarTelemetryData
    {
        public float Speed;
        public float Progress;
        public bool IsBraking;
        public bool IsDrifting;
        public bool IsAccelerating;
        public float CurrentAcceleration;
    }
}
