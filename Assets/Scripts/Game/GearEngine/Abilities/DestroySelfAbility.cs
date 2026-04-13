using UnityEngine;
using Scaffold.Events.Contracts;
using VContainer;

namespace GearEngine.GearEngine.Abilities
{
    [CreateAssetMenu(menuName = "GearEngine/Abilities/Destroy Self")]
    public class DestroySelfAbility : GearAbilitySO
    {
        public override void Execute(IGridNode owner)
        {
            Debug.Log($"<color=#ff8800>[DestroySelfAbility]</color> {owner.Position} reached max charge and destroyed itself!");
            owner.EventBus?.Raise(new GearDestroyedEvent(owner.Position));
        }
    }
}
