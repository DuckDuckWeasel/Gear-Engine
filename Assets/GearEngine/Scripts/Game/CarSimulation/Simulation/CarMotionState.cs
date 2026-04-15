namespace GearEngine.CarSimulation.Simulation
{
    internal sealed class CarMotionState
    {
        public float Distance;
        public float Speed;
        public float SpeedStress;
        public float LineError;
        public float LateralOffset;
        public float SlipAngle;
        public int SampleIndex;

        public void Reset()
        {
            Distance = 0f;
            Speed = 0f;
            SpeedStress = 0f;
            LineError = 0f;
            LateralOffset = 0f;
            SlipAngle = 0f;
            SampleIndex = 0;
        }
    }
}
