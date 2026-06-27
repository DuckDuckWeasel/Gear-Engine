using UnityEngine;

namespace GearEngine.Perks.Powerups
{
    [CreateAssetMenu(menuName = "Game/Perks/Modifiers/Initial Speed Boost", fileName = "InitialSpeedBoostMult")]
    public sealed class InitialSpeedBoostModifierSO : CarPowerupModifierSO
    {
        public float SpeedBoost => speedBoost;

        [SerializeField] [Min(0.1f)] private float speedBoost = 10f;

        public override void Apply(ref CarPowerupStats stats)
        {
            stats.InitialSpeedBoost += speedBoost;
        }

        public override string GetFormattedValue() => $"+{speedBoost}";
    }
}
