using System;

namespace GearEngine.Cards.Powerups
{
    /// <summary>
    /// Values consumed by car simulation / presentation after resolving collected cards.
    /// </summary>
    [Serializable]
    public readonly struct CarPowerupBuildResult : IEquatable<CarPowerupBuildResult>
    {
        public CarPowerupBuildResult(float maxSpeedMultiplier, float gripMultiplier)
        {
            MaxSpeedMultiplier = maxSpeedMultiplier;
            GripMultiplier = gripMultiplier;
        }

        public float MaxSpeedMultiplier { get; }

        public float GripMultiplier { get; }

        public static CarPowerupBuildResult Neutral => new CarPowerupBuildResult(1f, 1f);

        public static CarPowerupBuildResult FromStats(CarPowerupStats stats)
        {
            return new CarPowerupBuildResult(stats.MaxSpeedMultiplier, stats.GripMultiplier);
        }

        public bool Equals(CarPowerupBuildResult other)
        {
            return MaxSpeedMultiplier.Equals(other.MaxSpeedMultiplier) && GripMultiplier.Equals(other.GripMultiplier);
        }

        public override bool Equals(object obj)
        {
            return obj is CarPowerupBuildResult other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (MaxSpeedMultiplier.GetHashCode() * 397) ^ GripMultiplier.GetHashCode();
            }
        }
    }
}
