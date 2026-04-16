namespace GearEngine.Cards.Powerups
{
    /// <summary>
    /// Stable ordering for stacking modifiers (extend as new channels are added).
    /// </summary>
    public enum CarPowerupApplyPhase
    {
        Base = 0,
        Additive = 10,
        Multiplicative = 20,
        Post = 100,
    }
}
