using System;
using System.Collections.Generic;
using System.Reflection;
using GameModuleDTO.ModuleRequests;
using GameModuleDTO.Modules.Currency;
using GearEngine.Campaign;
using GearEngine.Currency;
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
            InventoryService = container.Resolve<IRaceInventoryService>();
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
        public IRaceInventoryService InventoryService { get; }
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
        private readonly CurrencyClientModule currencyClient;

        public FakeTrackService(
            TrackDefinition track,
            CarDefinition car,
            IReadOnlyList<GearConfig> roguelikePool = null,
            CurrencyClientModule currencyClient = null)
        {
            CurrentTrack = track;
            CurrentCar = car;
            roguelikeOptions = roguelikePool ?? Array.Empty<GearConfig>();
            this.currencyClient = currencyClient;
        }

        public TrackDefinition CurrentTrack { get; }
        public CarDefinition CurrentCar { get; }

        private readonly IReadOnlyList<GearConfig> roguelikeOptions;
        private readonly TrackProgressModel trackProgress = new TrackProgressModel();
        public int RecordResultCallCount { get; private set; }

        public TrackProgressModel GetTrackProgress() => trackProgress;

        public IReadOnlyList<TrackEntry> GetOrderedTracks() => Array.Empty<TrackEntry>();

        public IReadOnlyList<GearConfig> GetRoguelikeCardOptions() => roguelikeOptions;

        public System.Threading.Tasks.Task RecordResultAsync(RaceResultModel result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            RecordResultCallCount++;
            int reward = result.Gold.Amount;
            result.ServerOutcome = new RecordRaceResultResponse
            {
                Reward = reward,
                NewBestTimeSec = result.RaceTime,
                MatchedBandIndex = reward > 0 ? 0 : -1,
                Advanced = false,
            };

            if (currencyClient != null && reward > 0)
            {
                long cur = currencyClient.GetWallet("gold")?.Current ?? 0;
                currencyClient.ApplyNestedAddCurrency(new AddCurrencyResponse("gold", cur + reward, reward));
            }

            return System.Threading.Tasks.Task.CompletedTask;
        }
    }

    internal sealed class RecordingInventoryService : IRaceInventoryService
    {
        public event Action ItemsChanged;

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
            ItemsChanged?.Invoke();
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
                    ItemsChanged?.Invoke();
                    return true;
                }
            }

            return false;
        }
    }
}
