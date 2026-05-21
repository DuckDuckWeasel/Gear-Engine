namespace GearEngine.Perks.Powerups
{
    public interface ICarPowerupModifier
    {
        CarPowerupApplyPhase Phase { get; }

        void Apply(ref CarPowerupStats stats);
    }
}
