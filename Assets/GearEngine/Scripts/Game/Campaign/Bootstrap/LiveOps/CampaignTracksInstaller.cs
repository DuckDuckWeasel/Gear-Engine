using System;
using GearEngine.Campaign.Services;
using GearEngine.LayeredScope;
using Scaffold.LiveOps;
using VContainer;
using VContainer.Unity;

namespace GearEngine.Campaign.Bootstrap.LiveOps
{
    public sealed class CampaignTracksInstaller : IInstaller
    {
        private readonly TrackCatalogSO catalog;

        public CampaignTracksInstaller(TrackCatalogSO catalog)
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
            builder.Register<TracksClientModule>(Lifetime.Singleton)
                .As<ITrackService>()
                .AsSelf()
                .As<IGameClientModule>()
                .As<IAsyncInitializable>();
        }
    }
}
