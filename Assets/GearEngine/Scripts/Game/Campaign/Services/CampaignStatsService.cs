using System.Linq;
using GearEngine.CarSimulation.PhysicsSimulation;
using GearEngine.Campaign.Gear;
using GearEngine.CarSimulation.Definitions;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Config;
using UnityEngine;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Abilities;

namespace GearEngine.Campaign.Services
{
    public sealed class CampaignStatsService : ICampaignStatsService
    {
        private readonly RaceSessionConfig sessionTemplate;
        private readonly IGearEngineService gearEngine;

        public CampaignStatsService(RaceSessionConfig sessionTemplate, IGearEngineService gearEngine)
        {
            this.sessionTemplate = sessionTemplate ?? new RaceSessionConfig();
            this.gearEngine = gearEngine;
        }

        public RoguelikeCarStats GetBaseStats()
        {
            return sessionTemplate.RoguelikeStats;
        }

        public RoguelikeCarStats GetCalculatedStats()
        {
            return GetCalculatedStats(null);
        }

        public RoguelikeCarStats GetCalculatedStats(TrackDefinition track)
        {
            RoguelikeCarStats currentStats = sessionTemplate.RoguelikeStats;
            TrackBiome biome = track != null && track.Theme != null ? track.Theme.Biome : TrackBiome.None;
            int matchingGears = 0;

            if (gearEngine != null)
            {
                foreach (IGridNode node in gearEngine.GetAllNodes())
                {
                    if (node == null)
                    {
                        continue;
                    }

                    if (biome != TrackBiome.None && node.ConfigData?.BiomeAffinity == biome)
                    {
                        matchingGears++;
                    }
                    foreach (GearAbilitySO ability in node.GetAbilities())
                    {
                        if (ability is PassiveRaceGearAbilitySO passiveGear)
                        {
                            passiveGear.ApplyPassiveStats(ref currentStats, node, gearEngine);
                        }
                    }
                }
            }

            if (matchingGears > 0)
            {
                int appliedGears = Mathf.Min(matchingGears, GearItemData.k_maxBiomeBonusGears);
                currentStats.SpeedCapability = Mathf.Clamp(
                    currentStats.SpeedCapability + appliedGears * GearItemData.k_biomeSpeedBonusPerGear,
                    0f,
                    100f);
            }

            return currentStats;
        }
    }
}
