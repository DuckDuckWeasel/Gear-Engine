using System.Linq;
using GearEngine.CarSimulation.PhysicsSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Nodes;
using UnityEngine;

namespace GearEngine.Campaign.Gear
{
    [CreateAssetMenu(fileName = "CloneGear", menuName = "Gear Engine/Abilities/Group B/Clone Gear")]
    public sealed class CloneGearAbilitySO : PassiveRaceGearAbilitySO
    {
        [SerializeField] private float speedCapabilityBonus = 50f;
        [SerializeField] private float precisionBonus = 20f;

        public override void ApplyPassiveStats(ref RoguelikeCarStats stats, IGridNode owner, IGearEngineService engine)
        {
            stats.SpeedCapability += speedCapabilityBonus;
            stats.Precision += precisionBonus;
        }
    }
}
