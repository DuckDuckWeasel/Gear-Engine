using System;
using GearEngine.CarSimulation.Definitions;

namespace GearEngine.Campaign.Services
{
    /// <summary>Builds per-race <see cref="RaceSessionConfig"/> from campaign defaults and the current track.</summary>
    public sealed class CampaignRaceSessionDefaults
    {
        private readonly RaceSessionConfig template;

        public CampaignRaceSessionDefaults(RaceSessionConfig template)
        {
            this.template = template ?? new RaceSessionConfig();
        }

        public RaceSessionConfig CreateForTrack(TrackDefinition track)
        {
            if (track == null)
            {
                throw new ArgumentNullException(nameof(track));
            }

            RaceSessionConfig config = template.CloneForNewRace();
            config.ApplyFromTrack(track);
            return config;
        }
    }
}
