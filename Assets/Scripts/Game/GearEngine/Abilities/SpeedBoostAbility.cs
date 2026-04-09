using UnityEngine;

namespace Game.GearEngine
{
    [CreateAssetMenu(menuName = "GearEngine/Abilities/Speed Boost Ability")]
    public class SpeedBoostAbility : GearAbilitySO
    {
        [Tooltip("Multiplier applied to the owner gear. E.g. 2.0 means twice as fast.")]
        public float SpeedMultiplier = 2.0f;

        public override void OnActive(IGridNode owner)
        {
            // Apply speed boost initially
            owner.LocalSpeedMultiplier *= SpeedMultiplier;
            Debug.Log($"<color=#ffff33>[SpeedBoostAbility]</color> Gear at {owner.Position} was boosted by {SpeedMultiplier}x.");
        }

        public override void Execute(IGridNode owner)
        {
            // Passive auras don't trigger per step, so we leave execute blank.
        }

        public override void OnDeactive(IGridNode owner)
        {
            // Remove speed boost when the ability expires
            // By resetting or dividing. We'll divide back.
            if (SpeedMultiplier > 0)
            {
                owner.LocalSpeedMultiplier /= SpeedMultiplier;
                Debug.Log($"<color=#ffff33>[SpeedBoostAbility]</color> Speed boost expired on {owner.Position}.");
            }
        }
    }
}
