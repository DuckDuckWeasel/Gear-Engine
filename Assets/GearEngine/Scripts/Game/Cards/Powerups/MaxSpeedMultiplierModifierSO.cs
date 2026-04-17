using UnityEngine;

namespace GearEngine.Cards.Powerups
{
    [CreateAssetMenu(menuName = "Game/Cards/Modifiers/Max Speed Multiplier", fileName = "MaxSpeedMult")]
    public sealed class MaxSpeedMultiplierModifierSO : CarPowerupModifierSO
    {
        public float Multiplier => multiplier;

        [SerializeField] [Min(0.01f)] private float multiplier = 1.1f;

        public override void Apply(ref CarPowerupStats stats)
        {
            stats.MaxSpeedMultiplier *= multiplier;
        }
    }
}
