using System;
using System.Collections;
using System.Collections.Generic;
using GearEngine.CarSimulation.PhysicsSimulation;
using System.Threading;
using System.Threading.Tasks;
using LiveOps.DTO.GameModule;
using LiveOps.DTO.ModuleRequest;
using LiveOps.Modules.DTO.Currency;
using LiveOps.Modules.DTO.ModuleRequests;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Services;
using GearEngine.GearEngine.Services.Board;
using GearEngine.Campaign.Bootstrap;
using GearEngine.Campaign.Presentation;
using GearEngine.Campaign.Services;
using GearEngine.Currency;
using GearEngine.GearEngine;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Simulation;
using Newtonsoft.Json;
using NUnit.Framework;
using Scaffold.Events;
using Scaffold.LiveOps;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.TestTools;
using VContainer;
using Object = UnityEngine.Object;

namespace GearEngine.Campaign.Tests.Editor
{
    public sealed class ActiveRaceViewModelTests
    {
        private sealed class FakeEngine : IGearEngineService
        {
            public bool IsRunning { get; private set; }

            public void Play() => IsRunning = true;

            public void Stop() => IsRunning = false;

            public System.Collections.Generic.IEnumerable<IGridNode> GetAllNodes() => new System.Collections.Generic.List<IGridNode>();
            public void ResetGridSimulationState() => Stop();
        }

        private sealed class NeverCompletingTrackService : ITrackService
        {
            private readonly TaskCompletionSource<bool> recordResultCompletion = new TaskCompletionSource<bool>();

            public NeverCompletingTrackService(TrackDefinition track, CarDefinition car)
            {
                CurrentTrack = track;
                CurrentCar = car;
            }

            public TrackDefinition CurrentTrack { get; }

            public CarDefinition CurrentCar { get; }

            public int RecordResultCallCount { get; private set; }

            public TrackProgressModel GetTrackProgress() => new TrackProgressModel();

            public System.Collections.Generic.IReadOnlyList<TrackEntry> GetOrderedTracks() => Array.Empty<TrackEntry>();

            public bool IsTrackUnlocked(string trackId) => CurrentTrack != null && CurrentTrack.name == trackId;

            public bool TrySelectTrack(string trackId) => IsTrackUnlocked(trackId);

            public Task RecordResultAsync(RaceResultModel result)
            {
                if (result == null)
                {
                    throw new ArgumentNullException(nameof(result));
                }

                RecordResultCallCount++;
                return recordResultCompletion.Task;
            }

            public void CompleteRecordResult()
            {
                recordResultCompletion.TrySetResult(true);
            }
        }

        private sealed class StubBoardService : IBoardService
        {
            public BoardRulesSO BoardRules => null;

            public bool IsSimulationRunning => false;

            public int CurrentBoardGearCount => 0;

            public int MaxAllowedBoardGears => 0;

            public bool ContainsMotorCog => true;

            public event Action<IGridNode> GearPlaced
            {
                add { }
                remove { }
            }

            public event Action<IGridNode> GearRemoved
            {
                add { }
                remove { }
            }

            public event Action BoardLayoutChanged
            {
                add { }
                remove { }
            }

            public BoardModel GetBoard() => null;

            public IGridNode GetNode(Vector2Int coord) => null;

            public System.Collections.Generic.IEnumerable<IGridNode> GetAllNodes() => Array.Empty<IGridNode>();

            public void ToggleSimulation() { }

            public void LoadLayout(BoardLayoutData layout) { }

            public bool TryMoveBoardGear(IGridNode node, Vector2Int toPos, Vector2Int fromPos) => false;

            public bool TryPlace(Vector2Int targetDropPos, GearItemData gearData) => false;

            public bool TryRemoveBoardGear(IGridNode node) => false;

            public bool TryDeleteBoardGear(IGridNode node) => false;

            public void SnapNodeBackToOriginal(IGridNode node, Vector2Int originalPos) { }
        }

