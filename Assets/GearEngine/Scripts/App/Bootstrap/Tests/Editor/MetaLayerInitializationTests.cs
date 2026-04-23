using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using GameModuleDTO.GameApi;
using GameModuleDTO.ModuleRequests;
using Scaffold.AppFlow;
using NUnit.Framework;
using Scaffold.CloudCode;
using Scaffold.LiveOps;
using Scaffold.LiveOps.Container;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;
using VContainer.Unity;

namespace GearEngine.App.Bootstrap.Tests.Editor
{
    [TestFixture]
    public sealed class MetaLayerInitializationTests
    {
        [Test]
        public async Task SequentialLayers_RunUgsWaveBeforeLiveOpsWave()
        {
            var go = new GameObject("MetaLayerOrderTest");
            go.SetActive(false);
            try
            {
                var bootstrap = go.AddComponent<OrderRecordingBootstrap>();
                go.SetActive(true);
                InvokeStart(bootstrap);
                await bootstrap.ReadyTask;
                Assert.That(
                    bootstrap.Order,
                    Is.EqualTo(new[] { "foundation", "ugs_done", "liveops_done", "on_ready" }),
                    "Init waves must complete in stack order before OnReadyAsync.");
            }
            finally
            {
                if (go != null)
                {
                    UnityEngine.Object.DestroyImmediate(go);
                }
            }
        }

        [Test]
        [Timeout(10000)]
        public async Task LiveOpsLayerComposition_GameApiOptimistic_ReturnsBeforeSlowServerCompletes()
        {
            var serverGate = new TaskCompletionSource<GameApiEnvelopeResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
            var validateTcs = new TaskCompletionSource<(int ServerId, int OptimisticId)>(TaskCreationOptions.RunContinuationsAsynchronously);

            var optimisticHandler = new MetaOptimisticHandler
            {
                OptimisticValue = new MetaOptimisticResponse { Id = 1 },
                ValidateCallback = (server, optimistic) => validateTcs.TrySetResult((server.Id, optimistic.Id)),
            };

            var fakeCloud = new MetaFakeCloudCodeService(() => serverGate.Task);

            ILiveOpsService liveOps = BuildLiveOpsLayerStyleContainer(fakeCloud, optimisticHandler);

            Task<MetaOptimisticResponse> call = liveOps.CallAsync(new MetaOptimisticRequest { Marker = "meta-test" }, CancellationToken.None);
            MetaOptimisticResponse returned = await call.ConfigureAwait(false);
            Assert.That(returned.Id, Is.EqualTo(1));
            Assert.That(validateTcs.Task.IsCompleted, Is.False, "Validate must run only after the GameApi envelope completes.");

            serverGate.TrySetResult(
                GameApiEnvelopeResponse.Success(nameof(MetaOptimisticRequest), new MetaOptimisticResponse { Id = 99 }, null));

            (int serverId, int optimisticId) = await validateTcs.Task.ConfigureAwait(false);
            Assert.That(serverId, Is.EqualTo(99));
            Assert.That(optimisticId, Is.EqualTo(1));
        }

        [Test]
        public async Task LiveOpsInitThrows_InvokesOnStartupFailedAsync_AndReadyTaskFaults()
        {
            var go = new GameObject("MetaFailureTest");
            go.SetActive(false);
            try
            {
                var bootstrap = go.AddComponent<FailingLiveOpsBootstrap>();
                go.SetActive(true);

                LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("\\[AppFlow\\] Init layer 'ThrowingLiveOpsLayer'"));
                LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("\\[AppFlow\\] Startup AppFlowRoot"));

                InvokeStart(bootstrap);

                Exception faulted = null;
                try
                {
                    await bootstrap.ReadyTask;
                }
                catch (Exception ex)
                {
                    faulted = ex;
                }

