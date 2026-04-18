using System;
using GearEngine.Campaign.Services;
using GearEngine.CarSimulation.Bootstrap;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Bootstrap;
using GearEngine.GearEngine.Config;
using GearEngine.SceneFoundation.Bootstrap;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GearEngine.Campaign.Bootstrap
{
    public sealed class CampaignScope : SceneFoundationScope
    {
        [Header("Tracks (ordered campaign list)")]
        [SerializeField] private TrackEntry[] tracks;

        [Header("Gear Engine")]
        [SerializeField] private BoardConfigSO boardConfig;
        [SerializeField] private GearEngineFeatureToggleSO featureToggle;

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

            if (boardConfig == null)
            {
                throw new InvalidOperationException("[CampaignScope] Assign boardConfig.");
            }

            if (sceneBootstrap == null)
            {
                throw new InvalidOperationException("[CampaignScope] Assign sceneBootstrap.");
            }
        }

        protected override void InstallFeatureServices(IContainerBuilder builder)
        {
            GearEngineStartData gearStart = campaignGearStartData ?? new GearEngineStartData();

            LocalGearLoadoutService loadoutService = new LocalGearLoadoutService();
            if (gearStart.BoardLoadout.BoardLayout != null)
            {
                loadoutService.SaveBoardLayout(gearStart.BoardLoadout.BoardLayout);
            }

            GearInventoryLoadoutData inventoryLoadout = loadoutService.HasSavedInventory
                ? GearInventoryLoadoutData.FromGearConfigs(gearStart.InventoryLoadout.MaxSlots, loadoutService.GetInventoryGearConfigs())
                : gearStart.GetInventoryLoadoutData();

            new GearMechanicsInstaller(boardConfig, featureToggle).Install(builder, inventoryLoadout, gearStart.GetBoardLoadoutData());
            new CarTrackInstaller().Install(builder);

            LocalTrackService trackService = new LocalTrackService(tracks, roguelikeCardPool);
            builder.RegisterInstance<ITrackService>(trackService);

            builder.RegisterInstance(gearStart);

            LocalWalletService walletService = new LocalWalletService(initialGold: 0);
            builder.RegisterInstance<IWalletService>(walletService);

            builder.RegisterInstance<IGearLoadoutService>(loadoutService);
            builder.RegisterComponent(sceneBootstrap).AsImplementedInterfaces().AsSelf();
        }
    }
}
