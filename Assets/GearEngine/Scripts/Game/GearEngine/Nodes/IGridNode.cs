using UnityEngine;
using System;

namespace GearEngine.GearEngine.Nodes
{
    public interface IGridNode : IDisposable
    {
        Vector2Int Position { get; }
        float CurrentRotation { get; }
        GearConfigData ConfigData { get; }
        float LocalSpeedMultiplier { get; set; }
        bool IsActive { get; set; }
        bool IsInteractable { get; }
        
        Scaffold.Events.Contracts.IEventBus EventBus { get; }
        
        void AddAbility(GearAbilitySO ability, float duration = -1f);
        void RemoveAbility(GearAbilitySO ability);
        void Initialize(Vector2Int position, GearConfigData configData);
        void NodeUpdate(float deltaTime, float speedModifier);
        void WindDownUpdate(float deltaTime, float speedModifier);
    }
}
