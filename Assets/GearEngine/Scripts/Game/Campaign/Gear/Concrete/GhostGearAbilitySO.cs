using System.Linq;
using GearEngine.CarSimulation.PhysicsSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Nodes;
using UnityEngine;

namespace GearEngine.Campaign.Gear
{
    [CreateAssetMenu(fileName = "GhostGear", menuName = "Gear Engine/Abilities/Group B/Ghost Gear")]
    public sealed class GhostGearAbilitySO : PassiveRaceGearAbilitySO
    {
        public override void ApplyPassiveStats(ref RoguelikeCarStats stats, IGridNode owner, IGearEngineService engine)
        {
            if (engine == null || owner == null) return;
            var frontPos = owner.Position + Vector2Int.up;
            var targetNode = engine.GetAllNodes().FirstOrDefault(n => n != null && n.Position == frontPos);
            if (targetNode != null)
            {
                // Double its passive contributions
                foreach(var abl in targetNode.GetAbilities())
                {
                    if (abl is PassiveRaceGearAbilitySO passive && passive != this)
                    {
                        passive.ApplyPassiveStats(ref stats, targetNode, engine);
                    }
                }
            }
        }
    }
}
