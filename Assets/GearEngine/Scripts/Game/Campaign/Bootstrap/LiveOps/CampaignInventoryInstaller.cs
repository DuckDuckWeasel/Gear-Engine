using System;
using GearEngine.GearEngine.Services;
using Scaffold.LayeredScope;
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
                .As<IInventoryService>()
                .AsSelf()
                .As<IGameClientModule>()
                .As<IAsyncInitializable>();
        }
    }
}
