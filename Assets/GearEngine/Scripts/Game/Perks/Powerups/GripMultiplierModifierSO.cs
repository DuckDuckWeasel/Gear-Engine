using UnityEngine;

namespace GearEngine.Perks.Powerups
{
    [CreateAssetMenu(menuName = "Game/Perks/Modifiers/Grip Multiplier", fileName = "GripMult")]
    public sealed class GripMultiplierModifierSO : CarPowerupModifierSO
    {
        public float Multiplier => multiplier;

        [SerializeField] [Min(0.01f)] private float multiplier = 1.05f;

        public override void Apply(ref CarPowerupStats stats)
        {
            stats.GripMultiplier *= multiplier;
        }

        public override string GetFormattedValue() => $"+{(multiplier - 1f) * 100f:0}%";
    }
}
