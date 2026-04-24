using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GearEngine.App.Bootstrap;
using GearEngine.App.Bootstrap.Publishers.DataDriven;
using GearEngine.Campaign.Services;
using GearEngine.GearEngine.Config;
using NUnit.Framework;
using Scaffold.Addressables.Contracts;
using Scaffold.AppFlow;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GearEngine.App.Bootstrap.Tests.Editor
{
    [TestFixture]
    public sealed class DataDrivenAddressableScriptableObjectPublisherTests
    {
        [Test]
        public async Task TrackCatalog_PublishesLoadedCatalogToLayerPublisher()
        {
            await VerifyPublisherForwardsAssetAsync(
                AddressableCatalogAddresses.Track,
                ScriptableObject.CreateInstance<TrackCatalogSO>(),
                (lp, client, key) => new DataDrivenAddressableScriptableObjectPublisher<TrackCatalogSO>(lp, client, key));
        }

        [Test]
        public async Task GearCatalog_PublishesLoadedCatalogToLayerPublisher()
        {
            await VerifyPublisherForwardsAssetAsync(
                AddressableCatalogAddresses.Gear,
                ScriptableObject.CreateInstance<GearCatalogSO>(),
                (lp, client, key) => new DataDrivenAddressableScriptableObjectPublisher<GearCatalogSO>(lp, client, key));
        }

        [Test]
        public async Task RoguelikeGearPool_PublishesLoadedPoolToLayerPublisher()
        {
            await VerifyPublisherForwardsAssetAsync(
                AddressableCatalogAddresses.RoguelikeGearPool,
                ScriptableObject.CreateInstance<RoguelikeGearPoolSO>(),
                (lp, client, key) => new DataDrivenAddressableScriptableObjectPublisher<RoguelikeGearPoolSO>(lp, client, key));
        }

        private static async Task VerifyPublisherForwardsAssetAsync<T>(
            string address,
            T asset,
            Func<ILayerPublisher, IAddressablesAssetClient, string, IAsyncInitializable> factory)
            where T : ScriptableObject
        {
            try
            {
                var client = new StubAddressablesAssetClient();
                client.Register(address, asset);
                var publisher = new RecordingLayerPublisher();

                IAsyncInitializable underTest = factory(publisher, client, address);
                await underTest.InitializeAsync(CancellationToken.None);

                Assert.That(publisher.PublishedByType.TryGetValue(typeof(T), out object published), Is.True,
                    $"{typeof(T).Name} must be published to ILayerPublisher.");
                Assert.That(published, Is.SameAs(asset));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(asset);
            }
        }

        private sealed class RecordingLayerPublisher : ILayerPublisher
        {
            public Dictionary<Type, object> PublishedByType { get; } = new Dictionary<Type, object>();

            public void Publish<TPub>(TPub item) where TPub : class
            {
                PublishedByType[typeof(TPub)] = item;
            }

            public void Publish<TInterface, TImpl>(TImpl item) where TImpl : class, TInterface
            {
                PublishedByType[typeof(TInterface)] = item;
                PublishedByType[typeof(TImpl)] = item;
            }

            public void PublishMany<TPub>(IReadOnlyList<TPub> items) where TPub : class
            {
                PublishedByType[typeof(IReadOnlyList<TPub>)] = items;
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

            public Task<IReadOnlyList<T>> LoadAssetsByLabelAsync<T>(AssetLabelReference label, CancellationToken cancellationToken) where T : UnityEngine.Object
            {
                throw new NotSupportedException();
            }

            public Task<IReadOnlyList<string>> ResolveLabelAsync<T>(AssetLabelReference label, CancellationToken cancellationToken) where T : UnityEngine.Object
            {
                throw new NotSupportedException();
            }

            public void Release(UnityEngine.Object asset)
            {
            }
        }
    }
}
