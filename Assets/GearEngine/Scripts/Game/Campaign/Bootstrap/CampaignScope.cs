using System;
using GearEngine.Campaign.Services;
using GearEngine.CarSimulation.Bootstrap;
using GearEngine.CarSimulation.Definitions;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Bootstrap;
using GearEngine.GearEngine.Config;
using GearEngine.SceneFoundation.Bootstrap;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

namespace GearEngine.Campaign.Bootstrap
{
    public sealed class CampaignScope : SceneFoundationScope
    {
        [Header("Tracks (ordered campaign list)")]
        [SerializeField] private TrackEntry[] tracks;

        [Header("Gear Engine")]
        [FormerlySerializedAs("boardConfig")]
        [SerializeField] private BoardRulesSO boardRules;
        [SerializeField] private GearEngineFeatureToggleSO featureToggle;

        [Header("Simulation")]
        [SerializeField] private SplineCarRunnerConfigSO splineCarRunnerConfig;

        [Header("Active race session (defaults for ActiveRaceViewModel)")]
        [SerializeField] private RaceSessionConfig campaignRaceSession = new RaceSessionConfig();

        [Header("Campaign gear loadout (setup / roguelike inventory)")]
        [SerializeField] private GearEngineStartData campaignGearStartData;

        [Header("Roguelike card pool")]
        [SerializeField] private GearConfig[] roguelikeCardPool;

        [Header("Bootstrap")]
        [SerializeField] private CampaignBootstrap sceneBootstrap;

        protected override void ValidateSceneAssignments()
        {
            if (tracks == null || tracks.Length == 0)
            {
                throw new InvalidOperationException("[CampaignScope] Assign at least one TrackEntry.");
            }

            if (boardRules == null)
            {
                throw new InvalidOperationException("[CampaignScope] Assign boardRules.");
            }

            if (splineCarRunnerConfig == null)
            {
                throw new InvalidOperationException("[CampaignScope] Assign SplineCarRunnerConfigSO.");
            }

            if (sceneBootstrap == null)
            {
                throw new InvalidOperationException("[CampaignScope] Assign sceneBootstrap.");
            }

            if (campaignGearStartData == null)
            {
                throw new InvalidOperationException("[CampaignScope] Assign campaignGearStartData.");
            }
        }

        protected override void InstallFeatureServices(IContainerBuilder builder)
        {
            GearEngineStartData gearStart = campaignGearStartData;

            LocalGearLoadoutService loadoutService = new LocalGearLoadoutService();
            if (gearStart.BoardLoadout.BoardLayout != null)
            {
                loadoutService.SaveBoardLayout(gearStart.BoardLoadout.BoardLayout);
            }

            GearInventoryLoadoutData inventoryLoadout = loadoutService.HasSavedInventory
                ? GearInventoryLoadoutData.FromGearConfigs(gearStart.InventoryLoadout.MaxSlots, loadoutService.GetInventoryGearConfigs())
                : gearStart.GetInventoryLoadoutData();

            new GearMechanicsInstaller(boardRules, featureToggle, inventoryLoadout, gearStart.GetBoardLoadoutData()).Install(builder);
            builder.RegisterInstance(splineCarRunnerConfig);
            new CarTrackInstaller().Install(builder);

            RaceSessionConfig raceSessionTemplate = campaignRaceSession ?? new RaceSessionConfig();
            builder.RegisterInstance(new CampaignRaceSessionDefaults(raceSessionTemplate));

            LocalTrackService trackService = new LocalTrackService(tracks, roguelikeCardPool);
            builder.RegisterInstance<ITrackService>(trackService);

            builder.RegisterInstance(gearStart);

            LocalWalletService walletService = new LocalWalletService(initialGold: 0);
            builder.RegisterInstance<IWalletService>(walletService);

            builder.RegisterInstance<IGearLoadoutService>(loadoutService);
            builder.RegisterComponent(sceneBootstrap).AsImplementedInterfaces().AsSelf();
        }
    }

    /// <summary>Builds per-race <see cref="RaceSessionConfig"/> from serialized campaign defaults and the current track.</summary>
    public sealed class CampaignRaceSessionDefaults
    {
        private readonly RaceSessionConfig template;

        public CampaignRaceSessionDefaults(RaceSessionConfig template)
        {
            this.template = template ?? new RaceSessionConfig();
        }

        public RaceSessionConfig CreateForTrack(TrackDefinition track)
        {
            if (track == null)
            {
                throw new ArgumentNullException(nameof(track));
            }

            RaceSessionConfig config = template.CloneForNewRace();
            config.ApplyFromTrack(track);
            return config;
        }
    }
}
