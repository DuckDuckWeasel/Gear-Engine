using System;
using System.Collections.Generic;
using GearEngine.App.Bootstrap.Layers;
using GearEngine.CarSimulation.Definitions;
using Scaffold.AppFlow;
using Scaffold.AppFlow.Publishers.DataDriven;
using Scaffold.Navigation;
using UnityEngine;

namespace GearEngine.App.Bootstrap
{
    public abstract class GearAppFlowRoot : AppFlowRoot, IAssetPublisherDefinitionHost
    {
        [Header("Navigation")]
        [SerializeField]
        private NavigationSettings navigationSettings;

        [SerializeField]
        private Transform navigationViewHolder;

        [Header("Racing defaults")]
        [Tooltip("Default car for track services and LiveOps modules (same as former TrackCatalogSO.defaultCar).")]
        [SerializeField]
        private CarDefinition defaultRaceCar;

        [Header("Layer asset publishers")]
        [Tooltip("Optional: shared profile (tracks + gear catalogs). When set, inline rows below are ignored for registration and for Rebake All.")]
        [SerializeField]
        private LayerBootstrapPublishersProfile layerPublishersProfile;

        [Tooltip("Edit-time baked asset publishers (direct or Addressables). Used when no profile is assigned. Campaign: assign track/gear. Meta: same as campaign when probing client data.")]
        [SerializeField]
        private List<AssetPublisherDefinition> layerAssetPublishers = new List<AssetPublisherDefinition>();

        IReadOnlyList<AssetPublisherDefinition> IAssetPublisherDefinitionHost.AssetPublisherDefinitions =>
            GetEffectiveLayerAssetPublishers();

        private IReadOnlyList<AssetPublisherDefinition> GetEffectiveLayerAssetPublishers()
        {
            if (layerPublishersProfile != null)
            {
                IReadOnlyList<AssetPublisherDefinition> from = layerPublishersProfile.AssetPublisherDefinitions;
                if (from != null && from.Count > 0)
                {
                    return from;
                }
            }

            return layerAssetPublishers;
        }

        protected sealed override IInLayerScheduler CreateScheduler()
        {
            return new SequentialInLayerScheduler();
        }

        protected sealed override IEnumerable<IScopeLayer> GetInitialLayers()
        {
            yield return new FoundationLayer(navigationSettings, navigationViewHolder, GetEffectiveLayerAssetPublishers(), defaultRaceCar);
            foreach (IScopeLayer layer in GetGameLayers())
            {
                yield return layer;
            }
        }

        protected abstract IEnumerable<IScopeLayer> GetGameLayers();
    }
}
