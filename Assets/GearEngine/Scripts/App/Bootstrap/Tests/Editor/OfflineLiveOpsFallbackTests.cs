using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GearEngine.App.Bootstrap.Layers;
using GearEngine.GearEngine.Config;
using LiveOps.DTO.GameApi;
using LiveOps.Modules.DTO.Currency;
using LiveOps.Modules.DTO.Inventory;
using LiveOps.Modules.DTO.ModuleRequests;
using LiveOps.Modules.DTO.Perks;
using LiveOps.Modules.DTO.Roguelike;
using LiveOps.Modules.DTO.Tracks;
using NUnit.Framework;
using Scaffold.AppFlow;
using Scaffold.CloudCode;
using Scaffold.LiveOps;
using VContainer;
using UnityEditor;

namespace GearEngine.App.Bootstrap.Tests.Editor
{
    [TestFixture]
    public sealed class OfflineLiveOpsFallbackTests
    {
        [Test]
        [Timeout(5000)]
        public async Task InitializeAndRequest_WhenSessionIsOffline_UseLocalFallbackWithoutBlocking()
        {
            UnavailableCloudCodeService fakeCloud = new UnavailableCloudCodeService();
            OfflineSessionState offlineState = new OfflineSessionState();
            offlineState.MarkOffline();
            IObjectResolver container = BuildContainer(fakeCloud, offlineState);
            try
            {
                IAsyncInitializable initializer = container.Resolve<IAsyncInitializable>();
                await initializer.InitializeAsync(CancellationToken.None);

                ILiveOpsService liveOps = container.Resolve<ILiveOpsService>();
                Assert.That(liveOps.GetModuleData<CurrencyGameData>(), Is.Not.Null);
                Assert.That(liveOps.GetModuleData<InventoryGameData>(), Is.Not.Null);
                Assert.That(liveOps.GetModuleData<PerkGameData>(), Is.Not.Null);
                Assert.That(liveOps.GetModuleData<RoguelikeGameData>(), Is.Not.Null);
                Assert.That(liveOps.GetModuleData<TrackGameData>(), Is.Not.Null);

                AddCurrencyResponse response = await liveOps.CallAsync(
                    new AddCurrencyRequest("gold", 5),
                    CancellationToken.None);

                Assert.That(response.NewAmount, Is.EqualTo(20));
                Assert.That(offlineState.IsOffline, Is.True);
            }
            finally
            {
                (container as IDisposable)?.Dispose();
            }
        }

        [Test]
        [Timeout(5000)]
        public async Task Reroll_WhenSessionIsOffline_ReturnsUniqueChangingOptions()
        {
            GearCatalogSO catalog = AssetDatabase.LoadAssetAtPath<GearCatalogSO>(
                "Assets/GearEngine/Data/Campaign/Catalogs/CampaignGearCatalog.asset");
            Assert.That(catalog, Is.Not.Null);

            OfflineSessionState offlineState = new OfflineSessionState();
            offlineState.MarkOffline();
            IObjectResolver container = BuildContainer(
                new UnavailableCloudCodeService(),
                offlineState,
                new CatalogLayerResolver(catalog));
            try
            {
                IAsyncInitializable initializer = container.Resolve<IAsyncInitializable>();
                await initializer.InitializeAsync(CancellationToken.None);
                ILiveOpsService liveOps = container.Resolve<ILiveOpsService>();

                DrawRoguelikeRollResponse first = await liveOps.CallAsync(
                    new DrawRoguelikeRollRequest(),
                    CancellationToken.None);
                RerollRoguelikeRollResponse second = await liveOps.CallAsync(
                    new RerollRoguelikeRollRequest(),
                    CancellationToken.None);

                Assert.That(first.CurrentRollIds, Has.Count.EqualTo(3));
                Assert.That(first.CurrentRollIds, Is.Unique);
                Assert.That(second.CurrentRollIds, Has.Count.EqualTo(3));
                Assert.That(second.CurrentRollIds, Is.Unique);
                Assert.That(second.CurrentRollIds, Is.Not.EquivalentTo(first.CurrentRollIds));
            }
            finally
            {
                (container as IDisposable)?.Dispose();
            }
        }

        private static IObjectResolver BuildContainer(
            ICloudCodeService cloudCodeService,
            OfflineSessionState offlineSessionState,
            ILayerResolver layerResolver = null)
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterInstance(cloudCodeService).As<ICloudCodeService>();
            builder.RegisterInstance(offlineSessionState);
            builder.RegisterInstance(layerResolver ?? NullLayerResolver.s_instance).As<ILayerResolver>();
            new ResilientLiveOpsInstaller().Install(builder);
            return builder.Build();
        }

        private sealed class CatalogLayerResolver : ILayerResolver
        {
            private readonly GearCatalogSO catalog;

            public CatalogLayerResolver(GearCatalogSO catalog)
            {
                this.catalog = catalog;
            }

            public IObjectResolver Top => null;

            public bool TryResolve<T>(out T value)
            {
                if (catalog is T match)
                {
                    value = match;
                    return true;
                }

                value = default;
                return false;
            }

            public T Resolve<T>()
            {
                throw new InvalidOperationException("Only optional catalog resolution is supported in this test.");
            }
        }

        private sealed class UnavailableCloudCodeService : ICloudCodeService
        {
            public Task<T> CallEndpointAsync<T>(
                string module,
                string endpoint,
                object payload = null,
                CancellationToken cancellationToken = default)
            {
                return Task.FromException<T>(new InvalidOperationException("Network unavailable."));
            }
        }

        private sealed class NullLayerResolver : ILayerResolver
        {
            internal static readonly NullLayerResolver s_instance = new NullLayerResolver();

            public IObjectResolver Top => null;

            public bool TryResolve<T>(out T value)
            {
                value = default;
                return false;
            }

            public T Resolve<T>()
            {
                throw new InvalidOperationException("No layer services are available in this test.");
            }
        }
    }
}
