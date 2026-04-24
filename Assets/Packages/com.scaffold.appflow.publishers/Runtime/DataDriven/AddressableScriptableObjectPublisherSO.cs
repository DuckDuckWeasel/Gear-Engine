using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;

namespace Scaffold.AppFlow.Publishers.DataDriven
{
    /// <summary>Authoring asset for data-driven Addressables ScriptableObject publishers; baked at edit time via the custom inspector or the Tools menu.</summary>
    [CreateAssetMenu(
        fileName = "AddressableScriptableObjectPublisher",
        menuName = "Scaffold/AppFlow/Addressable ScriptableObject Publisher",
        order = 0)]
    public sealed class AddressableScriptableObjectPublisherSO : ScriptableObject
    {
        public AssetReferenceT<ScriptableObject> AssetReference => assetReference;

        [SerializeField]
        private AssetReferenceT<ScriptableObject> assetReference;

        // Test-only accessor; exposed via InternalsVisibleTo to Scaffold.AppFlow.Publishers.Tests.
        internal IPublisherRegistrar BakedRegistrarForTests => bakedRegistrar;

        [SerializeReference]
        [HideInInspector]
        private IPublisherRegistrar bakedRegistrar;

        public void Register(IContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (bakedRegistrar == null)
            {
                throw new InvalidOperationException(
                    $"[{nameof(AddressableScriptableObjectPublisherSO)}] '{name}' has no baked registrar. Select the asset in the Project window and click Rebuild, or run Tools → Scaffold → AppFlow → Rebake All Publisher SOs.");
            }

            bakedRegistrar.Register(builder);
        }

        // Test-only hook to assign bake without the asset pipeline.
        internal void SetBakedRegistrarForTests(IPublisherRegistrar registrar)
        {
            bakedRegistrar = registrar;
        }

        internal void ClearBakedRegistrarForTests()
        {
            bakedRegistrar = null;
        }
    }
}
