using System;
using GearEngine.Campaign.Services;
using Scaffold.AppFlow;
using Scaffold.LiveOps;
using VContainer;
using VContainer.Unity;

namespace GearEngine.Campaign.Bootstrap.LiveOps
{
    public sealed class CampaignTracksInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Register<TracksClientModule>(Lifetime.Singleton)
                .As<ITrackService>()
                .AsSelf()
                .As<IGameClientModule>()
                .As<IAsyncInitializable>();
        }
    }
}
