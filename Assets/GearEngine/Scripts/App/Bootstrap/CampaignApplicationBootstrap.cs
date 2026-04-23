using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GearEngine.App.Bootstrap.Layers;
using GearEngine.Campaign.Bootstrap.Cards;
using GearEngine.Campaign.Bootstrap.LiveOps;
using GearEngine.Campaign.Presentation;
using GearEngine.Campaign.Services;
using GearEngine.CarSimulation.Definitions;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Config;
using Scaffold.AppFlow;
using Scaffold.Navigation;
using Scaffold.Navigation.Contracts;
using UnityEngine;
using VContainer;

namespace GearEngine.App.Bootstrap
{
    public sealed class CampaignApplicationBootstrap : AppFlowRoot
    {
        [Header("Navigation")]
        [SerializeField]
        private NavigationSettings navigationSettings;

        [SerializeField]
        private Transform navigationViewHolder;

        [Header("Catalogs")]
        [SerializeField]
        private TrackCatalogSO trackCatalog;

        [SerializeField]
        private RoguelikeGearPoolSO roguelikeGearPool;

        [SerializeField]
        private GearCatalogSO gearCatalog;

        [Header("Gear Engine")]
        [SerializeField]
        private BoardRulesSO boardRules;

        [SerializeField]
        private GearEngineFeatureToggleSO featureToggle;

        [SerializeField]
        private GearEngineStartDataSO gearStartData;

        [Header("Simulation")]
        [SerializeField]
        private SplineCarRunnerConfigSO splineCarRunnerConfig;

        [Header("Race session defaults")]
        [SerializeField]
        private RaceSessionDefaultsSO raceSessionDefaults;

        protected override void ConfigureApplication(IContainerBuilder builder)
        {
        }

        protected override IEnumerable<IScopeLayer> GetInitialLayers()
        {
            Require(navigationSettings, nameof(navigationSettings));
            Require(navigationViewHolder, nameof(navigationViewHolder));
            Require(trackCatalog, nameof(trackCatalog));
            Require(roguelikeGearPool, nameof(roguelikeGearPool));
            Require(gearCatalog, nameof(gearCatalog));
            Require(boardRules, nameof(boardRules));
            Require(gearStartData, nameof(gearStartData));
            Require(splineCarRunnerConfig, nameof(splineCarRunnerConfig));
            Require(raceSessionDefaults, nameof(raceSessionDefaults));

            yield return new FoundationLayer(navigationSettings, navigationViewHolder);
            yield return new UgsLayer();
            yield return new LiveOpsServiceLayer();
            yield return new LiveOpsClientModulesLayer(
                new CampaignTracksInstaller(trackCatalog),
                new CampaignGearCatalogInstaller(gearCatalog),
                new CampaignInventoryInstaller(),
                new CampaignLoadoutInstaller(),
                new CardsClientInstaller(),
                new CampaignRoguelikeInstaller(roguelikeGearPool));
            yield return new CampaignLayer(
                boardRules,
                featureToggle,
                gearStartData,
                splineCarRunnerConfig,
                raceSessionDefaults);
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

        private static void Require(UnityEngine.Object value, string name)
        {
            if (value == null)
            {
                throw new InvalidOperationException($"[{nameof(CampaignApplicationBootstrap)}] Assign {name}.");
            }
        }
    }
}
