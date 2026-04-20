using System;
using GearEngine.Campaign.Services;
using GearEngine.CarSimulation.Definitions;
using VContainer;
using VContainer.Unity;

namespace GearEngine.Campaign.Bootstrap
{
    /// <summary>Registers <see cref="CampaignRaceSessionDefaults"/> from a <see cref="RaceSessionDefaultsSO"/> asset.</summary>
    public sealed class CampaignRaceSessionInstaller : IInstaller
    {
        private readonly RaceSessionDefaultsSO defaultsSo;

        public CampaignRaceSessionInstaller(RaceSessionDefaultsSO defaultsSo)
        {
            this.defaultsSo = defaultsSo ?? throw new ArgumentNullException(nameof(defaultsSo));
        }

        public void Install(IContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            RaceSessionConfig template = defaultsSo.Template ?? new RaceSessionConfig();
            builder.RegisterInstance(new CampaignRaceSessionDefaults(template));
        }
    }
}
