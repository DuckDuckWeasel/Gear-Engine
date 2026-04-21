using System;
using GearEngine.Campaign.Bootstrap;
using GearEngine.Campaign.Services;
using GearEngine.CarSimulation.Bootstrap;
using GearEngine.CarSimulation.Definitions;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Bootstrap;
using GearEngine.GearEngine.Config;
using Scaffold.LayeredScope;
using VContainer;

namespace GearEngine.App.Bootstrap.Layers
{
    /// <summary>
    /// Campaign gameplay layer: gear mechanics, car/track simulation, and race session defaults.
    /// LiveOps game clients install in <see cref="LiveOpsClientModulesLayer"/> before this layer is pushed.
    /// </summary>
    public sealed class CampaignLayer : IScopeLayer
    {
        public CampaignLayer(
            BoardRulesSO boardRules,
            GearEngineFeatureToggleSO featureToggle,
            GearEngineStartDataSO gearStartData,
            SplineCarRunnerConfigSO splineCarRunnerConfig,
            RaceSessionDefaultsSO raceSessionDefaults)
        {
            this.boardRules = boardRules ?? throw new ArgumentNullException(nameof(boardRules));
            this.featureToggle = featureToggle;
            this.gearStartData = gearStartData ?? throw new ArgumentNullException(nameof(gearStartData));
            this.splineCarRunnerConfig = splineCarRunnerConfig ?? throw new ArgumentNullException(nameof(splineCarRunnerConfig));
            this.raceSessionDefaults = raceSessionDefaults ?? throw new ArgumentNullException(nameof(raceSessionDefaults));
        }

        private readonly BoardRulesSO boardRules;
        private readonly GearEngineFeatureToggleSO featureToggle;
        private readonly GearEngineStartDataSO gearStartData;
        private readonly SplineCarRunnerConfigSO splineCarRunnerConfig;
        private readonly RaceSessionDefaultsSO raceSessionDefaults;

        public void Install(IContainerBuilder builder)
        {
            GearEngineStartData start = gearStartData.Data ?? throw new InvalidOperationException("[CampaignLayer] gearStartData.Data is null.");
            new GearMechanicsInstaller(
                    boardRules,
                    featureToggle,
                    start.GetInventoryLoadoutData(),
                    start.GetBoardLoadoutData())
                .Install(builder);

            builder.RegisterInstance(splineCarRunnerConfig);
            new CarTrackInstaller().Install(builder);
            new CampaignRaceSessionInstaller(raceSessionDefaults).Install(builder);
            builder.RegisterInstance(start);

            builder.Register<CampaignGearPersistenceHookup>(Lifetime.Singleton)
                .As<IAsyncInitializable>()
                .As<IDisposable>();

            builder.Register<RoguelikeRollService>(Lifetime.Singleton)
                .As<IRoguelikeRollService>();
        }
    }
}
