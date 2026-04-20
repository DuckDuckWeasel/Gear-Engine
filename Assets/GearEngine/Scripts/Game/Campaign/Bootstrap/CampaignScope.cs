using System;
using System.Collections.Generic;
using System.Linq;
using GameModuleDTO.Modules.Inventory;
using GameModuleDTO.Modules.Loadout;
using GearEngine.Campaign.Bootstrap.LiveOps;
using GearEngine.Campaign.Services;
using GearEngine.CarSimulation.Bootstrap;
using GearEngine.CarSimulation.Definitions;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Bootstrap;
using GearEngine.GearEngine.Config;
using GearEngine.SceneFoundation.Bootstrap;
using Scaffold.LiveOps;
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
            ILiveOpsService liveOps = TryResolveParentLiveOps();
            if (liveOps == null)
            {
                throw new InvalidOperationException(
                    "[CampaignScope] No ILiveOpsService in parent LifetimeScope. Assign this scope's Parent to the Meta application root (or any scope that registered LiveOps).");
            }

            TrackCatalogSO trackCatalog = ScriptableObject.CreateInstance<TrackCatalogSO>();
            trackCatalog.SetRuntimeEntries(tracks, roguelikeCardPool);

            BoardLayoutData initialBoard = gearStart.BoardLoadout.BoardLayout;
            GearCatalogSO gearCatalog = BuildGearCatalog(gearStart, roguelikeCardPool, initialBoard);

            GearInventoryLoadoutData inventoryLoadout = ResolveInventorySeed(gearStart, gearCatalog, liveOps);
            GearBoardLoadoutData boardLoadout = ResolveBoardSeed(gearStart, gearCatalog, liveOps);

            new GearMechanicsInstaller(boardRules, featureToggle, inventoryLoadout, boardLoadout).Install(builder);
            builder.RegisterInstance(splineCarRunnerConfig);
            new CarTrackInstaller().Install(builder);

            RaceSessionConfig raceSessionTemplate = campaignRaceSession ?? new RaceSessionConfig();
            builder.RegisterInstance(new CampaignRaceSessionDefaults(raceSessionTemplate));

            builder.RegisterInstance(trackCatalog);
            builder.RegisterInstance(gearCatalog);
            builder.Register<TracksClientModule>(Lifetime.Singleton).As<ITrackService>().AsSelf();
            builder.Register<LoadoutClientModule>(Lifetime.Singleton).As<IGearLoadoutService>().AsSelf();
            builder.Register<InventoryClientModule>(Lifetime.Singleton).As<IOwnedGearInventoryService>().AsSelf();

            builder.RegisterInstance(gearStart);
            builder.RegisterComponent(sceneBootstrap).AsImplementedInterfaces().AsSelf();
        }

        private ILiveOpsService TryResolveParentLiveOps()
        {
            for (LifetimeScope p = Parent; p != null; p = p.Parent)
            {
                if (p.Container == null)
                {
                    continue;
                }

                try
                {
                    return p.Container.Resolve<ILiveOpsService>();
                }
                catch (VContainerException)
                {
                    // Parent chain continues
                }
            }

            return null;
        }

        private static GearCatalogSO BuildGearCatalog(GearEngineStartData gearStart, GearConfig[] roguelike, BoardLayoutData boardLayout)
        {
            var set = new HashSet<GearConfig>();
            if (roguelike != null)
            {
                foreach (GearConfig g in roguelike)
                {
                    if (g != null)
                    {
                        set.Add(g);
                    }
                }
            }

            if (gearStart?.GetInventoryLoadoutData()?.StartingItems != null)
            {
                foreach (GearConfig g in gearStart.GetInventoryLoadoutData().StartingItems)
                {
                    if (g != null)
                    {
                        set.Add(g);
                    }
                }
            }

            if (boardLayout?.Placements != null)
            {
                foreach (BoardGearPlacementData p in boardLayout.Placements)
                {
                    if (p?.GearConfig != null)
                    {
                        set.Add(p.GearConfig);
                    }
                }
            }

            GearCatalogSO catalog = ScriptableObject.CreateInstance<GearCatalogSO>();
            catalog.SetRuntimeEntries(set.Count > 0 ? set.ToArray() : Array.Empty<GearConfig>());
            return catalog;
        }

        private static GearInventoryLoadoutData ResolveInventorySeed(
            GearEngineStartData gearStart,
            GearCatalogSO gearCatalog,
            ILiveOpsService liveOps)
        {
            InventoryGameData cloud = liveOps.GetModuleData<InventoryGameData>();
            if (cloud != null && cloud.GearIds.Count > 0)
            {
                var configs = new List<GearConfig>();
                foreach (string id in cloud.GearIds)
                {
                    GearConfig g = gearCatalog.Get(id);
                    if (g != null)
                    {
                        configs.Add(g);
                    }
                }

                if (configs.Count > 0)
                {
                    return GearInventoryLoadoutData.FromGearConfigs(gearStart.InventoryLoadout.MaxSlots, configs);
                }
            }

            return gearStart.GetInventoryLoadoutData();
        }

        private static GearBoardLoadoutData ResolveBoardSeed(
            GearEngineStartData gearStart,
            GearCatalogSO gearCatalog,
            ILiveOpsService liveOps)
        {
            LoadoutGameData cloud = liveOps.GetModuleData<LoadoutGameData>();
            if (cloud != null && cloud.Board.Count > 0)
            {
                var items = new List<BoardGearPlacementData>();
                foreach (LoadoutPlacement p in cloud.Board)
                {
                    GearConfig g = gearCatalog.Get(p.GearId);
                    if (g != null)
                    {
                        items.Add(new BoardGearPlacementData(new Vector2Int(p.X, p.Y), g));
                    }
                }

                if (items.Count > 0)
                {
                    var boardData = new GearBoardLoadoutData();
                    boardData.BoardLayout = new BoardLayoutData(items);
                    return boardData;
                }
            }

            return gearStart.GetBoardLoadoutData();
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
