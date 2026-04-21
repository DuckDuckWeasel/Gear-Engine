using System;
using GearEngine.CarSimulation.Definitions;

namespace GearEngine.Campaign.Services
{
    /// <summary>Builds per-race <see cref="RaceSessionConfig"/> from campaign defaults and the current track.</summary>
    public sealed class CampaignRaceSessionDefaults
    {
        private readonly RaceSessionConfig template;
        private readonly ICampaignStatsService statsService;

        public CampaignRaceSessionDefaults(RaceSessionConfig template, ICampaignStatsService statsService)
        {
            this.template = template ?? new RaceSessionConfig();
            this.statsService = statsService;
        }

        public RaceSessionConfig CreateForTrack(TrackDefinition track)
        {
            if (track == null)
            {
                throw new ArgumentNullException(nameof(track));
            }

            RaceSessionConfig config = template.CloneForNewRace();
            config.ApplyFromTrack(track);

            if (statsService != null)
            {
                config.SetRoguelikeStats(statsService.GetCalculatedStats());
            }

            return config;
        }
    }
}
