using System;
using GearEngine.GearEngine.Config;
using VContainer;
using VContainer.Unity;

namespace GearEngine.Campaign.Bootstrap.LiveOps
{
    /// <summary>Registers a shared <see cref="GearCatalogSO"/> for loadout and inventory client modules.</summary>
    public sealed class CampaignGearCatalogInstaller : IInstaller
    {
        private readonly GearCatalogSO catalog;

        public CampaignGearCatalogInstaller(GearCatalogSO catalog)
        {
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        public void Install(IContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.RegisterInstance(catalog);
        }
    }
}
