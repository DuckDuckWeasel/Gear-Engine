using UnityEngine;

namespace GearEngine.Cards.Powerups
{
    [CreateAssetMenu(menuName = "Game/Cards/Modifiers/Grip Multiplier", fileName = "GripMult")]
    public sealed class GripMultiplierModifierSO : CarPowerupModifierSO
    {
        [SerializeField] [Min(0.01f)] private float multiplier = 1.05f;

        public float Multiplier => multiplier;

        public override void Apply(ref CarPowerupStats stats)
        {
            stats.GripMultiplier *= multiplier;
        }
    }
}
