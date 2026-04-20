using GearEngine.CarSimulation;
using GearEngine.GearEngine;
using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine.Nodes;
using Scaffold.Entities;
using UnityEngine;

namespace GearEngine.Campaign.Gear
{
    [CreateAssetMenu(fileName = "TemporaryBoostGear", menuName = "Gear Engine/Abilities/Temporary Race Start Boost")]
    public sealed class TemporaryBoostGearAbilitySO : ActiveRaceGearAbilitySO
    {
        [Header("Boost Setup")]
        [SerializeField] private VariableSO targetVariable;
        [SerializeField] private float buffValue = 50f;
        [SerializeField] private float durationSeconds = 3f;

        private bool hasApplied;

        public override void Initialize(RaceState state, IGearEngineService gearEngine)
        {
            base.Initialize(state, gearEngine);
            hasApplied = false;
        }

        public override void Execute(IGridNode owner)
        {
            if (!hasApplied && RaceContext != null && RaceContext.Phase == SimulationLifecycleState.Running)
            {
                // The new ApplyModifier signature accepts "durationSeconds"
                // which automatically tracks and removes the buff internally via the Tick method!
                ApplyModifier(owner, targetVariable, buffValue, durationSeconds);
                hasApplied = true;
                
                Debug.Log($"[TemporaryBoostGear] Fired temporary boost of {buffValue} to {targetVariable.name} for {durationSeconds}s!");
            }
        }
    }
}
