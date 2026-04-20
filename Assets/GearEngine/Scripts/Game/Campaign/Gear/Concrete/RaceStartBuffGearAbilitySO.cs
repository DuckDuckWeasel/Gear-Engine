using GearEngine.CarSimulation;
using GearEngine.GearEngine;
using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine.Nodes;
using Scaffold.Entities;
using UnityEngine;

namespace GearEngine.Campaign.Gear
{
    [CreateAssetMenu(fileName = "RaceStartBuffGear", menuName = "Gear Engine/Abilities/Race Start Buff")]
    public sealed class RaceStartBuffGearAbilitySO : ActiveRaceGearAbilitySO
    {
        [Header("Buff Info")]
        [SerializeField] private VariableSO targetVariable;
        [SerializeField] private float buffValue = 20f;

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
                ApplyModifier(owner, targetVariable, buffValue);
                hasApplied = true;
                Debug.Log($"[RaceStartBuffGear] Injected {buffValue} into {targetVariable.name} right at the green light!");
            }
        }
    }
}
