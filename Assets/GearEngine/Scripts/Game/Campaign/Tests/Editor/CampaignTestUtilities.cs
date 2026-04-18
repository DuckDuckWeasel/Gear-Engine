using System;
using System.Collections.Generic;
using System.Reflection;
using GearEngine.Campaign;
using GearEngine.Campaign.Services;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.GearEngine;
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
        public GearMechanicsTestContext(BoardConfigSO boardConfig)
        {
            var builder = new ContainerBuilder();
            new EventsInstaller().Install(builder);
            new GearMechanicsInstaller(boardConfig, null).Install(builder);
            container = builder.Build();
            Engine = container.Resolve<IGearEngineService>();
            GridManager = container.Resolve<IGridManager>();
            NodeFactory = container.Resolve<IGearNodeFactory>();
            BoardConfig = boardConfig;
            EventBus = container.Resolve<IEventBus>();
            FeatureToggle = container.Resolve<GearEngineFeatureToggleSO>();
            DragService = container.Resolve<IDragService>();
            SwapService = container.Resolve<IGridSwapService>();
            MergeService = container.Resolve<IGridMergeService>();
            InventoryService = container.Resolve<IInventoryService>();
            PresentationTransfer = container.Resolve<IGearPresentationTransferService>();
        }

        public IGearEngineService Engine { get; }
        public IGridManager GridManager { get; }
        public IGearNodeFactory NodeFactory { get; }
        public BoardConfigSO BoardConfig { get; }
        public IEventBus EventBus { get; }
        public GearEngineFeatureToggleSO FeatureToggle { get; }
        public IDragService DragService { get; }
        public IGridSwapService SwapService { get; }
        public IGridMergeService MergeService { get; }
        public IInventoryService InventoryService { get; }
        public IGearPresentationTransferService PresentationTransfer { get; }

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

        public static LapRaceSession CreateMinimalSession(CarDefinition carDef, TrackDefinition trackDef)
        {
            var factory = new TrackSimulationFactory();
            return factory.Create(carDef, trackDef);
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
            LapRaceSession session,
            IReadOnlyList<GearConfig> roguelikePool = null)
        {
            CurrentTrack = track;
            CurrentCar = car;
            CurrentSession = session;
            roguelikeOptions = roguelikePool ?? Array.Empty<GearConfig>();
        }

        public TrackDefinition CurrentTrack { get; }
        public CarDefinition CurrentCar { get; }
        public LapRaceSession CurrentSession { get; private set; }

        private readonly IReadOnlyList<GearConfig> roguelikeOptions;
        public int AdvanceCallCount { get; private set; }
        public int RecordResultCallCount { get; private set; }

        public IReadOnlyList<GearConfig> GetRoguelikeCardOptions() => roguelikeOptions;

        public void SetCurrentSession(LapRaceSession session)
        {
            CurrentSession = session ?? throw new ArgumentNullException(nameof(session));
        }

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
        public int CurrentGold { get; private set; }

        public void AddGold(int amount)
        {
            CurrentGold += amount;
        }

        public void SpendGold(int amount)
        {
            CurrentGold -= amount;
        }
    }

    internal sealed class RecordingInventoryService : IInventoryService
    {
        public readonly List<IItem> AddedItems = new List<IItem>();
        public InventoryModel Model { get; } = new InventoryModel();
        public int CurrentCount => Model.AvailableItems.Count;
        public int MaxSlots { get; private set; } = 32;

        public void Initialize(int maxSlots, IReadOnlyList<GearConfig> inventoryGears)
        {
            MaxSlots = maxSlots;
        }

        public void LoadInventory(IEnumerable<IItem> items)
        {
        }

        public void AddItem(IItem item)
        {
            if (item != null)
            {
                AddedItems.Add(item);
            }
        }

        public void ConsumeSpecificItem(IItem item)
        {
        }
    }
}
