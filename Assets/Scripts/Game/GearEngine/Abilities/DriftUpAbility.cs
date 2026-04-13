using UnityEngine;

namespace GearEngine.GearEngine.Abilities
{
    [CreateAssetMenu(fileName = "DriftUpAbility", menuName = "GearEngine/Abilities/DriftUp")]
    public class DriftUpAbility : GearAbilitySO
    {
        public override void Execute(IGridNode owner)
        {
            Debug.Log($"[DriftUpAbility] Gear at {owner.Position} triggered Drift Up!");
        }
    }
}
