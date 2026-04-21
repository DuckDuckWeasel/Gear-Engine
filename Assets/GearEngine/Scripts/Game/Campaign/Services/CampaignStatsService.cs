using System.Linq;
using GearEngine.Campaign.Gear;
using GearEngine.CarSimulation.Definitions;
using GearEngine.GearEngine;

namespace GearEngine.Campaign.Services
{
    public interface ICampaignStatsService
    {
        RoguelikeCarStats GetBaseStats();
        RoguelikeCarStats GetCalculatedStats();
    }

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
            RoguelikeCarStats currentStats = sessionTemplate.RoguelikeStats;

            if (gearEngine != null)
            {
                foreach (var node in gearEngine.GetAllNodes())
                {
                    if (node == null) continue;
                    foreach (var ability in node.GetAbilities())
                    {
                        if (ability is PassiveRaceGearAbilitySO passiveGear)
                        {
                            passiveGear.ApplyPassiveStats(ref currentStats, node, gearEngine);
                        }
                    }
                }
            }

            return currentStats;
        }
    }
}
