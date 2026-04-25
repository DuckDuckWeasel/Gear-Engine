using System;
using System.Collections.Generic;
using GearEngine.Campaign.Services;
using GearEngine.CarSimulation.Definitions;
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

            builder.Register(c => new TrackAssetIndex(
                    c.Resolve<IReadOnlyList<TrackDefinition>>(),
                    c.Resolve<CarDefinition>()),
                Lifetime.Singleton);

            builder.Register<TracksClientModule>(Lifetime.Singleton)
                .As<ITrackService>()
                .AsSelf()
                .As<IGameClientModule>()
                .As<IAsyncInitializable>();
        }
    }
}
