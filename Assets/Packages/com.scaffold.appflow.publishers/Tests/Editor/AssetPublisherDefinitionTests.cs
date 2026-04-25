using NUnit.Framework;
using Scaffold.AppFlow.Publishers.Addressables;
using Scaffold.AppFlow.Publishers.DataDriven;
using VContainer;
using System;

#if UNITY_EDITOR
using UnityEngine;
#endif

namespace Scaffold.AppFlow.Publishers.Tests.Editor
{
    [TestFixture]
    public sealed class AssetPublisherDefinitionTests
    {
        [Test]
        public void Register_WithoutBaked_Throws()
        {
            var def = new AssetPublisherDefinition();
            def.SetSourceForTests(new UnbakedTestSource());
            var builder = new ContainerBuilder();
            Assert.Throws<InvalidOperationException>(() => def.Register(builder));
        }

        [Test]
        public void Register_WithNullBuilder_Throws()
        {
            var def = new AssetPublisherDefinition();
            def.SetSourceForTests(new UnbakedTestSource());
            Assert.Throws<ArgumentNullException>(() => def.Register(null));
        }

#if UNITY_EDITOR
        [Test]
        public void Register_AddressableLabelSource_WithoutBakedFallsBackToRuntimeBaked_Registers()
        {
            var s = new AddressableLabelSource();
            s.SetLabelAndTypeForEditor("liveops.test", "UnityEngine.ScriptableObject, UnityEngine.CoreModule");
            var def = new AssetPublisherDefinition();
            def.SetSourceForTests(s);
            def.ClearBake();

            var builder = new ContainerBuilder();
            def.Register(builder);
            Assert.DoesNotThrow(() => builder.Build());
        }
#endif

        [Serializable]
        private sealed class UnbakedTestSource : IAssetPublisherSource
        {
#if UNITY_EDITOR
            public bool IsConfigured => false;

            public IPublisherRegistrar Bake() => null;
#endif
        }
    }
}
