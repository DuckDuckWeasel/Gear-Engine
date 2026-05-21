using UnityEngine;

namespace GearEngine.Perks.Powerups
{
    [CreateAssetMenu(menuName = "Game/Perks/Modifiers/Extra Inventory Gears", fileName = "ExtraInventoryGearsMult")]
    public sealed class ExtraInventoryGearsModifierSO : CarPowerupModifierSO
    {
        public int ExtraGears => extraGears;

        [SerializeField] [Min(1)] private int extraGears = 1;

        public override void Apply(ref CarPowerupStats stats)
        {
            stats.ExtraInventoryGears += extraGears;
        }
    }
}
