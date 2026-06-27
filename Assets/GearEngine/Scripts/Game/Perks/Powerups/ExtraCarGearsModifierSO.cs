using UnityEngine;

namespace GearEngine.Perks.Powerups
{
    [CreateAssetMenu(menuName = "Game/Perks/Modifiers/Extra Car Gears", fileName = "ExtraCarGearsMult")]
    public sealed class ExtraCarGearsModifierSO : CarPowerupModifierSO
    {
        public int ExtraGears => extraGears;

        [SerializeField] [Min(1)] private int extraGears = 1;

        public override void Apply(ref CarPowerupStats stats)
        {
            stats.ExtraCarGears += extraGears;
        }

        public override string GetFormattedValue() => $"+{extraGears}";
    }
}
