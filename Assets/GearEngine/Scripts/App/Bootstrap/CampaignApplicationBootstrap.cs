using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GearEngine.App.Bootstrap.Layers;
using GearEngine.Campaign.Presentation;
using GearEngine.Campaign.Services;
using GearEngine.CarSimulation.Definitions;
using GearEngine.GearEngine.Config;
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

        [Header("Simulation")]
        [SerializeField]
        private SplineCarRunnerConfigSO splineCarRunnerConfig;

        [Header("Race session defaults")]
        [SerializeField]
        private RaceSessionDefaultsSO raceSessionDefaults;

        protected override IEnumerable<IScopeLayer> GetGameLayers()
        {
            yield return new UgsLayer();
            yield return new LiveOpsLayer();
            yield return new CampaignLayer(
                boardRules,
                featureToggle,
                raceSessionDefaults,
                splineCarRunnerConfig);
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
