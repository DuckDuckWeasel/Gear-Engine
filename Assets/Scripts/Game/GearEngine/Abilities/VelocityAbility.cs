using UnityEngine;

namespace Game.GearEngine
{
    [CreateAssetMenu(fileName = "VelocityAbility", menuName = "GearEngine/Abilities/Velocity")]
    public class VelocityAbility : GearAbilitySO
    {
        public override void Execute(IGridNode owner)
        {
            Debug.Log($"[VelocityAbility] Gear at {owner.Position} triggered Velocity increase!");
        }
    }
}
