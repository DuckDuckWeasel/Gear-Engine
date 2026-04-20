using System;
using GearEngine.Campaign.Services;
using GearEngine.LayeredScope;
using Scaffold.LiveOps;
using VContainer;
using VContainer.Unity;

namespace GearEngine.Campaign.Bootstrap.LiveOps
{
    /// <summary>Registers <see cref="InventoryClientModule"/>; requires <see cref="GearEngine.GearEngine.Config.GearCatalogSO"/> from <see cref="CampaignGearCatalogInstaller"/>.</summary>
    public sealed class CampaignInventoryInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Register<InventoryClientModule>(Lifetime.Singleton)
                .As<IOwnedGearInventoryService>()
                .AsSelf()
                .As<IGameClientModule>()
                .As<IAsyncInitializable>();
        }
    }
}
