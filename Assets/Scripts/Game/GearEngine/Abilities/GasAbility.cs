using UnityEngine;

namespace Game.GearEngine
{
    [CreateAssetMenu(fileName = "GasAbility", menuName = "GearEngine/Abilities/Gas")]
    public class GasAbility : GearAbilitySO
    {
        public override void Execute(IGridNode owner)
        {
            Debug.Log($"[GasAbility] Gear at {owner.Position} generated Gas!");
        }
    }
}
