using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Scaffold.Addressables.Contracts;
using Scaffold.AppFlow;
using Scaffold.AppFlow.Publishers.DataDriven;
using UnityEngine;
using VContainer;

namespace Scaffold.AppFlow.Publishers.Tests.Editor
{
    /// <summary>
    /// Contract-level tests for <see cref="AddressableScriptableObjectPublisherSO"/> and
    /// <see cref="AddressableScriptableObjectPublisherRegistrar{T}"/>. Rebaker / end-to-end coverage that requires
    /// real Addressable assets lives in the consuming game's host tests.
    /// </summary>
    [TestFixture]
    public sealed class AddressableScriptableObjectPublisherSOTests
    {
        private const string TestAddressableKey = "Tests/Scaffold/AppFlow/Publishers/PublisherTestAsset";

        [Test]
        public void Register_WithoutBake_ThrowsInvalidOperationException()
        {
            AddressableScriptableObjectPublisherSO so = ScriptableObject.CreateInstance<AddressableScriptableObjectPublisherSO>();
            try
            {
                var builder = new ContainerBuilder();
                Assert.Throws<InvalidOperationException>(() => so.Register(builder));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(so);
            }
        }

        [Test]
        public void Register_WithNullBuilder_ThrowsArgumentNullException()
        {
            AddressableScriptableObjectPublisherSO so = ScriptableObject.CreateInstance<AddressableScriptableObjectPublisherSO>();
            try
            {
                so.SetBakedRegistrarForTests(new AddressableScriptableObjectPublisherRegistrar<PublisherTestAsset>(TestAddressableKey));
                Assert.Throws<ArgumentNullException>(() => so.Register(null));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(so);
            }
        }

        [Test]
        public void Register_WithBakedRegistrar_RegistersPublisherResolvableFromContainer()
        {
            AddressableScriptableObjectPublisherSO so = ScriptableObject.CreateInstance<AddressableScriptableObjectPublisherSO>();
            try
            {
                so.SetBakedRegistrarForTests(new AddressableScriptableObjectPublisherRegistrar<PublisherTestAsset>(TestAddressableKey));

                var builder = new ContainerBuilder();
                builder.RegisterInstance<ILayerPublisher>(new NoOpLayerPublisher());
                builder.RegisterInstance<IAddressablesAssetClient>(new NoOpAddressablesClient());
                so.Register(builder);

                IObjectResolver container = builder.Build();
                DataDrivenAddressableScriptableObjectPublisher<PublisherTestAsset> publisher =
                    container.Resolve<DataDrivenAddressableScriptableObjectPublisher<PublisherTestAsset>>();
                Assert.That(publisher, Is.Not.Null);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(so);
            }
        }

        [Test]
        public void Registrar_WithEmptyKey_ThrowsOnRegister()
        {
            var registrar = new AddressableScriptableObjectPublisherRegistrar<PublisherTestAsset>();
            var builder = new ContainerBuilder();
            Assert.Throws<InvalidOperationException>(() => registrar.Register(builder));
        }

