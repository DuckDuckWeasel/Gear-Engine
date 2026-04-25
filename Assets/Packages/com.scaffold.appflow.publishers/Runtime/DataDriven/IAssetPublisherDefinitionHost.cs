using System.Collections.Generic;

namespace Scaffold.AppFlow.Publishers.DataDriven
{
    /// <summary>MonoBehaviours that own <see cref="AssetPublisherDefinition"/> rows for project-wide tools (rebake all).</summary>
    public interface IAssetPublisherDefinitionHost
    {
        IReadOnlyList<AssetPublisherDefinition> AssetPublisherDefinitions { get; }
    }
}
