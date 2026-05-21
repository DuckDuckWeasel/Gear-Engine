using System.Linq;
using GearEngine.CarSimulation.PhysicsSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Nodes;
using UnityEngine;

namespace GearEngine.Campaign.Gear
{
    [CreateAssetMenu(fileName = "AdjacentSynergyGear", menuName = "Gear Engine/Abilities/Group B/Adjacent Synergy Gear")]
    public sealed class AdjacentSynergyGearAbilitySO : PassiveRaceGearAbilitySO
    {
        [Header("Synergy Metrics")]
        [SerializeField] private PassiveStatModifier baseBonusPerNeighbor;

        public override void ApplyPassiveStats(ref RoguelikeCarStats stats, IGridNode owner, IGearEngineService engine)
        {
            if (engine == null || owner == null) return;

            // Count adjacent neighbors (Distance == 1 vertically/horizontally)
            int neighborCount = engine.GetAllNodes()
                .Count(n => n != null && n != owner && IsAdjacent(n.Position, owner.Position));

            if (neighborCount > 0)
            {
                float totalBonus = baseBonusPerNeighbor.Amount * neighborCount;
                ApplyStat(ref stats, baseBonusPerNeighbor.Stat, totalBonus);
                Debug.Log($"[AdjacentSynergy] Detected {neighborCount} neighbors. Applying total bonus: {totalBonus}");
            }
        }

        private bool IsAdjacent(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) == 1;
        }

        private void ApplyStat(ref RoguelikeCarStats stats, PassiveStatType statType, float amount)
        {
            switch (statType)
            {
                case PassiveStatType.SpeedCapability: stats.SpeedCapability += amount; break;
                case PassiveStatType.CorneringSkill: stats.CorneringSkill += amount; break;
                case PassiveStatType.Drift: stats.Drift += amount; break;
                case PassiveStatType.Precision: stats.Precision += amount; break;
                case PassiveStatType.Smoothness: stats.Smoothness += amount; break;
            }
        }
    }
}
