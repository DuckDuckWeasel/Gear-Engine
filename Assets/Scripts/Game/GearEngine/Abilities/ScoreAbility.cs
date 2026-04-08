using UnityEngine;

namespace Game.GearEngine
{
    [CreateAssetMenu(fileName = "ScoreAbility", menuName = "GearEngine/Abilities/Score")]
    public class ScoreAbility : GearAbilitySO
    {
        public override void Execute(IGridNode owner)
        {
            Debug.Log($"[ScoreAbility] Gear at {owner.Position} granted Score points!");
        }
    }
}
