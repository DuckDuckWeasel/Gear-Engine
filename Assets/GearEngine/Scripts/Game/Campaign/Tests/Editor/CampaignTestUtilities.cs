using System;
using System.Collections.Generic;
using System.Reflection;
using GameModuleDTO.ModuleRequests;
using GameModuleDTO.Modules.Currency;
using GearEngine.Campaign;
using GearEngine.Campaign.Services;
using GearEngine.Currency;
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
            builder.RegisterInstance<IInventoryService>(new RecordingInventoryService());
            new GearMechanicsInstaller(boardRules, null, new GearBoardLoadoutData()).Install(builder);
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
        private readonly CurrencyClientModule currencyClient;

        public FakeTrackService(
            TrackDefinition track,
            CarDefinition car,
            CurrencyClientModule currencyClient = null)
        {
            CurrentTrack = track;
            CurrentCar = car;
            this.currencyClient = currencyClient;
        }

        public TrackDefinition CurrentTrack { get; }
        public CarDefinition CurrentCar { get; }

        private readonly TrackProgressModel trackProgress = new TrackProgressModel();
        public int RecordResultCallCount { get; private set; }

        public TrackProgressModel GetTrackProgress() => trackProgress;

        public IReadOnlyList<TrackEntry> GetOrderedTracks() => Array.Empty<TrackEntry>();

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

    internal sealed class RecordingInventoryService : IInventoryService
    {
        public event Action InventoryChanged;

        public readonly List<GearConfig> AddedGearConfigs = new List<GearConfig>();

        private readonly List<OwnedGear> owned = new List<OwnedGear>();

        public bool HasSavedInventory => owned.Count > 0;

        public IReadOnlyList<OwnedGear> Owned => owned;

        public OwnedGear Add(GearConfig gear)
        {
            if (gear == null)
            {
                return null;
            }

            AddedGearConfigs.Add(gear);
            var o = new OwnedGear { InstanceId = Guid.NewGuid().ToString("N"), Config = gear };
            owned.Add(o);
            InventoryChanged?.Invoke();
            return o;
        }

        public bool Remove(OwnedGear gear)
        {
            if (gear == null)
            {
                return false;
            }

            if (!owned.Remove(gear))
            {
                return false;
            }

            InventoryChanged?.Invoke();
            return true;
        }

        public void Clear()
        {
            owned.Clear();
            InventoryChanged?.Invoke();
        }
    }
}
