using Scaffold.Events.Contracts;
using GearEngine.CarSimulation;
using System.Linq;
using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Events;
using UnityEngine;

namespace GearEngine.Campaign.Gear
{
    [CreateAssetMenu(fileName = "QuantumLinkGear", menuName = "Gear Engine/Abilities/Group B/Quantum Link Gear")]
    public sealed class QuantumLinkGearAbilitySO : ActiveRaceGearAbilitySO
    {
        [Header("Global Battery")]
        [SerializeField] private float injectedChargeAmount = 50f;

        public override void Execute(IGridNode owner)
        {
            if (GearEngineContext == null || RaceContext == null || RaceContext.Phase != SimulationLifecycleState.Running) return;

            Debug.Log($"[QuantumLinkGear] Triggered! Master Battery discharging +{injectedChargeAmount} to all active gears...");
            
            var allNodes = GearEngineContext.GetAllNodes().Where(n => n != null && n != owner);
            foreach (var remoteNode in allNodes)
            {
                // Emit an event directly addressing the specific grid position to artificially inject Charge
                if (remoteNode.EventBus != null)
                {
                    remoteNode.EventBus.Raise(new DirectionalTriggerEvent(remoteNode.Position, injectedChargeAmount, 1f));
                }
            }
        }

        public override string GetRichTextDescription()
        {
            return $"+{injectedChargeAmount} Charge to All Gears";
        }

        public override string GetFloatingTextDescription()
        {
            return $"+{injectedChargeAmount} Charge to All Gears";
        }
    }
}
