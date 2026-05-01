using System;
using GearEngine.Campaign.Authoring;
using GearEngine.Campaign.Bootstrap;
using GearEngine.Campaign.Bootstrap.Perks;
using GearEngine.Campaign.Bootstrap.LiveOps;
using GearEngine.Campaign.Services;
using GearEngine.CarSimulation.Bootstrap;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.PhysicsSimulation;
using GearEngine.Currency.Bootstrap;
using GearEngine.GearEngine.Bootstrap;
using GearEngine.GearEngine.Config;
using GearEngine.Perks.Config;
using Scaffold.AppFlow;
using VContainer;

namespace GearEngine.App.Bootstrap.Layers
{
    public sealed class CampaignLayer : IScopeLayer
    {
        public CampaignLayer(BoardRulesSO boardRules, GearEngineFeatureToggleSO featureToggle, RaceSessionDefaultsSO raceSessionDefaults, SimulationConfigBase simulationConfig, RoguelikeGearPoolSO roguelikeGearPool = null)
        {
            this.boardRules = boardRules ?? throw new ArgumentNullException(nameof(boardRules));
            this.raceSessionDefaults = raceSessionDefaults ?? throw new ArgumentNullException(nameof(raceSessionDefaults));
            this.simulationConfig = simulationConfig ?? throw new ArgumentNullException(nameof(simulationConfig));
            this.featureToggle = featureToggle ?? throw new ArgumentNullException(nameof(featureToggle));
            this.roguelikeGearPool = roguelikeGearPool ?? throw new ArgumentNullException(nameof(roguelikeGearPool));
        }

        private readonly BoardRulesSO boardRules;
        private readonly GearEngineFeatureToggleSO featureToggle;
        private readonly RaceSessionDefaultsSO raceSessionDefaults;
        private readonly SimulationConfigBase simulationConfig;
        private readonly RoguelikeGearPoolSO roguelikeGearPool;

        public void Install(IContainerBuilder builder)
        {
            RegisterGameplayConfigs(builder);
            RegisterLiveOpsClientModules(builder);
            RegisterGameplayServices(builder);
        }

        private void RegisterGameplayConfigs(IContainerBuilder builder)
        {
            builder.RegisterInstance(boardRules);
            builder.RegisterInstance(featureToggle);
            builder.RegisterInstance(raceSessionDefaults.Template);
        }

        private void RegisterLiveOpsClientModules(IContainerBuilder builder)
        {
            new CurrencyClientInstaller().Install(builder);
            new CampaignTracksInstaller().Install(builder);
            new CampaignInventoryInstaller().Install(builder);
            new CampaignLoadoutInstaller().Install(builder);
            new PerksClientInstaller().Install(builder);
            new CampaignRoguelikeInstaller().Install(builder);
        }

        private void RegisterGameplayServices(IContainerBuilder builder)
        {
            new GearMechanicsInstaller().Install(builder);
            new CarTrackInstaller().Install(builder, simulationConfig);
            new CampaignRaceSessionInstaller().Install(builder);

            builder.Register<CampaignGearPersistenceHookup>(Lifetime.Singleton)
                .As<IAsyncInitializable>()
                .As<IDisposable>();

            builder.Register<RoguelikeRollService>(Lifetime.Singleton)
                .As<IRoguelikeRollService>();
        }
    }
}
