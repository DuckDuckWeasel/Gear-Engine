using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GearEngine.App.Bootstrap.Layers;
using GearEngine.App.Bootstrap.Offline;
using GearEngine.Campaign.Presentation;
using GearEngine.Campaign.Services;
using GearEngine.CarSimulation.Definitions;
using GearEngine.GearEngine.Config;
using Scaffold.Ads;
using Scaffold.Ads.Levelplay;
using Scaffold.AppFlow;
using Scaffold.Navigation.Contracts;
using UnityEngine;

namespace GearEngine.App.Bootstrap
{
    public sealed class CampaignApplicationBootstrap : GearAppFlowRoot
    {
        [Header("Gear Engine")]
        [SerializeField]
        private BoardRulesSO boardRules;

        [SerializeField]
        private GearEngineFeatureToggleSO featureToggle;

        [SerializeField]
        private AdPlacementKeySO rerollPlacementKey;

        [Header("Simulation")]
        [SerializeField]
        private SimulationConfigBase simulationConfig;

        [Header("Race session defaults")]
        [SerializeField]
        private RaceSessionDefaultsSO raceSessionDefaults;
        
        [SerializeField]
        private LevelPlayAdConfigurationSO adConfig;

        protected override IEnumerable<IScopeLayer> GetGameLayers()
        {
            if (OfflineMode)
            {
                yield return new OfflineLiveOpsLayer(OfflineConfigBuilders);
            }
            else
            {
                yield return new UgsLayer();
                yield return new LiveOpsLayer();
            }

            yield return new AdsLayer(adConfig);
            yield return new CampaignLayer(
                boardRules,
                featureToggle,
                raceSessionDefaults,
                simulationConfig,
                rerollPlacementKey);
        }

        protected override Task OnReadyAsync(CancellationToken ct)
        {
            try
            {
                INavigation navigation = Host.Resolve<INavigation>();
                navigation.Open(new MainViewModel());
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Campaign] OnReadyAsync failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }

            _ = ct;
            return Task.CompletedTask;
        }

        protected override Task OnStartupFailedAsync(Exception ex, CancellationToken ct)
        {
            Debug.LogError($"[Campaign] Startup failed: {ex.Message}\n{ex.StackTrace}");
            _ = ct;
            return Task.CompletedTask;
        }
    }
}
