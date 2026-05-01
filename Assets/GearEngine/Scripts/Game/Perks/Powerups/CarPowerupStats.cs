using System;

namespace GearEngine.Perks.Powerups
{
    /// <summary>sample: Baseline car tuning values; modifiers apply in <see cref="CarPowerupApplyPhase"/> order.</summary>
    [Serializable]
    public struct CarPowerupStats
    {
        public static CarPowerupStats Neutral => new CarPowerupStats
        {
            MaxSpeedMultiplier = 1f,
            GripMultiplier = 1f,
            ExtraCarGears = 0,
            ExtraInventoryGears = 0,
            ExtraMaxNitro = 0f,
            InitialSpeedBoost = 0f,
        };

        public float MaxSpeedMultiplier;
        public float GripMultiplier;
        public int ExtraCarGears;
        public int ExtraInventoryGears;
        public float ExtraMaxNitro;
        public float InitialSpeedBoost;
    }
}
