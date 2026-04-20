using System;
using GearEngine.Campaign.Bootstrap.LiveOps;
using GearEngine.Campaign.Services;
using GearEngine.GearEngine.Config;
using GearEngine.LayeredScope;
using Scaffold.LiveOps;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GearEngine.Campaign.Bootstrap
{
    /// <summary>
    /// Registers Tracks / Loadout / Inventory LiveOps clients for the Meta application root so Play Mode
    /// pulls the same module payloads as Campaign (empty local catalogs; cloud ids still hydrate).
    /// </summary>
    public sealed class MetaLiveOpsClientInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            TrackCatalogSO trackCatalog = ScriptableObject.CreateInstance<TrackCatalogSO>();
            trackCatalog.SetRuntimeEntries(Array.Empty<TrackEntry>(), Array.Empty<GearConfig>());
            GearCatalogSO gearCatalog = ScriptableObject.CreateInstance<GearCatalogSO>();
            gearCatalog.SetRuntimeEntries(Array.Empty<GearConfig>());

            builder.RegisterInstance(trackCatalog);
            builder.RegisterInstance(gearCatalog);

            builder.Register<TracksClientModule>(Lifetime.Singleton)
                .As<ITrackService>()
                .AsSelf()
                .As<IGameClientModule>()
                .As<IAsyncInitializable>();

            builder.Register<LoadoutClientModule>(Lifetime.Singleton)
                .As<IGearLoadoutService>()
                .AsSelf()
                .As<IGameClientModule>()
                .As<IAsyncInitializable>();

            builder.Register<InventoryClientModule>(Lifetime.Singleton)
                .As<IOwnedGearInventoryService>()
                .AsSelf()
                .As<IGameClientModule>()
                .As<IAsyncInitializable>();
        }
    }
}
