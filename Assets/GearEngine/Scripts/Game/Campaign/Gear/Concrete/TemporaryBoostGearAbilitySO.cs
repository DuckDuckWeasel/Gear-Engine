using GearEngine.CarSimulation;
using GearEngine.GearEngine;
using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine.Nodes;
using Scaffold.Entities;
using UnityEngine;

namespace GearEngine.Campaign.Gear
{
    [CreateAssetMenu(fileName = "TemporaryBoostGear", menuName = "GearEngine/Abilities/Temporary Race Start Boost")]
    public sealed class TemporaryBoostGearAbilitySO : ActiveRaceGearAbilitySO
    {
        [Header("Boost Setup")]
        [SerializeField] private VariableSO targetVariable;
        [SerializeField] private float buffValue = 50f;
        [SerializeField] private float durationSeconds = 3f;

        public override void Execute(IGridNode owner)
        {
            if (RaceContext != null && RaceContext.Phase == SimulationLifecycleState.Running)
            {
                ApplyModifier(owner, targetVariable, buffValue, durationSeconds);
                Debug.Log($"[TemporaryBoostGear] Fired temporary boost of {buffValue} to {targetVariable.name} for {durationSeconds}s!");
            }
        }
    }
}
