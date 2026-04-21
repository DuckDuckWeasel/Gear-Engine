using System;
using GearEngine.Campaign.Services;
using Scaffold.LayeredScope;
using Scaffold.LiveOps;
using VContainer;
using VContainer.Unity;

namespace GearEngine.Campaign.Bootstrap.LiveOps
{
    /// <summary>Registers <see cref="LoadoutClientModule"/>; requires <see cref="GearEngine.GearEngine.Config.GearCatalogSO"/> from <see cref="CampaignGearCatalogInstaller"/>.</summary>
    public sealed class CampaignLoadoutInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Register<LoadoutClientModule>(Lifetime.Singleton)
                .As<IGearLoadoutService>()
                .AsSelf()
                .As<IGameClientModule>()
                .As<IAsyncInitializable>();
        }
    }
}