        [Test]
        public void Initialize_CreatesSessionAndWaitsForCarBeforeStartingEngine()
        {
            CarDefinition carDef = ScriptableObject.CreateInstance<CarDefinition>();
            TrackDefinition trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            trackDef.Spline.Knots = new[] { new BezierKnot(Vector3.zero), new BezierKnot(Vector3.right * 10f) };
            trackDef.Spline.Closed = false;
            trackDef.SetTiersForTests(new[] { new TrackTierConfig(9999f, 0, 200) });

            RaceState initialSession = CampaignTestUtilities.CreateMinimalSession(carDef, trackDef);
            FakeTrackService trackService = new FakeTrackService(trackDef, carDef);
            FakeEngine engine = new FakeEngine();
            TrackSimulationFactory factory = new TrackSimulationFactory();
            RecordingNavigation navigation = new RecordingNavigation();

            PhysicsSimulationConfig carRunnerConfig = ScriptableObject.CreateInstance<PhysicsSimulationConfig>();
            SplineCarRunnerService carRunner = new SplineCarRunnerService(carRunnerConfig);
            RaceManagerService raceManager = new RaceManagerService(carRunner);

            using (IObjectResolver container = BuildCurrencyContainer(0))
            {
                CurrencyClientModule currency = container.Resolve<CurrencyClientModule>();
                currency.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();

                ActiveRaceViewModel vm = new ActiveRaceViewModel();
                ViewModelTestInject.InjectPrivateField(vm, "trackService", trackService);
                ViewModelTestInject.InjectPrivateField(vm, "engineService", engine);
                ViewModelTestInject.InjectPrivateField(vm, "trackFactory", factory);
                ViewModelTestInject.InjectPrivateField(vm, "raceManager", raceManager);
                ViewModelTestInject.InjectPrivateField(vm, "aiRunner", carRunner);
                ViewModelTestInject.InjectPrivateField(vm, "raceSessionDefaults", new CampaignRaceSessionDefaults(new RaceSessionConfig(), null));
                ViewModelTestInject.InjectPrivateField(vm, "boardService", new StubBoardService());
                ViewModelTestInject.InjectPrivateField(vm, "inventoryService", new RecordingInventoryService());
                ViewModelTestInject.InjectPrivateField(vm, "eventBus", new EventController());
                ViewModelTestInject.InjectNavigation(vm, navigation);

                vm.Bind(navigation);

                Assert.That(engine.IsRunning, Is.False);
                vm.StartRaceAfterCarReady();
                Assert.That(engine.IsRunning, Is.True);
                Assert.That(raceManager.GetFirstRaceForDebug(), Is.SameAs(vm.Track.Session));
                Assert.That(vm.Track.Session, Is.Not.SameAs(initialSession));
            }

            Object.DestroyImmediate(carDef);
            Object.DestroyImmediate(trackDef);
            Object.DestroyImmediate(carRunnerConfig);
        }

