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
    [CreateAssetMenu(fileName = "BurnoutGear", menuName = "GearEngine/Abilities/Group A/Burnout Gear")]
    public sealed class BurnoutGearAbilitySO : ActiveRaceGearAbilitySO
    {
        [Header("Burnout Settings")]
        [SerializeField] private VariableSO targetVariable;
        [SerializeField] private float massiveBuffValue = 300f;
        [SerializeField] private float timeRequiredAtRedline = 5f;

        private float timeAtRedline;
        private bool isBurntOut;

        [SerializeField] private float maxSpeedThreshold = 90f;
        public override void Initialize(RaceState state, IGearEngineService gearEngine)
        {
            base.Initialize(state, gearEngine);
            timeAtRedline = 0f;
            isBurntOut = false;
        }

        public override void Tick(IGridNode owner, float deltaTime)
        {
            base.Tick(owner, deltaTime);
            if (isBurntOut || RaceContext == null || RaceContext.Phase != SimulationLifecycleState.Running) return;

            // Simplified: If CurrentSpeed > 90% of base capacity (assume generic TopSpeed approximation)
            // We use standard maxSpeedThreshold as generic top line if pure stats aren't exposed directly here.
            if (RaceContext.CurrentSpeed > maxSpeedThreshold) 
            {
                timeAtRedline += deltaTime;
                if (timeAtRedline >= timeRequiredAtRedline)
                {
                    ApplyModifier(owner, targetVariable, massiveBuffValue);
                    Debug.Log($"[BurnoutGear] Engine REDLINED for {timeRequiredAtRedline}s! Firing permanent boost and destroying self!");
                    owner.EventBus?.Raise(new GearDestroyedEvent(owner.Position));
                    isBurntOut = true;
                }
            }
            else
            {
                // Reset if they brake or slow down
                timeAtRedline = 0f;
            }
        }
    }
}
