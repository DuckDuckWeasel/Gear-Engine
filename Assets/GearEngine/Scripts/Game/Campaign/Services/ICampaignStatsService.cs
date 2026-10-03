using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.PhysicsSimulation;

namespace GearEngine.Campaign.Services
{
    public interface ICampaignStatsService
    {
        RoguelikeCarStats GetBaseStats();
        RoguelikeCarStats GetCalculatedStats();
        RoguelikeCarStats GetCalculatedStats(TrackDefinition track);
    }
}