        [UnityTest]
        public IEnumerator WhenTrackCompletes_OpensResultPopupAndCreditsCurrency()
        {
            CarDefinition carDef = ScriptableObject.CreateInstance<CarDefinition>();
            TrackDefinition trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            trackDef.Spline.Knots = new[] { new BezierKnot(Vector3.zero), new BezierKnot(Vector3.right * 10f) };
            trackDef.Spline.Closed = false;
            trackDef.SetTiersForTests(new[] { new TrackTierConfig(9999f, 0, 200) });

            RaceState initialSession = CampaignTestUtilities.CreateMinimalSession(carDef, trackDef);
            FakeEngine engine = new FakeEngine();
            TrackSimulationFactory factory = new TrackSimulationFactory();
            RecordingNavigation navigation = new RecordingNavigation();

            PhysicsSimulationConfig carRunnerConfig = ScriptableObject.CreateInstance<PhysicsSimulationConfig>();
            SplineCarRunnerService carRunner = new SplineCarRunnerService(carRunnerConfig);
            RaceManagerService raceManager = new RaceManagerService(carRunner);

            using (IObjectResolver container = BuildCurrencyContainer(0, (req, _) =>
            {
                if (req is AddCurrencyRequest add)
                {
                    return new AddCurrencyResponse(add.CurrencyId, add.Amount, add.Amount);
                }

                return new AddCurrencyResponse("gold", 0, 0);
            }))
            {
                CurrencyClientModule currency = container.Resolve<CurrencyClientModule>();
                currency.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();

                FakeTrackService trackService = new FakeTrackService(trackDef, carDef, currencyClient: currency);

                ActiveRaceViewModel vm = new ActiveRaceViewModel();
                ViewModelTestInject.InjectPrivateField(vm, "trackService", trackService);
                ViewModelTestInject.InjectPrivateField(vm, "engineService", engine);
                ViewModelTestInject.InjectPrivateField(vm, "trackFactory", factory);
                ViewModelTestInject.InjectPrivateField(vm, "raceManager", raceManager);
                ViewModelTestInject.InjectPrivateField(vm, "aiRunner", carRunner);
                ViewModelTestInject.InjectPrivateField(vm, "raceSessionDefaults", new CampaignRaceSessionDefaults(new RaceSessionConfig(), null));
                ViewModelTestInject.InjectPrivateField(vm, "boardService", new StubBoardService());
                ViewModelTestInject.InjectPrivateField(vm, "inventoryService", new RecordingInventoryService());
                ViewModelTestInject.InjectPrivateField(vm, "eventBus", new EventController());
                ViewModelTestInject.InjectNavigation(vm, navigation);

                vm.Bind(navigation);
                vm.Track.Session.Phase = SimulationLifecycleState.Completed;
                vm.Track.Session.TriggerPresentationChanged();
                vm.Track.Session.TriggerPresentationChanged();

                DateTime deadline = DateTime.UtcNow.AddSeconds(5);
                while (DateTime.UtcNow < deadline && navigation.OpenedControllers.Count == 0)
                {
                    vm.Tick(0.1f);
                    yield return null;
                }

                Assert.That(engine.IsRunning, Is.False);
                Assert.That(navigation.OpenedControllers.Count, Is.EqualTo(1));
                Assert.That(navigation.OpenedControllers[0], Is.InstanceOf<ResultPopupViewModel>());
                Assert.That(currency.GetWallet("gold")?.Current ?? 0, Is.GreaterThan(0));
                Assert.That(trackService.RecordResultCallCount, Is.EqualTo(1));
            }

            Object.DestroyImmediate(carDef);
            Object.DestroyImmediate(trackDef);
            Object.DestroyImmediate(carRunnerConfig);
        }

        [UnityTest]
        public IEnumerator WhenResultPersistenceStalls_StillOpensResultPopup()
        {
            CarDefinition carDef = ScriptableObject.CreateInstance<CarDefinition>();
            TrackDefinition trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            trackDef.Spline.Knots = new[] { new BezierKnot(Vector3.zero), new BezierKnot(Vector3.right * 10f) };
            trackDef.Spline.Closed = false;
            trackDef.SetTiersForTests(new[] { new TrackTierConfig(9999f, 0, 200) });

            NeverCompletingTrackService trackService = new NeverCompletingTrackService(trackDef, carDef);
            FakeEngine engine = new FakeEngine();
            TrackSimulationFactory factory = new TrackSimulationFactory();
            RecordingNavigation navigation = new RecordingNavigation();
            PhysicsSimulationConfig carRunnerConfig = ScriptableObject.CreateInstance<PhysicsSimulationConfig>();
            SplineCarRunnerService carRunner = new SplineCarRunnerService(carRunnerConfig);
            RaceManagerService raceManager = new RaceManagerService(carRunner);

            ActiveRaceViewModel vm = new ActiveRaceViewModel();
            ViewModelTestInject.InjectPrivateField(vm, "trackService", trackService);
            ViewModelTestInject.InjectPrivateField(vm, "engineService", engine);
            ViewModelTestInject.InjectPrivateField(vm, "trackFactory", factory);
            ViewModelTestInject.InjectPrivateField(vm, "raceManager", raceManager);
            ViewModelTestInject.InjectPrivateField(vm, "aiRunner", carRunner);
            ViewModelTestInject.InjectPrivateField(vm, "raceSessionDefaults", new CampaignRaceSessionDefaults(new RaceSessionConfig(), null));
            ViewModelTestInject.InjectPrivateField(vm, "boardService", new StubBoardService());
            ViewModelTestInject.InjectPrivateField(vm, "inventoryService", new RecordingInventoryService());
            ViewModelTestInject.InjectPrivateField(vm, "eventBus", new EventController());
            ViewModelTestInject.InjectNavigation(vm, navigation);

            vm.Bind(navigation);
            InvokeRaceCompleted(vm);

            float deadline = Time.realtimeSinceStartup + 3f;
            while (Time.realtimeSinceStartup < deadline && navigation.OpenedControllers.Count == 0)
            {
                vm.Tick(0.1f);
                yield return null;
            }

            bool openedBeforePersistenceCompleted = navigation.OpenedControllers.Count == 1;
            trackService.CompleteRecordResult();
            yield return null;

            Assert.That(trackService.RecordResultCallCount, Is.EqualTo(1));
            Assert.That(openedBeforePersistenceCompleted, Is.True,
                "The result popup must not wait for remote persistence to complete.");
            Assert.That(navigation.OpenedControllers[0], Is.InstanceOf<ResultPopupViewModel>());

            Object.DestroyImmediate(carDef);
            Object.DestroyImmediate(trackDef);
            Object.DestroyImmediate(carRunnerConfig);
        }

