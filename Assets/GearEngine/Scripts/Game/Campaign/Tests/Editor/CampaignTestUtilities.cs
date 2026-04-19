using System;
using System.Collections.Generic;
using System.Reflection;
using GearEngine.Campaign;
using GearEngine.Campaign.Services;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Services.Board;
using GearEngine.GearEngine.Bootstrap;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Merge;
using GearEngine.GearEngine.Manager;
using GearEngine.GearEngine.Services;
using GearEngine.GearEngine.Services.Inventory;
using Scaffold.Events.Contracts;
using Scaffold.Events.Container;
using UnityEngine;
using VContainer;
using Object = UnityEngine.Object;

namespace GearEngine.Campaign.Tests.Editor
{
    internal sealed class GearMechanicsTestContext : System.IDisposable
    {
        public GearMechanicsTestContext(BoardRulesSO boardRules)
        {
            var builder = new ContainerBuilder();
            new EventsInstaller().Install(builder);
            new GearMechanicsInstaller(boardRules, null, GearInventoryLoadoutData.Empty(), new GearBoardLoadoutData()).Install(builder);
            container = builder.Build();
            Engine = container.Resolve<IGearEngineService>();
            GridManager = container.Resolve<IGridManager>();
            NodeFactory = container.Resolve<IGearNodeFactory>();
            BoardRules = boardRules;
            EventBus = container.Resolve<IEventBus>();
            FeatureToggle = container.Resolve<GearEngineFeatureToggleSO>();
            DragService = container.Resolve<IDragService>();
            SwapService = container.Resolve<IGridSwapService>();
            MergeService = container.Resolve<IGridMergeService>();
            InventoryService = container.Resolve<IInventoryService>();
            PresentationTransfer = container.Resolve<IGearPresentationTransferService>();
            BoardService = container.Resolve<IBoardService>();
        }

        public IGearEngineService Engine { get; }
        public IGridManager GridManager { get; }
        public IGearNodeFactory NodeFactory { get; }
        public BoardRulesSO BoardRules { get; }
        public IEventBus EventBus { get; }
        public GearEngineFeatureToggleSO FeatureToggle { get; }
        public IDragService DragService { get; }
        public IGridSwapService SwapService { get; }
        public IGridMergeService MergeService { get; }
        public IInventoryService InventoryService { get; }
        public IGearPresentationTransferService PresentationTransfer { get; }
        public IBoardService BoardService { get; }

        private readonly IObjectResolver container;

        public void Dispose()
        {
            container?.Dispose();
        }
    }

    internal static class CampaignTestUtilities
    {
        public static GearConfig CreateGearConfigWithData(string id)
        {
            var config = ScriptableObject.CreateInstance<GearConfig>();
            var data = new GearConfigData { Id = id };
            FieldInfo field = typeof(GearConfig).GetField("data", BindingFlags.Instance | BindingFlags.NonPublic);
            AssertFieldFound(field);
            field.SetValue(config, data);
            return config;
        }

        public static RaceState CreateMinimalSession(CarDefinition carDef, TrackDefinition trackDef)
        {
            var factory = new TrackSimulationFactory();
            return factory.Create(carDef, trackDef, null);
        }

        public static TrackDefinition CreateTrackWithScoreBandsForTests(params TrackScoreBand[] bands)
        {
            var track = ScriptableObject.CreateInstance<TrackDefinition>();
            track.SetScoreBandsForTests(bands);
            return track;
        }

        public static void DestroyGearConfig(GearConfig config)
        {
            if (config != null)
            {
                Object.DestroyImmediate(config);
            }
        }

        private static void AssertFieldFound(FieldInfo field)
        {
            if (field == null)
            {
                throw new InvalidOperationException("GearConfig 'data' field not found.");
            }
        }
    }

    internal sealed class FakeTrackService : ITrackService
    {
        public FakeTrackService(
            TrackDefinition track,
            CarDefinition car,
            IReadOnlyList<GearConfig> roguelikePool = null)
        {
            CurrentTrack = track;
            CurrentCar = car;
            roguelikeOptions = roguelikePool ?? Array.Empty<GearConfig>();
        }

        public TrackDefinition CurrentTrack { get; }
        public CarDefinition CurrentCar { get; }

        private readonly IReadOnlyList<GearConfig> roguelikeOptions;
        private readonly TrackProgressModel trackProgress = new TrackProgressModel();
        public int AdvanceCallCount { get; private set; }
        public int RecordResultCallCount { get; private set; }

        public TrackProgressModel GetTrackProgress() => trackProgress;

        public IReadOnlyList<TrackEntry> GetOrderedTracks() => Array.Empty<TrackEntry>();

        public IReadOnlyList<GearConfig> GetRoguelikeCardOptions() => roguelikeOptions;

        public void RecordResult(RaceResultModel result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            RecordResultCallCount++;
        }

        public void AdvanceToNextTrack()
        {
            AdvanceCallCount++;
        }
    }

    internal sealed class FakeWalletService : IWalletService
    {
        private readonly WalletModel wallet = new WalletModel();

        public WalletModel GetWallet() => wallet;

        public void AddGold(int amount)
        {
            wallet.Gold += amount;
        }

        public bool TrySpendGold(int amount)
        {
            if (amount < 0 || amount > wallet.Gold)
            {
                return false;
            }

            wallet.Gold -= amount;
            return true;
        }
    }

    internal sealed class RecordingInventoryService : IInventoryService
    {
        public readonly List<IItem> AddedItems = new List<IItem>();
        private readonly InventoryModel model = new InventoryModel();

        public RecordingInventoryService()
        {
            model.MaxSlots = 32;
        }

        public InventoryModel GetInventory() => model;

        public bool TryAdd(IItem item)
        {
            if (item == null)
            {
                return false;
            }

            AddedItems.Add(item);
            model.Items.Add(item);
            return true;
        }

        public bool TryConsume(IItem item)
        {
            if (item == null)
            {
                return false;
            }

            for (int i = 0; i < model.Items.Count; i++)
            {
                if (ReferenceEquals(model.Items[i], item))
                {
                    model.Items.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }
    }
}
