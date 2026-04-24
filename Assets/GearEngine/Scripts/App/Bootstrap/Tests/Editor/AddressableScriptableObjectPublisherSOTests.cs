using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GearEngine.App.Bootstrap;
using GearEngine.GearEngine.Config;
using NUnit.Framework;
using Scaffold.Addressables.Contracts;
using Scaffold.AppFlow;
using Scaffold.AppFlow.Publishers.DataDriven;
using Scaffold.AppFlow.Publishers.Editor;
using UnityEditor;
using UnityEngine;
using VContainer;

namespace GearEngine.App.Bootstrap.Tests.Editor
{
    /// <summary>
    /// Host integration tests for <see cref="AddressableScriptableObjectPublisherSO"/> against the real Addressable
    /// catalog assets in this project. Contract-level coverage (no rebake required) lives in
    /// <c>Scaffold.AppFlow.Publishers.Tests</c>.
    /// </summary>
    [TestFixture]
    public sealed class AddressableScriptableObjectPublisherSOTests
    {
        private const string GearCatalogAddressableGuid = "f2f2f2f2f2f2f2f2f2f2f2f2f2f2f2f2";

        [Test]
        public void Rebake_WithAddressableGearCatalog_ProducesClosedGenericRegistrar()
        {
            AddressableScriptableObjectPublisherSO so = ScriptableObject.CreateInstance<AddressableScriptableObjectPublisherSO>();
            try
            {
                SerializedObject serializedObject = new SerializedObject(so);
                SerializedProperty assetRef = serializedObject.FindProperty("assetReference");
                Assert.That(assetRef, Is.Not.Null);
                SerializedProperty guidProp = assetRef.FindPropertyRelative("m_AssetGUID");
                Assert.That(guidProp, Is.Not.Null);
                guidProp.stringValue = GearCatalogAddressableGuid;
                serializedObject.ApplyModifiedProperties();

                AddressableScriptableObjectPublisherSORebaker.Rebake(so);

                Type bakedType = ReadBakedRegistrarType(so);
                Assert.That(bakedType, Is.Not.Null);
                Assert.That(bakedType, Is.EqualTo(typeof(AddressableScriptableObjectPublisherRegistrar<GearCatalogSO>)));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(so);
            }
        }

        [Test]
        public async Task Rebake_ThenRegisterAndInitialize_PublishesAsset()
        {
            GearCatalogSO asset = ScriptableObject.CreateInstance<GearCatalogSO>();
            AddressableScriptableObjectPublisherSO so = ScriptableObject.CreateInstance<AddressableScriptableObjectPublisherSO>();
            try
            {
                SerializedObject serializedObject = new SerializedObject(so);
                SerializedProperty assetRef = serializedObject.FindProperty("assetReference");
                assetRef.FindPropertyRelative("m_AssetGUID").stringValue = GearCatalogAddressableGuid;
                serializedObject.ApplyModifiedProperties();

                AddressableScriptableObjectPublisherSORebaker.Rebake(so);

                var builder = new ContainerBuilder();
                var layerPublisher = new RecordingLayerPublisher();
                var client = new StubAddressablesAssetClient();
                client.Register(AddressableCatalogAddresses.Gear, asset);
                builder.RegisterInstance<ILayerPublisher>(layerPublisher);
                builder.RegisterInstance<IAddressablesAssetClient>(client);
                so.Register(builder);

                IObjectResolver container = builder.Build();
                IAsyncInitializable init = container.Resolve<IAsyncInitializable>();
                await init.InitializeAsync(CancellationToken.None);

                Assert.That(layerPublisher.PublishedByType.TryGetValue(typeof(GearCatalogSO), out object published), Is.True);
                Assert.That(published, Is.SameAs(asset));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(so);
                UnityEngine.Object.DestroyImmediate(asset);
            }
        }

        [Test]
        public void BakedRegistrar_SerializedRoundTrip_PreservesManagedReference()
        {
            const string tempPath = "Assets/GearEngine/Scripts/App/Bootstrap/Tests/Editor/TempPublisherSORoundtrip.asset";
            AddressableScriptableObjectPublisherSO so = ScriptableObject.CreateInstance<AddressableScriptableObjectPublisherSO>();
            try
            {
                SerializedObject serializedObject = new SerializedObject(so);
                SerializedProperty assetRef = serializedObject.FindProperty("assetReference");
                assetRef.FindPropertyRelative("m_AssetGUID").stringValue = GearCatalogAddressableGuid;
                serializedObject.ApplyModifiedProperties();

                AddressableScriptableObjectPublisherSORebaker.Rebake(so);
                Assert.That(ReadBakedRegistrarType(so), Is.Not.Null, "Rebake must produce a registrar before disk round-trip.");

                AssetDatabase.CreateAsset(so, tempPath);
                AssetDatabase.SaveAssets();

                AddressableScriptableObjectPublisherSO reloaded =
                    AssetDatabase.LoadAssetAtPath<AddressableScriptableObjectPublisherSO>(tempPath);
                Assert.That(reloaded, Is.Not.Null);
                Type reloadedType = ReadBakedRegistrarType(reloaded);
                Assert.That(reloadedType, Is.Not.Null);
                Assert.That(reloadedType, Is.EqualTo(typeof(AddressableScriptableObjectPublisherRegistrar<GearCatalogSO>)));
            }
            finally
            {
                AssetDatabase.DeleteAsset(tempPath);
            }
        }

        private static Type ReadBakedRegistrarType(AddressableScriptableObjectPublisherSO so)
        {
            SerializedObject serialized = new SerializedObject(so);
            SerializedProperty baked = serialized.FindProperty("bakedRegistrar");
            return baked?.managedReferenceValue?.GetType();
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

        private sealed class StubAddressablesAssetClient : IAddressablesAssetClient
        {
            private readonly Dictionary<string, UnityEngine.Object> assets =
                new Dictionary<string, UnityEngine.Object>(StringComparer.Ordinal);

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
