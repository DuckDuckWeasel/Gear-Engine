using GearEngine.CarSimulation;
using GearEngine.GearEngine;
using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine.Nodes;
using Scaffold.Entities;
using UnityEngine;

namespace GearEngine.Campaign.Gear
{
    [CreateAssetMenu(fileName = "TrackSegmentBoostGear", menuName = "Gear Engine/Abilities/Track Segment Boost")]
    public sealed class TrackSegmentBoostGearAbilitySO : ActiveRaceGearAbilitySO
    {
        [Header("Trigger Setup")]
        [Range(0f, 1f)] [SerializeField] private float triggerProgress = 0.5f;
        
        [Header("Boost Setup")]
        [SerializeField] private VariableSO targetVariable;
        [SerializeField] private float buffValue = 40f;
        [SerializeField] private float durationSeconds = 2f;

        private int lastActivatedLap;

        public override void Initialize(RaceState state, IGearEngineService gearEngine)
        {
            base.Initialize(state, gearEngine);
            lastActivatedLap = -1; // Ready for lap 0
        }

        public override void Tick(IGridNode owner, float deltaTime)
        {
            base.Tick(owner, deltaTime);
            if (RaceContext == null || RaceContext.Phase != SimulationLifecycleState.Running) return;

            if (RaceContext.CurrentLap > lastActivatedLap && RaceContext.NormalizedProgress >= triggerProgress)
            {
                ApplyModifier(owner, targetVariable, buffValue, durationSeconds);
                lastActivatedLap = RaceContext.CurrentLap;
                Debug.Log($"[TrackSegmentBoostGear] Hit {triggerProgress*100}% track marker! Turbo engaged!");
            }
        }
    }
}