        [Test]
        public void Registrar_WithNullKey_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new AddressableScriptableObjectPublisherRegistrar<PublisherTestAsset>(null));
        }

        [Test]
        public async Task BakedPublisher_InitializeAsync_PublishesLoadedAssetToLayerPublisher()
        {
            PublisherTestAsset asset = ScriptableObject.CreateInstance<PublisherTestAsset>();
            AddressableScriptableObjectPublisherSO so = ScriptableObject.CreateInstance<AddressableScriptableObjectPublisherSO>();
            try
            {
                so.SetBakedRegistrarForTests(new AddressableScriptableObjectPublisherRegistrar<PublisherTestAsset>(TestAddressableKey));

                var builder = new ContainerBuilder();
                var layerPublisher = new RecordingLayerPublisher();
                var client = new StubAddressablesAssetClient();
                client.Register(TestAddressableKey, asset);
                builder.RegisterInstance<ILayerPublisher>(layerPublisher);
                builder.RegisterInstance<IAddressablesAssetClient>(client);
                so.Register(builder);

                IObjectResolver container = builder.Build();
                IAsyncInitializable init = container.Resolve<IAsyncInitializable>();
                await init.InitializeAsync(CancellationToken.None);

                Assert.That(layerPublisher.PublishedByType.TryGetValue(typeof(PublisherTestAsset), out object published), Is.True);
                Assert.That(published, Is.SameAs(asset));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(so);
                UnityEngine.Object.DestroyImmediate(asset);
            }
        }

        [Test]
        public void ClearBakedRegistrar_RestoresUnbakedState()
        {
            AddressableScriptableObjectPublisherSO so = ScriptableObject.CreateInstance<AddressableScriptableObjectPublisherSO>();
            try
            {
                so.SetBakedRegistrarForTests(new AddressableScriptableObjectPublisherRegistrar<PublisherTestAsset>(TestAddressableKey));
                Assert.That(so.BakedRegistrarForTests, Is.Not.Null);

                so.ClearBakedRegistrarForTests();

                Assert.That(so.BakedRegistrarForTests, Is.Null);
                var builder = new ContainerBuilder();
                Assert.Throws<InvalidOperationException>(() => so.Register(builder));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(so);
            }
        }

        // Test-only ScriptableObject so the package fixture does not depend on any game type.
        private sealed class PublisherTestAsset : ScriptableObject
        {
        }

        private sealed class NoOpLayerPublisher : ILayerPublisher
        {
            public void Publish<T>(T item) where T : class
            {
            }

            public void Publish<TInterface, TImpl>(TImpl item) where TImpl : class, TInterface
            {
            }

            public void PublishMany<T>(IReadOnlyList<T> items) where T : class
            {
            }
        }

        private sealed class RecordingLayerPublisher : ILayerPublisher
        {
            public Dictionary<Type, object> PublishedByType { get; } = new Dictionary<Type, object>();

            public void Publish<T>(T item) where T : class
            {
                PublishedByType[typeof(T)] = item;
            }

            public void Publish<TInterface, TImpl>(TImpl item) where TImpl : class, TInterface
            {
                PublishedByType[typeof(TInterface)] = item;
                PublishedByType[typeof(TImpl)] = item;
            }

            public void PublishMany<T>(IReadOnlyList<T> items) where T : class
            {
                PublishedByType[typeof(IReadOnlyList<T>)] = items;
            }
        }

        private sealed class NoOpAddressablesClient : IAddressablesAssetClient
        {
            public Task SyncCatalogAndContentAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            public Task<T> LoadAssetAsync<T>(string key, CancellationToken cancellationToken) where T : UnityEngine.Object
            {
                throw new NotSupportedException();
            }

            public Task<IReadOnlyList<T>> LoadAssetsByLabelAsync<T>(
                UnityEngine.AddressableAssets.AssetLabelReference label,
                CancellationToken cancellationToken) where T : UnityEngine.Object
            {
                throw new NotSupportedException();
            }

            public Task<IReadOnlyList<string>> ResolveLabelAsync<T>(
                UnityEngine.AddressableAssets.AssetLabelReference label,
                CancellationToken cancellationToken) where T : UnityEngine.Object
            {
                throw new NotSupportedException();
            }

            public void Release(UnityEngine.Object asset)
            {
            }
        }

        private sealed class StubAddressablesAssetClient : IAddressablesAssetClient
        {
            private readonly Dictionary<string, UnityEngine.Object> assets = new Dictionary<string, UnityEngine.Object>(StringComparer.Ordinal);

            public void Register<T>(string key, T asset) where T : UnityEngine.Object
            {
                assets[key] = asset;
            }

            public Task SyncCatalogAndContentAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            public Task<T> LoadAssetAsync<T>(string key, CancellationToken cancellationToken) where T : UnityEngine.Object
            {
                if (assets.TryGetValue(key, out UnityEngine.Object obj) && obj is T cast)
                {
                    return Task.FromResult(cast);
                }

                throw new InvalidOperationException($"No stub asset for key '{key}' and type {typeof(T).Name}.");
            }

            public Task<IReadOnlyList<T>> LoadAssetsByLabelAsync<T>(
                UnityEngine.AddressableAssets.AssetLabelReference label,
                CancellationToken cancellationToken) where T : UnityEngine.Object
            {
                throw new NotSupportedException();
            }

            public Task<IReadOnlyList<string>> ResolveLabelAsync<T>(
                UnityEngine.AddressableAssets.AssetLabelReference label,
                CancellationToken cancellationToken) where T : UnityEngine.Object
            {
                throw new NotSupportedException();
            }

            public void Release(UnityEngine.Object asset)
            {
            }
        }
    }
}
