namespace GearEngine.CarSimulation.Simulation
{
    internal sealed class CarMotionState
    {
        public float Distance;
        public float Speed;
        public float HeadingErrorDeg;
        public float LateralOffset;
        public float SlipAngle;
        public int SampleIndex;

        public void Reset()
        {
            Distance = 0f;
            Speed = 0f;
            HeadingErrorDeg = 0f;
            LateralOffset = 0f;
            SlipAngle = 0f;
            SampleIndex = 0;
        }
    }
}
