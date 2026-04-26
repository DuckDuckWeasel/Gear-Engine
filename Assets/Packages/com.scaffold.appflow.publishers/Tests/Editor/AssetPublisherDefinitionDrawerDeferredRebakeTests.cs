using System;
using System.Collections.Generic;
using NUnit.Framework;
using Scaffold.AppFlow.Publishers.DataDriven;
using UnityEditor;
using UnityEngine;

namespace Scaffold.AppFlow.Publishers.Tests.Editor
{
    [TestFixture]
    public sealed class AssetPublisherDefinitionDrawerDeferredRebakeTests
    {
        [TearDown]
        public void TearDown()
        {
            AssetPublisherDefinitionDrawer.ResetSourceTypeCacheForTests();
            CountingBakeSource.BakeInvocations = 0;
        }

        [Test]
        public void DeferredRebake_CoalescesSameTargetPath()
        {
            var host = ScriptableObject.CreateInstance<TestPublisherHost>();
            host.SetDefinitionForTests(
                new AssetPublisherDefinition
                {
                });
            host.GetDefinitionForTests().SetSourceForTests(new CountingBakeSource());

            var so = new SerializedObject(host);
            string path = nameof(TestPublisherHost.definition);
            AssetPublisherDefinitionDrawer.EnqueueDeferredRebakeForTests(so, path);
            AssetPublisherDefinitionDrawer.EnqueueDeferredRebakeForTests(so, path);
            AssetPublisherDefinitionDrawer.FlushPendingRebakesForTests();

            Assert.AreEqual(1, AssetPublisherDefinitionDrawer.DeferredRebakeWorkItemCount);
            Assert.AreEqual(1, CountingBakeSource.BakeInvocations);
        }

        private sealed class TestPublisherHost : ScriptableObject, IAssetPublisherDefinitionHost
        {
            [SerializeField]
            private AssetPublisherDefinition definition;

            public IReadOnlyList<AssetPublisherDefinition> AssetPublisherDefinitions =>
                new[] { definition };

            internal void SetDefinitionForTests(AssetPublisherDefinition value) => definition = value;

            internal AssetPublisherDefinition GetDefinitionForTests() => definition;
        }

        [Serializable]
        public sealed class CountingBakeSource : IAssetPublisherSource
        {
            public static int BakeInvocations;

            public bool IsConfigured => true;

            public IPublisherRegistrar Bake()
            {
                BakeInvocations++;
                return null;
            }
        }
    }
}
