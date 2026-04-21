using GearEngine.CarSimulation;
using GearEngine.GearEngine;
using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine.Nodes;
using Scaffold.Entities;
using UnityEngine;

namespace GearEngine.Campaign.Gear
{
    [CreateAssetMenu(fileName = "RecoveryGear", menuName = "Gear Engine/Abilities/Recovery Gear (Low Speed Boost)")]
    public sealed class RecoveryGearAbilitySO : ActiveRaceGearAbilitySO
    {
        [Header("Triggers")]
        [SerializeField] private float speedThreshold = 20f;
        [SerializeField] private float cooldownTime = 5f;

        [Header("Boost")]
        [SerializeField] private VariableSO targetVariable;
        [SerializeField] private float buffValue = 30f;
        [SerializeField] private float durationSeconds = 1.5f;

        private float nextAvailableTime;

        public override void Initialize(RaceState state, IGearEngineService gearEngine)
        {
            base.Initialize(state, gearEngine);
            nextAvailableTime = 0f;
        }

        public override void Tick(IGridNode owner, float deltaTime)
        {
            base.Tick(owner, deltaTime);

            if (RaceContext == null || RaceContext.Phase != SimulationLifecycleState.Running) return;

            if (RaceContext.RaceTime >= nextAvailableTime && RaceContext.CurrentSpeed < speedThreshold)
            {
                ApplyModifier(owner, targetVariable, buffValue, durationSeconds);
                nextAvailableTime = RaceContext.RaceTime + cooldownTime;
                Debug.Log($"[RecoveryGear] Speed dropped below {speedThreshold}! Applying +{buffValue} recovery boost.");
            }
        }
    }
}