        private static void InvokeRaceCompleted(ActiveRaceViewModel viewModel)
        {
            System.Reflection.MethodInfo method = typeof(ActiveRaceViewModel).GetMethod(
                "OnRaceCompleted",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "The race completion handler must remain available for the regression test.");
            method.Invoke(viewModel, null);
        }

        private static CurrencyGameData BuildGameData(long gold)
        {
            CurrencyPersistence persistence = new CurrencyPersistence();
            persistence.Set("gold", gold);
            CurrencyConfig config = JsonConvert.DeserializeObject<CurrencyConfig>(
                "{\"entries\":[{\"id\":\"gold\",\"initial\":0}]}");
            return new CurrencyGameData(persistence, config);
        }

        private static IObjectResolver BuildCurrencyContainer(long initialGold, Func<object, CancellationToken, ModuleResponse> onCall = null)
        {
            FakeLiveOpsService fake = new FakeLiveOpsService
            {
                ModuleData = BuildGameData(initialGold),
                CallImpl = onCall ?? ((_, _) => new AddCurrencyResponse("gold", 0, 0)),
            };

            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterInstance<ILiveOpsService>(fake);
            builder.RegisterInstance<Scaffold.Analytics.IAnalyticsService>(new RecordingAnalytics());
            builder.RegisterInstance<Scaffold.Events.Contracts.IEventBus>(new EventController());
            builder.Register<CurrencyClientModule>(Lifetime.Singleton);
            return builder.Build();
        }

        private sealed class RecordingAnalytics : Scaffold.Analytics.IAnalyticsService
        {
            public readonly List<Scaffold.Analytics.AnalyticsEvent> Events = new List<Scaffold.Analytics.AnalyticsEvent>();

            public void Record<T>(T evt) where T : Scaffold.Analytics.AnalyticsEvent
            {
                Events.Add(evt);
            }
        }

        private sealed class FakeLiveOpsService : ILiveOpsService
        {
            public CurrencyGameData ModuleData { get; set; }

            public Func<object, CancellationToken, ModuleResponse> CallImpl { get; set; }

            public T GetModuleData<T>()
                where T : class, IGameModuleData
            {
                return ModuleData as T;
            }

            public Task<TResponse> CallAsync<TResponse>(ModuleRequest<TResponse> request, CancellationToken cancellationToken = default)
                where TResponse : ModuleResponse
            {
                if (CallImpl == null)
                {
                    throw new InvalidOperationException("CallImpl not set");
                }

                ModuleResponse result = CallImpl((object)request, cancellationToken);
                return Task.FromResult((TResponse)result);
            }
        }
    }
}