                Assert.That(faulted, Is.Not.Null, "ReadyTask should have faulted.");
                Assert.That(bootstrap.StartupFailureCalled, Is.True);
                Assert.That(bootstrap.StartupFailureException, Is.SameAs(faulted));
            }
            finally
            {
                if (go != null)
                {
                    UnityEngine.Object.DestroyImmediate(go);
                }
            }
        }

        private static void InvokeStart(AppFlowRoot bootstrap)
        {
            MethodInfo start = typeof(AppFlowRoot).GetMethod(
                "Start",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(start, Is.Not.Null);
            start.Invoke(bootstrap, Array.Empty<object>());
        }

        /// <summary>
        /// Mirrors <see cref="GearEngine.App.Bootstrap.Layers.LiveOpsServiceLayer"/> then
        /// <see cref="GearEngine.App.Bootstrap.Layers.LiveOpsClientModulesLayer"/> registration order for EditMode:
        /// Cloud Code client + optimistic registry + LiveOps (real <see cref="LiveOpsService"/>), with a fake <see cref="ICloudCodeService"/> so no network/SDK call runs.
        /// </summary>
        private static ILiveOpsService BuildLiveOpsLayerStyleContainer(
            ICloudCodeService cloudCode,
            IOptimisticCloudCodeHandler optimisticHandler)
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(cloudCode).As<ICloudCodeService>();
            builder.Register<CloudCodeOptimisticHandlerRegistry>(Lifetime.Singleton);
            builder.RegisterInstance(new CloudCodeErrorHandler());
            builder.RegisterInstance(MetaNoMatchResponseHandler.Instance).As<IResponseHandler>();
            builder.RegisterInstance(optimisticHandler).As<IOptimisticCloudCodeHandler>().AsImplementedInterfaces();
            new LiveOpsInstaller().Install(builder);
            IObjectResolver container = builder.Build();
            return container.Resolve<ILiveOpsService>();
        }

        [UsesGameApi]
        private sealed class MetaOptimisticRequest : ModuleRequest<MetaOptimisticResponse>
        {
            public string Marker { get; set; }
        }

        private sealed class MetaOptimisticResponse : ModuleResponse
        {
            public int Id { get; set; }
        }

        private sealed class MetaOptimisticHandler : IRequestHandler<MetaOptimisticRequest, MetaOptimisticResponse>, IOptimisticCloudCodeHandler
        {
            public Type RequestClrType => typeof(MetaOptimisticRequest);

            public Type ResponseClrType => typeof(MetaOptimisticResponse);

            public MetaOptimisticResponse OptimisticValue { get; init; }

            public Action<MetaOptimisticResponse, MetaOptimisticResponse> ValidateCallback { get; init; }

            public bool TryMatch(string module, string endpoint, MetaOptimisticRequest request)
            {
                return module == "LiveOps" && endpoint == "GameApi";
            }

            public MetaOptimisticResponse GetOptimisticResponse(MetaOptimisticRequest request)
            {
                return OptimisticValue;
            }

            public void Validate(MetaOptimisticResponse serverResponse, MetaOptimisticResponse optimisticResponse)
            {
                ValidateCallback?.Invoke(serverResponse, optimisticResponse);
            }
        }

        private sealed class MetaNoMatchResponseStub : ModuleResponse
        {
        }

        private sealed class MetaNoMatchResponseHandler : IResponseHandler
        {
            internal static readonly MetaNoMatchResponseHandler Instance = new MetaNoMatchResponseHandler();

            private MetaNoMatchResponseHandler()
            {
            }

            public Type HandledResponseType => typeof(MetaNoMatchResponseStub);

            public void Handle(ModuleResponse response)
            {
            }
        }

        private sealed class MetaFakeCloudCodeService : ICloudCodeService
        {
            private readonly Func<Task<GameApiEnvelopeResponse>> onGameApi;

            public MetaFakeCloudCodeService(Func<Task<GameApiEnvelopeResponse>> onGameApi)
            {
                this.onGameApi = onGameApi ?? throw new ArgumentNullException(nameof(onGameApi));
            }

            public async Task<T> CallEndpointAsync<T>(string module, string endpoint, object payload = null, CancellationToken cancellationToken = default)
            {
                if (endpoint == "GameApi")
                {
                    GameApiEnvelopeResponse envelope = await onGameApi().ConfigureAwait(false);
                    return (T)(object)envelope;
                }

                throw new InvalidOperationException($"Unexpected endpoint '{endpoint}' in meta optimistic test fake.");
            }
        }

        private sealed class OrderRecorder
        {
            public List<string> Order { get; } = new();
        }

        private sealed class OrderRecordingBootstrap : AppFlowRoot
        {
            private readonly OrderRecorder ledger = new();

            public List<string> Order => ledger.Order;

            protected override IEnumerable<IScopeLayer> GetInitialLayers()
            {
                yield return new FoundationStubLayer(ledger);
                yield return new UgsStubLayer(ledger);
                yield return new LiveOpsStubLayer(ledger);
            }

            protected override Task OnReadyAsync(CancellationToken ct)
            {
                ledger.Order.Add("on_ready");
                return Task.CompletedTask;
            }
        }

        private sealed class FoundationStubLayer : IScopeLayer
        {
            private readonly OrderRecorder ledger;

            public FoundationStubLayer(OrderRecorder ledger)
            {
                this.ledger = ledger;
            }

            public void Install(IContainerBuilder builder)
            {
                builder.RegisterInstance(ledger);
                builder.Register<FoundationMarker>(Lifetime.Singleton).As<IAsyncInitializable>();
            }

            private sealed class FoundationMarker : IAsyncInitializable
            {
                private readonly OrderRecorder ledger;

                public FoundationMarker(OrderRecorder ledger)
                {
                    this.ledger = ledger;
                }

                public Task InitializeAsync(CancellationToken ct)
                {
                    ledger.Order.Add("foundation");
                    return Task.CompletedTask;
                }
            }
        }

        private sealed class UgsStubLayer : IScopeLayer
        {
            private readonly OrderRecorder ledger;

            public UgsStubLayer(OrderRecorder ledger)
            {
                this.ledger = ledger;
            }

            public void Install(IContainerBuilder builder)
            {
                builder.Register<UgsMarker>(Lifetime.Singleton).As<IAsyncInitializable>();
            }

            private sealed class UgsMarker : IAsyncInitializable
            {
                private readonly OrderRecorder ledger;

                public UgsMarker(OrderRecorder ledger)
                {
                    this.ledger = ledger;
                }

                public Task InitializeAsync(CancellationToken ct)
                {
                    ledger.Order.Add("ugs_done");
                    return Task.CompletedTask;
                }
            }
        }

        private sealed class LiveOpsStubLayer : IScopeLayer
        {
            private readonly OrderRecorder ledger;

            public LiveOpsStubLayer(OrderRecorder ledger)
            {
                this.ledger = ledger;
            }

            public void Install(IContainerBuilder builder)
            {
                builder.Register<LiveOpsMarker>(Lifetime.Singleton).As<IAsyncInitializable>();
            }

            private sealed class LiveOpsMarker : IAsyncInitializable
            {
                private readonly OrderRecorder ledger;

                public LiveOpsMarker(OrderRecorder ledger)
                {
                    this.ledger = ledger;
                }

                public Task InitializeAsync(CancellationToken ct)
                {
                    ledger.Order.Add("liveops_done");
                    return Task.CompletedTask;
                }
            }
        }

        private sealed class FailingLiveOpsBootstrap : AppFlowRoot
        {
            public bool StartupFailureCalled { get; private set; }
            public Exception StartupFailureException { get; private set; }

            protected override IEnumerable<IScopeLayer> GetInitialLayers()
            {
                yield return new ThrowingLiveOpsLayer();
            }

            protected override Task OnStartupFailedAsync(Exception ex, CancellationToken ct)
            {
                StartupFailureCalled = true;
                StartupFailureException = ex;
                return Task.CompletedTask;
            }
        }

        private sealed class ThrowingLiveOpsLayer : IScopeLayer
        {
            public void Install(IContainerBuilder builder)
            {
                builder.Register<BoomInit>(Lifetime.Singleton).As<IAsyncInitializable>();
            }

            private sealed class BoomInit : IAsyncInitializable
            {
                public Task InitializeAsync(CancellationToken ct)
                {
                    throw new InvalidOperationException("liveops init boom");
                }
            }
        }
    }
}
