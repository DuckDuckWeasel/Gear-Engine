using UnityEngine;

namespace GearEngine.GearEngine.Abilities
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

        /// <summary>Sample: No-op execute; inactive gears rarely reach max charge.</summary>
        public override void Execute(IGridNode owner)
        {
        }
    }
}
