using GearEngine.GearEngine.Nodes;
using GearEngine.CarSimulation.PhysicsSimulation;
using GearEngine.GearEngine;
using GearEngine.CarSimulation.Definitions;
using UnityEngine;

namespace GearEngine.Campaign.Gear
{
    public enum PassiveStatType
    {
        SpeedCapability,
        CorneringSkill,
        Drift,
        Precision,
        Smoothness
    }

    [System.Serializable]
    public struct PassiveStatModifier
    {
        public PassiveStatType Stat;
        public float Amount;
    }

    [CreateAssetMenu(fileName = "ModifierPassiveGear", menuName = "Gear Engine/Abilities/Passive Stat Modifier")]
    public sealed class ModifierPassiveGearAbilitySO : PassiveRaceGearAbilitySO
    {
        [Header("Passive Stat Adjustments")]
        [SerializeField] private System.Collections.Generic.List<PassiveStatModifier> modifiers = new System.Collections.Generic.List<PassiveStatModifier>();

        public override void ApplyPassiveStats(ref RoguelikeCarStats stats, IGridNode owner, IGearEngineService engine)
        {
            foreach (var mod in modifiers)
            {
                switch (mod.Stat)
                {
                    case PassiveStatType.SpeedCapability: stats.SpeedCapability += mod.Amount; break;
                    case PassiveStatType.CorneringSkill: stats.CorneringSkill += mod.Amount; break;
                    case PassiveStatType.Drift: stats.Drift += mod.Amount; break;
                    case PassiveStatType.Precision: stats.Precision += mod.Amount; break;
                    case PassiveStatType.Smoothness: stats.Smoothness += mod.Amount; break;
                }
            }
        }
    }
}
