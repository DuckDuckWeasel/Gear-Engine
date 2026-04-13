using UnityEngine;

namespace GearEngine.GearEngine.Abilities
{
    [CreateAssetMenu(fileName = "BoostAbility", menuName = "GearEngine/Abilities/Boost")]
    public class BoostAbility : GearAbilitySO
    {
        public override void Execute(IGridNode owner)
        {
            Debug.Log($"[BoostAbility] Gear at {owner.Position} fired a Boost burst!");
        }
    }
}
