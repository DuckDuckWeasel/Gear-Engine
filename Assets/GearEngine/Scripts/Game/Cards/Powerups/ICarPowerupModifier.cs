namespace GearEngine.Cards.Powerups
{
    public interface ICarPowerupModifier
    {
        CarPowerupApplyPhase Phase { get; }

        void Apply(ref CarPowerupStats stats);
    }
}
