using System;
using GearEngine.CarSimulation.Definitions;

namespace GearEngine.Campaign.Services
{
    public sealed class CampaignRaceSessionDefaults
    {
        public CampaignRaceSessionDefaults(RaceSessionConfig template, ICampaignStatsService statsService)
        {
            this.template = template ?? new RaceSessionConfig();
            this.statsService = statsService;
        }

        private readonly RaceSessionConfig template;
        private readonly ICampaignStatsService statsService;

        public RaceSessionConfig CreateForTrack(TrackDefinition track)
        {
            if (track == null)
            {
                throw new ArgumentNullException(nameof(track));
            }

            RaceSessionConfig config = template.CloneForNewRace();

            if (statsService != null)
            {
                config.SetRoguelikeStats(statsService.GetCalculatedStats());
            }

            return config;
        }
    }
}
