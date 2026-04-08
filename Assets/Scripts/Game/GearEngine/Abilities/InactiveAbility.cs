using UnityEngine;

namespace Game.GearEngine
{
    [CreateAssetMenu(menuName = "GearEngine/Abilities/Inactive Effect")]
    public class InactiveAbility : GearAbilitySO
    {
        public override void OnActive(IGridNode owner)
        {
            owner.IsActive = false;
            Debug.Log($"<color=#ff4444>[InactiveAbility]</color> {owner.Position} is now INACTIVE.");
        }

        public override void OnDeactive(IGridNode owner)
        {
            owner.IsActive = true;
            Debug.Log($"<color=#44ff44>[InactiveAbility]</color> {owner.Position} is now ACTIVE again.");
        }

        // Execute happens on max charge, but since it's inactive, it probably won't trigger. 
        // We leave it empty as this is a Status Effect ability.
        public override void Execute(IGridNode owner)
        {
        }
    }
}
