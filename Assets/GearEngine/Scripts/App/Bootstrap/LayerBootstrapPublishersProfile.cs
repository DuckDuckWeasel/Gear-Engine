using System;
using System.Collections.Generic;
using Scaffold.AppFlow.Publishers.DataDriven;
using UnityEngine;

namespace GearEngine.App.Bootstrap
{
    /// <summary>
    /// Addressable layer publishers (tracks label + gear single-address catalog) for <see cref="FoundationLayer"/>,
    /// assignable on <see cref="GearAppFlowRoot"/> when you prefer a shared profile over inline <see cref="AssetPublisherDefinition"/> rows.
    /// </summary>
    [CreateAssetMenu(menuName = "GearEngine/Bootstrap/Layer Asset Publishers", fileName = "LayerPublishers_Campaign")]
    public sealed class LayerBootstrapPublishersProfile : ScriptableObject, IAssetPublisherDefinitionHost
    {
        [SerializeField]
        private List<AssetPublisherDefinition> definitions = new();

        public IReadOnlyList<AssetPublisherDefinition> AssetPublisherDefinitions => definitions;

#if UNITY_EDITOR
        public void ReplaceDefinitionsForEditor(IReadOnlyList<AssetPublisherDefinition> next)
        {
            if (next == null)
            {
                definitions = new List<AssetPublisherDefinition>();
            }
            else
            {
                definitions = new List<AssetPublisherDefinition>(next);
            }
        }
#endif
    }
}
