using Scaffold.Events.Contracts;
using GearEngine.CarSimulation;
using GearEngine.GearEngine;
using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Events;
using UnityEngine;

namespace GearEngine.Campaign.Gear
{
    [CreateAssetMenu(fileName = "NeighborOverclockGear", menuName = "Gear Engine/Abilities/Neighbor Overclock (Board Boost)")]
    public sealed class NeighborOverclockGearAbilitySO : ActiveRaceGearAbilitySO
    {
        [Header("Overclock Pulse")]
        [SerializeField] private float overclockAmount = 20f;
        [SerializeField] private float intervalSeconds = 2f;

        private float nextTriggerTime;

        public override void Initialize(RaceState state, IGearEngineService gearEngine)
        {
            base.Initialize(state, gearEngine);
            nextTriggerTime = 0f;
        }

        public override void Tick(IGridNode owner, float deltaTime)
        {
            base.Tick(owner, deltaTime);
            if (RaceContext == null || RaceContext.Phase != SimulationLifecycleState.Running) return;

            if (RaceContext.RaceTime >= nextTriggerTime)
            {
                nextTriggerTime = RaceContext.RaceTime + intervalSeconds;
                
                // Emitting Directional Trigger events out to adjacent cells natively spins/charges them!
                Vector2Int pos = owner.Position;
                owner.EventBus.Raise(new DirectionalTriggerEvent(pos + Vector2Int.up, overclockAmount, 1f));
                owner.EventBus.Raise(new DirectionalTriggerEvent(pos + Vector2Int.down, overclockAmount, 1f));
                owner.EventBus.Raise(new DirectionalTriggerEvent(pos + Vector2Int.left, overclockAmount, 1f));
                owner.EventBus.Raise(new DirectionalTriggerEvent(pos + Vector2Int.right, overclockAmount, 1f));
                
                Debug.Log($"[NeighborOverclock] Pulsing extra charge to adjacent gears!");
            }
        }
    }
}
