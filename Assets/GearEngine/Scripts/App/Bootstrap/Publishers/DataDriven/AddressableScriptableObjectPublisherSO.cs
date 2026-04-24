using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;

namespace GearEngine.App.Bootstrap.Publishers.DataDriven
{
    // todo: Authoring asset for data-driven Addressables publishers; bake via Gear Bootstrap editor tools.
    [CreateAssetMenu(
        fileName = "AddressableScriptableObjectPublisher",
        menuName = "Gear/Bootstrap/Addressable ScriptableObject Publisher",
        order = 0)]
    public sealed class AddressableScriptableObjectPublisherSO : ScriptableObject
    {
        public AssetReferenceT<ScriptableObject> AssetReference => assetReference;

        [SerializeField]
        private AssetReferenceT<ScriptableObject> assetReference;

        // todo: Exposes bake for edit-mode tests only.
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
                    $"[{nameof(AddressableScriptableObjectPublisherSO)}] '{name}' has no baked registrar. Select the asset in the Project window and click Rebuild, or run Tools → Bootstrap → Rebake All Publisher SOs.");
            }

            bakedRegistrar.Register(builder);
        }

        // todo: Test-only hook to assign bake without the asset pipeline.
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
