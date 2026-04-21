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
    [CreateAssetMenu(fileName = "KamikazeRecoveryGear", menuName = "Gear Engine/Abilities/Group A/Kamikaze Recovery Gear")]
    public sealed class KamikazeRecoveryGearAbilitySO : ActiveRaceGearAbilitySO
    {
        [Header("Recovery Burst")]
        [SerializeField] private VariableSO targetVariable;
        [SerializeField] private float thrustValue = 150f;
        [SerializeField] private float thrustDurationSeconds = 2f;

        private bool armed;

        [SerializeField] private float crashSpeedThreshold = 0.1f;
        [SerializeField] private float idleGracePeriod = 2f;
        public override void Initialize(RaceState state, IGearEngineService gearEngine)
        {
            base.Initialize(state, gearEngine);
            armed = true;
        }

        public override void Tick(IGridNode owner, float deltaTime)
        {
            base.Tick(owner, deltaTime);
            if (RaceContext == null || RaceContext.Phase != SimulationLifecycleState.Running) return;

            if (armed && RaceContext.CurrentSpeed <= crashSpeedThreshold && RaceContext.RaceTime > idleGracePeriod) // Allow 2s to leave starting line
            {
                ApplyModifier(owner, targetVariable, thrustValue, thrustDurationSeconds);
                Debug.Log($"[Kamikaze] Car stalled! Firing emergency thrust and breaking engine!");
                owner.EventBus?.Raise(new GearDestroyedEvent(owner.Position));
                armed = false;
            }
        }
    }
}
