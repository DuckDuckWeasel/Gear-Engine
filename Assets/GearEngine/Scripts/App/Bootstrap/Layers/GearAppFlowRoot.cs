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

        [Header("Timing")]
        [Tooltip("Minimum time (in seconds) the application bootstrap should take before declaring readiness.")]
        [SerializeField]
        private float minimumLoadingTimeSeconds = 2f;

        [Header("Global UI")]
        [SerializeField]
        private global::GearEngine.SceneFoundation.Presentation.GlobalLoadingOverlay globalLoadingPrefab;

        [Header("Racing defaults")]
        [Tooltip("Default car for track services and LiveOps modules (same as former TrackCatalogSO.defaultCar).")]
        [SerializeField]
        private CarDefinition defaultRaceCar;

        [Header("Layer asset publishers")]
        [Tooltip("Edit-time baked asset publishers (direct or Addressables). Campaign: track/gear (and related). Meta: same defaults as campaign when probing client data.")]
        [SerializeField]
        private List<AssetPublisherDefinition> layerAssetPublishers = new List<AssetPublisherDefinition>();

        private float _startupTime;

        protected override void Awake()
        {
            base.Awake();
            _startupTime = Time.realtimeSinceStartup;
        }

        protected sealed override IInLayerScheduler CreateScheduler()
        {
            return new SequentialInLayerScheduler();
        }

        protected sealed override IEnumerable<IScopeLayer> GetInitialLayers()
        {
            yield return new FoundationLayer(navigationSettings, navigationViewHolder, globalLoadingPrefab, layerAssetPublishers, defaultRaceCar);
            foreach (IScopeLayer layer in GetGameLayers())
            {
                yield return layer;
            }
            yield return new MinimumDelayLayer(_startupTime, minimumLoadingTimeSeconds);
        }

        protected abstract IEnumerable<IScopeLayer> GetGameLayers();
    }
}
