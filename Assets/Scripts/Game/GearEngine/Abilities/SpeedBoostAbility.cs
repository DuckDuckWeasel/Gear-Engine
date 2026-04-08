using UnityEngine;

namespace Game.GearEngine
{
    [CreateAssetMenu(fileName = "SpeedBoostAbility", menuName = "GearEngine/Abilities/SpeedBoost")]
    public class SpeedBoostAbility : GearAbilitySO
    {
        public override void Execute(IGridNode owner)
        {
            Debug.Log($"[SpeedBoostAbility] Gear at {owner.Position} triggered Speed Boost!");
        }
    }
}
