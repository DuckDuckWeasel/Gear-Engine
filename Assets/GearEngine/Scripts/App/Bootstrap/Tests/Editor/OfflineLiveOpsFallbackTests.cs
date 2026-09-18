using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GearEngine.App.Bootstrap.Layers;
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

        private static IObjectResolver BuildContainer(
            ICloudCodeService cloudCodeService,
            OfflineSessionState offlineSessionState)
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterInstance(cloudCodeService).As<ICloudCodeService>();
            builder.RegisterInstance(offlineSessionState);
            builder.RegisterInstance<ILayerResolver>(NullLayerResolver.s_instance);
            new ResilientLiveOpsInstaller().Install(builder);
            return builder.Build();
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
