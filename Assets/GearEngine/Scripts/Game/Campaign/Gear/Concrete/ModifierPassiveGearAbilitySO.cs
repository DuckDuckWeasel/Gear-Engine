using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine;
using GearEngine.CarSimulation.Definitions;
using UnityEngine;

namespace GearEngine.Campaign.Gear
{
    public enum PassiveStatType
    {
        TopSpeed,
        Acceleration,
        BrakingSystem,
        DriftControl,
        NitrousBoost,
        SteeringGrip,
        RacingLine,
        DriverReflexes
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
                    case PassiveStatType.TopSpeed: stats.statTopSpeed += mod.Amount; break;
                    case PassiveStatType.Acceleration: stats.statAcceleration += mod.Amount; break;
                    case PassiveStatType.BrakingSystem: stats.statBrakingSystem += mod.Amount; break;
                    case PassiveStatType.DriftControl: stats.statDriftControl += mod.Amount; break;
                    case PassiveStatType.NitrousBoost: stats.statNitrousBoost += mod.Amount; break;
                    case PassiveStatType.SteeringGrip: stats.statSteeringGrip += mod.Amount; break;
                    case PassiveStatType.RacingLine: stats.statRacingLine += mod.Amount; break;
                    case PassiveStatType.DriverReflexes: stats.statDriverReflexes += mod.Amount; break;
                }
            }
        }
    }
}
