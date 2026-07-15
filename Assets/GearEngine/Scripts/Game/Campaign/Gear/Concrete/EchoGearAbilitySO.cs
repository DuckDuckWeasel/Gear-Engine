using GearEngine.CarSimulation;
using System.Linq;
using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine.Nodes;
using UnityEngine;

namespace GearEngine.Campaign.Gear
{
    [CreateAssetMenu(fileName = "EchoGear", menuName = "GearEngine/Abilities/Group B/Echo Gear")]
    public sealed class EchoGearAbilitySO : ActiveRaceGearAbilitySO
    {
        public override void Execute(IGridNode owner)
        {
            if (GearEngineContext == null || RaceContext == null || RaceContext.Phase != SimulationLifecycleState.Running) return;

            // Find the node physically located at local Y+1 (Up in traditional 2D grids)
            var targetPos = owner.Position + Vector2Int.up;
            var nodeAbove = GearEngineContext.GetAllNodes()
                .FirstOrDefault(n => n != null && n.Position == targetPos);

            if (nodeAbove != null)
            {
                Debug.Log($"[EchoGear] Triggered! Copying abilities of node above at {targetPos}...");
                foreach (var remoteAbility in nodeAbove.GetAbilities())
                {
                    if (remoteAbility is ActiveRaceGearAbilitySO activeSO && activeSO != this)
                    {
                        // Safely execute the neighbor's runtime ability as if WE triggered it
                        // Since Execute depends on 'owner', the buff will correctly map back to the Echo Gear!
                        activeSO.Execute(owner);
                    }
                }
            }
        }
    }
}
