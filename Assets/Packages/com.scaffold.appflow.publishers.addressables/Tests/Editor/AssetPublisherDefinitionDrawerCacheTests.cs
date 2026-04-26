using NUnit.Framework;
using Scaffold.AppFlow.Publishers.Editor;

namespace Scaffold.AppFlow.Publishers.Addressables.Tests.Editor
{
    [TestFixture]
    public sealed class AssetPublisherDefinitionDrawerCacheTests
    {
        [TearDown]
        public void TearDown()
        {
            AssetPublisherDefinitionDrawer.ResetSourceTypeCacheForTests();
        }

        [Test]
        public void SourceTypeCache_MultipleEnsure_RebuildsOnce()
        {
            AssetPublisherDefinitionDrawer.ResetSourceTypeCacheForTests();
            int a = AssetPublisherDefinitionDrawer.SourceTypeCacheRebuilds;
            AssetPublisherDefinitionDrawer.EnsureSourceTypeCacheForTests();
            int b = AssetPublisherDefinitionDrawer.SourceTypeCacheRebuilds;
            Assert.Greater(b, a);
            AssetPublisherDefinitionDrawer.EnsureSourceTypeCacheForTests();
            Assert.AreEqual(b, AssetPublisherDefinitionDrawer.SourceTypeCacheRebuilds);
        }

        [Test]
        public void SourceTypeCache_Reset_CanRebuild()
        {
            AssetPublisherDefinitionDrawer.EnsureSourceTypeCacheForTests();
            int b = AssetPublisherDefinitionDrawer.SourceTypeCacheRebuilds;
            AssetPublisherDefinitionDrawer.ResetSourceTypeCacheForTests();
            AssetPublisherDefinitionDrawer.EnsureSourceTypeCacheForTests();
            Assert.Greater(AssetPublisherDefinitionDrawer.SourceTypeCacheRebuilds, b);
        }
    }
}
