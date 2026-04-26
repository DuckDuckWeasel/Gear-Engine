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
    public abstract class GearAppFlowRoot : AppFlowRoot
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
        [Tooltip("Edit-time baked asset publishers (direct or Addressables). Campaign: track/gear (and related). Meta: same defaults as campaign when probing client data.")]
        [SerializeField]
        private List<AssetPublisherDefinition> layerAssetPublishers = new List<AssetPublisherDefinition>();

        protected sealed override IInLayerScheduler CreateScheduler()
        {
            return new SequentialInLayerScheduler();
        }

        protected sealed override IEnumerable<IScopeLayer> GetInitialLayers()
        {
            yield return new FoundationLayer(navigationSettings, navigationViewHolder, layerAssetPublishers, defaultRaceCar);
            foreach (IScopeLayer layer in GetGameLayers())
            {
                yield return layer;
            }
        }

        protected abstract IEnumerable<IScopeLayer> GetGameLayers();
    }
}
