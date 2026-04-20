using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using GearEngine.LayeredScope;
using NUnit.Framework;
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
        public async Task LiveOpsInitThrows_InvokesOnStartupFailedAsync_AndReadyTaskFaults()
        {
            var go = new GameObject("MetaFailureTest");
            go.SetActive(false);
            try
            {
                var bootstrap = go.AddComponent<FailingLiveOpsBootstrap>();
                go.SetActive(true);

                LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Push failed for 'ThrowingLiveOpsLayer'"));
                LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("\\[ApplicationBootstrap\\] Startup failed"));

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

        private static void InvokeStart(ApplicationBootstrap bootstrap)
        {
            MethodInfo start = typeof(ApplicationBootstrap).GetMethod(
                "Start",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(start, Is.Not.Null);
            start.Invoke(bootstrap, Array.Empty<object>());
        }

        private sealed class OrderRecorder
        {
            public List<string> Order { get; } = new();
        }

        private sealed class OrderRecordingBootstrap : ApplicationBootstrap
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

        private sealed class FailingLiveOpsBootstrap : ApplicationBootstrap
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
