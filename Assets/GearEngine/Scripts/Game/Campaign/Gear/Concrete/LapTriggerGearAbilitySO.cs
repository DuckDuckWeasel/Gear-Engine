using GearEngine.CarSimulation;
using GearEngine.GearEngine;
using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine.Nodes;
using Scaffold.Entities;
using UnityEngine;

namespace GearEngine.Campaign.Gear
{
    [CreateAssetMenu(fileName = "LapTriggerGear", menuName = "GearEngine/Abilities/Lap Trigger Buff")]
    public sealed class LapTriggerGearAbilitySO : ActiveRaceGearAbilitySO
    {
        [Header("Buff Setup")]
        [SerializeField] private VariableSO targetVariable;
        [SerializeField] private float buffPerLap = 5f;

        private int lastProcessedLap;

        public override void Initialize(RaceState state, IGearEngineService gearEngine)
        {
            base.Initialize(state, gearEngine);
            lastProcessedLap = state?.CurrentLap ?? 0;
        }

        public override void Tick(IGridNode owner, float deltaTime)
        {
            base.Tick(owner, deltaTime);
            if (RaceContext == null || RaceContext.Phase != SimulationLifecycleState.Running) return;

            if (owner is BaseGearNode gearNode && gearNode.ConfigData != null)
            {
                // Sync the visual 'Charge' (which animates the gear) to the car's lap progress!
                float targetCharge = RaceContext.NormalizedProgress * gearNode.ConfigData.MaxCharge;
                gearNode.SetCharge(targetCharge);
            }
        }

        public override void Execute(IGridNode owner)
        {
            if (RaceContext == null) return;

            // Wait, the physical completion of a lap is usually an increment
            if (RaceContext.CurrentLap > lastProcessedLap && RaceContext.Phase == SimulationLifecycleState.Running)
            {
                ApplyModifier(owner, targetVariable, buffPerLap);
                lastProcessedLap = RaceContext.CurrentLap;
                
                Debug.Log($"[LapTriggerGear] Applied +{buffPerLap} to {targetVariable.name} for finishing lap!");
            }
        }
    }
}
