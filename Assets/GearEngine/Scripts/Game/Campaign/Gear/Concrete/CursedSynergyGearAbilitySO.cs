using Scaffold.Events.Contracts;
using GearEngine.CarSimulation;
using GearEngine.GearEngine;
using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Events;
using UnityEngine;
using Scaffold.Entities;

namespace GearEngine.Campaign.Gear
{
    [CreateAssetMenu(fileName = "CursedSynergyGear", menuName = "Gear Engine/Abilities/Group B/Cursed Synergy Gear")]
    public sealed class CursedSynergyGearAbilitySO : ActiveRaceGearAbilitySO
    {
        [SerializeField] private VariableSO penaltyVar;
        [SerializeField] private float penaltyAmount = -30f;
        
        [SerializeField] private float penaltyDuration = 3f;
        [SerializeField] private float neighborBoostEventForce = 50f;
        [SerializeField] private float eventDuration = 1f;
        public override void Initialize(RaceState state, IGearEngineService gearEngine)
        {
            base.Initialize(state, gearEngine);
        }

        public override void Execute(IGridNode owner)
        {
            if (RaceContext == null || RaceContext.Phase != SimulationLifecycleState.Running) return;
            ApplyModifier(owner, penaltyVar, penaltyAmount, penaltyDuration);
            
            // Pulse massive positive to surrounding
            owner.EventBus?.Raise(new DirectionalTriggerEvent(owner.Position + Vector2Int.up, neighborBoostEventForce, eventDuration));
            owner.EventBus?.Raise(new DirectionalTriggerEvent(owner.Position + Vector2Int.down, neighborBoostEventForce, eventDuration));
        }
    }
}
