using UnityEngine;
using System;

namespace GearEngine.GearEngine.Nodes
{
    public interface IGridNode : IDisposable
    {
        Vector2Int Position { get; }
        float CurrentRotation { get; }
        GearItemData ConfigData { get; }
        float LocalSpeedMultiplier { get; set; }
        bool IsActive { get; set; }
        bool IsInteractable { get; }
        
        Scaffold.Events.Contracts.IEventBus EventBus { get; }
        
        void SetPosition(Vector2Int position);
        void AddAbility(GearAbilitySO ability, float duration = -1f);
        void RemoveAbility(GearAbilitySO ability);
        void Initialize(Vector2Int position, GearItemData configData);
        void NodeUpdate(float deltaTime, float speedModifier);
        void WindDownUpdate(float deltaTime, float speedModifier);

        /// <summary>Returns deeply cloned abilities or standard active ones attached.</summary>
        System.Collections.Generic.IEnumerable<GearEngine.Abilities.GearAbilitySO> GetAbilities();

        /// <summary>Clears rotation, charge, and other per-run simulation state (layout unchanged).</summary>
        void ResetSimulationState();
    }
}
