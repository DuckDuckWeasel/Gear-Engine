using System;

namespace GearEngine.Cards.Powerups
{
    /// <summary>
    /// Baseline car tuning values; modifiers apply in <see cref="CarPowerupApplyPhase"/> order.
    /// </summary>
    [Serializable]
    public struct CarPowerupStats
    {
        public float MaxSpeedMultiplier;
        public float GripMultiplier;

        public static CarPowerupStats Neutral => new CarPowerupStats
        {
            MaxSpeedMultiplier = 1f,
            GripMultiplier = 1f,
        };
    }
}
