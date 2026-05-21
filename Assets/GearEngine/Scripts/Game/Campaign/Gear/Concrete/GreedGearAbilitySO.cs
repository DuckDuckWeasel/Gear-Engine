using System.Linq;
using GearEngine.CarSimulation.PhysicsSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Nodes;
using UnityEngine;

namespace GearEngine.Campaign.Gear
{
    [CreateAssetMenu(fileName = "GreedGear", menuName = "Gear Engine/Abilities/Group B/Greed Gear")]
    public sealed class GreedGearAbilitySO : PassiveRaceGearAbilitySO
    {
        [SerializeField] private float speedCapabilityBonusPerSlot = 10f;
        [SerializeField] private float corneringSkillBonusPerSlot = 5f;

        public override void ApplyPassiveStats(ref RoguelikeCarStats stats, IGridNode owner, IGearEngineService engine)
        {
            if (engine == null) return;
            int totalEquipped = engine.GetAllNodes().Count(n => n != null);
            int emptySlots = 16 - totalEquipped; // Hardcoded generic 4x4 default assumption
            if(emptySlots > 0)
            {
                stats.SpeedCapability += emptySlots * speedCapabilityBonusPerSlot;
                stats.CorneringSkill += emptySlots * corneringSkillBonusPerSlot;
            }
        }
    }
}
