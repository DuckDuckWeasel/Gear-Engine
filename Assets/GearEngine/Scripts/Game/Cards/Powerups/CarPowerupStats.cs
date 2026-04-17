using System;

namespace GearEngine.Cards.Powerups
{
    /// <summary>sample: Baseline car tuning values; modifiers apply in <see cref="CarPowerupApplyPhase"/> order.</summary>
    [Serializable]
    public struct CarPowerupStats
    {
        public static CarPowerupStats Neutral => new CarPowerupStats
        {
            MaxSpeedMultiplier = 1f,
            GripMultiplier = 1f,
        };

        public float MaxSpeedMultiplier;
        public float GripMultiplier;
    }
}
