using Scaffold.Events.Contracts;
using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Events;
using UnityEngine;
using Scaffold.Entities;

namespace GearEngine.Campaign.Gear
{
    [CreateAssetMenu(fileName = "BlackholeGear", menuName = "Gear Engine/Abilities/Group B/Blackhole Gear")]
    public sealed class BlackholeGearAbilitySO : ActiveRaceGearAbilitySO
    {
        [SerializeField] private VariableSO burstTarget;
        [SerializeField] private float burstAmount = 400f;
        private float tickTimer = 0f;

        [SerializeField] private float pullForce = -10f;
        [SerializeField] private float eventDuration = 1f;
        [SerializeField] private float buffDuration = 4f;
        public override void Tick(IGridNode owner, float deltaTime)
        {
            base.Tick(owner, deltaTime);
            tickTimer += deltaTime;
            if(tickTimer > 1f) {
                tickTimer = 0f;
                // Suck charge from neighbors (Negative directional event)
                owner.EventBus?.Raise(new DirectionalTriggerEvent(owner.Position + Vector2Int.up, pullForce, eventDuration));
                owner.EventBus?.Raise(new DirectionalTriggerEvent(owner.Position + Vector2Int.down, pullForce, eventDuration));
            }
        }

        public override void Execute(IGridNode owner)
        {
            if (RaceContext == null) return;
            ApplyModifier(owner, burstTarget, burstAmount, buffDuration);
        }
    }
}
