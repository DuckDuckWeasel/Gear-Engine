using System;
using UnityEngine;
using VContainer;

namespace Scaffold.AppFlow.Publishers.DataDriven
{
    /// <summary>Concrete, serializable row embedded on <see cref="IAssetPublisherDefinitionHost"/>; polymorphism is on <see cref="source"/> and <see cref="bakedRegistrar"/>.</summary>
    [Serializable]
    public sealed class AssetPublisherDefinition
    {
        [SerializeField]
        [SerializeReference]
        private IAssetPublisherSource source;

        [SerializeField]
        [SerializeReference]
        [HideInInspector]
        private IPublisherRegistrar bakedRegistrar;

        public IAssetPublisherSource Source => source;

        public IPublisherRegistrar BakedRegistrar => bakedRegistrar;

        public void Register(IContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (source == null)
            {
                throw new InvalidOperationException("[AssetPublisherDefinition] Source is not set.");
            }

            if (bakedRegistrar == null)
            {
                bakedRegistrar = source.TryCreateRuntimeBakedRegistrar();
            }

            if (bakedRegistrar == null)
            {
                throw new InvalidOperationException(
                    $"[AssetPublisherDefinition] '{source.GetType().Name}' is not baked. Re-open the inspector or run Tools > Scaffold > AppFlow > Rebake All Publishers.");
            }

            bakedRegistrar.Register(builder);
        }

#if UNITY_EDITOR
        /// <summary>Re-runs <see cref="IAssetPublisherSource.Bake"/> and updates the embedded registrar.</summary>
        public void Rebake()
        {
            bakedRegistrar = source?.Bake();
        }

        /// <summary>Clears the baked registrar (e.g. when the source is invalid).</summary>
        public void ClearBake()
        {
            bakedRegistrar = null;
        }

        internal void SetSourceForTests(IAssetPublisherSource value)
        {
            source = value;
        }

        /// <summary>Editor: assign a source and re-bake (for migration / tooling).</summary>
        public void SetSourceAndRebake(IAssetPublisherSource newSource)
        {
            if (newSource == null)
            {
                throw new ArgumentNullException(nameof(newSource));
            }

            source = newSource;
            Rebake();
        }
#endif
    }
}
