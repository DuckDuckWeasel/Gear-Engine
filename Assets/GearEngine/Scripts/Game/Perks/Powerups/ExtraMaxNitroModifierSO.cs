using UnityEngine;

namespace GearEngine.Perks.Powerups
{
    [CreateAssetMenu(menuName = "Game/Perks/Modifiers/Extra Max Nitro", fileName = "ExtraMaxNitroMult")]
    public sealed class ExtraMaxNitroModifierSO : CarPowerupModifierSO
    {
        public float ExtraNitro => extraNitro;

        [SerializeField] [Min(0.1f)] private float extraNitro = 10f;

        public override void Apply(ref CarPowerupStats stats)
        {
            stats.ExtraMaxNitro += extraNitro;
        }
    }
}
