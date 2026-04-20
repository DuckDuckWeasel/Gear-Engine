using Scaffold.Events.Contracts;
using GearEngine.CarSimulation;
using GearEngine.GearEngine;
using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Events;
using Scaffold.Entities;
using UnityEngine;

namespace GearEngine.Campaign.Gear
{
    [CreateAssetMenu(fileName = "FragileBombGear", menuName = "Gear Engine/Abilities/Group A/Fragile Bomb Gear")]
    public sealed class FragileBombGearAbilitySO : ActiveRaceGearAbilitySO
    {
        [Header("Bomb Metrics")]
        [SerializeField] private VariableSO targetVariable;
        [SerializeField] private float explosiveBuffValue = 200f;
        [SerializeField] private float buffDurationSeconds = 5f;
        [SerializeField] private int requiredTriggersToExplode = 5;

        private int currentTriggers;

        public override void Initialize(RaceState state, IGearEngineService gearEngine)
        {
            base.Initialize(state, gearEngine);
            currentTriggers = 0;
        }

        public override void Execute(IGridNode owner)
        {
            if (RaceContext == null || RaceContext.Phase != SimulationLifecycleState.Running) return;

            currentTriggers++;
            Debug.Log($"[FragileBomb] Ticking... ({currentTriggers}/{requiredTriggersToExplode})");

            if (currentTriggers >= requiredTriggersToExplode)
            {
                ApplyModifier(owner, targetVariable, explosiveBuffValue, buffDurationSeconds);
                Debug.Log($"[FragileBomb] EXPLODED! Applied +{explosiveBuffValue} to {targetVariable.name} and shattered itself.");
                owner.EventBus?.Raise(new GearDestroyedEvent(owner.Position));
            }
        }
    }
}
