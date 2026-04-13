using UnityEngine;

namespace GearEngine.GearEngine.Abilities
{
    [CreateAssetMenu(menuName = "GearEngine/Abilities/Score Ability")]
    public class ScoreAbility : GearAbilitySO
    {
        [Tooltip("The amount of score to award when this gear reaches Max Charge.")]
        public int ScoreAmount = 100;

        public override void Execute(IGridNode owner)
        {
            // In a complete game, we would raise a ScoreEvent here 
            // e.g., owner.EventBus?.Raise(new ScoreGainedEvent(ScoreAmount));
            Debug.Log($"<color=#55ff55>[ScoreAbility]</color> Gear at {owner.Position} produced {ScoreAmount} Points!");
            
            // For now, raising a simple placeholder log is enough to demonstrate the mechanic.
        }
    }
}
