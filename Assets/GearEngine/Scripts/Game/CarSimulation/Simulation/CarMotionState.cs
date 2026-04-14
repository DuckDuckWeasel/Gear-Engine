namespace GearEngine.CarSimulation.Simulation
{
    internal sealed class CarMotionState
    {
        public float Distance;
        public float Speed;
        public float LateralOffset;
        public float SlipAngle;
        public float DriftIntensity;
        public float PendingSpeedBoost;
        public int SampleIndex;

        public void Reset()
        {
            Distance = 0f;
            Speed = 0f;
            LateralOffset = 0f;
            SlipAngle = 0f;
            DriftIntensity = 0f;
            PendingSpeedBoost = 0f;
            SampleIndex = 0;
        }
    }
}
