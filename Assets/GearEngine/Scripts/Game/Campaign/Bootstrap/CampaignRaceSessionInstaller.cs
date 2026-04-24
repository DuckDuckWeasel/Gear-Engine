using System;
using GearEngine.Campaign.Services;
using GearEngine.CarSimulation.Definitions;
using VContainer;
using VContainer.Unity;

namespace GearEngine.Campaign.Bootstrap
{
    /// <summary>Registers <see cref="CampaignRaceSessionDefaults"/> and <see cref="CampaignStatsService"/>; requires <see cref="RaceSessionConfig"/> and <see cref="GearEngine.GearEngine.IGearEngineService"/> in the container.</summary>
    public sealed class CampaignRaceSessionInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Register<CampaignStatsService>(Lifetime.Singleton)
                   .AsImplementedInterfaces();

            builder.Register<CampaignRaceSessionDefaults>(Lifetime.Singleton);
        }
    }
}
