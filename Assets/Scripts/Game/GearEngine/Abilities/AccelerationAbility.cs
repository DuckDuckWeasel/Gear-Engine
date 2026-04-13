using UnityEngine;

namespace Scaffold.GearEngine.Abilities
{
    [CreateAssetMenu(fileName = "AccelerationAbility", menuName = "GearEngine/Abilities/Acceleration")]
    public class AccelerationAbility : GearAbilitySO
    {
        public override void Execute(IGridNode owner)
        {
            Debug.Log($"[AccelerationAbility] Gear at {owner.Position} triggered Acceleration increase!");
        }
    }
}
