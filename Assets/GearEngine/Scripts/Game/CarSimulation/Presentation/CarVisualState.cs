namespace GearEngine.CarSimulation.Presentation
{
    public sealed class CarVisualState
    {
        public float CornerEffect;
        public float LateralOffset;
        public float SlipAngle;
        public bool IsDrifting;

        public void Reset()
        {
            CornerEffect = 0f;
            LateralOffset = 0f;
            SlipAngle = 0f;
            IsDrifting = false;
        }

        internal void ClearCosmetic()
        {
            CornerEffect = 0f;
            LateralOffset = 0f;
            SlipAngle = 0f;
            IsDrifting = false;
        }
    }
}
