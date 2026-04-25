namespace Scaffold.AppFlow.Publishers.DataDriven
{
    /// <summary>Polymorphic authoring payload for a single <see cref="AssetPublisherDefinition"/> row. Implementations are discovered by the default inspector (TypeCache).</summary>
    public interface IAssetPublisherSource
    {
        /// <summary>When a scene asset has no embedded bake (e.g. never saved after re-seed), create the same registrar the editor would embed — label/single only.</summary>
        IPublisherRegistrar TryCreateRuntimeBakedRegistrar() => null;

#if UNITY_EDITOR
        /// <summary>Whether the source is ready for a successful bake in the editor.</summary>
        bool IsConfigured { get; }

        /// <summary>Edit-time only: produce the closed-generic registrar to embed in the wrapper.</summary>
        IPublisherRegistrar Bake();
#endif
    }
}
