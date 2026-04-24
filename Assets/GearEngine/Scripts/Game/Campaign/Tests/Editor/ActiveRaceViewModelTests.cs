using System;
using System.Threading;
using System.Threading.Tasks;
using LiveOps.DTO.GameModule;
using LiveOps.DTO.ModuleRequest;
using LiveOps.Modules.DTO.Currency;
using LiveOps.Modules.DTO.ModuleRequests;
using GearEngine.GearEngine.Nodes;
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
using Scaffold.LiveOps;
using UnityEngine;
using UnityEngine.Splines;
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

        [Test]
        public void Initialize_CreatesSessionRegistersRunnerAndStartsEngine()
        {
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            trackDef.Spline.Knots = new[] { new BezierKnot(Vector3.zero), new BezierKnot(Vector3.right * 10f) };
            trackDef.Spline.Closed = false;
            trackDef.SetScoreBandsForTests(new[] { new TrackScoreBand(9999f, 200) });

            RaceState initialSession = CampaignTestUtilities.CreateMinimalSession(carDef, trackDef);
            var trackService = new FakeTrackService(trackDef, carDef);
            var engine = new FakeEngine();
            var factory = new TrackSimulationFactory();
            var navigation = new RecordingNavigation();

            var carRunnerConfig = ScriptableObject.CreateInstance<SplineCarRunnerConfigSO>();
            var carRunner = new SplineCarRunnerService(carRunnerConfig);
            var raceManager = new RaceManagerService(carRunner);

            using (IObjectResolver container = BuildCurrencyContainer(0))
            {
                CurrencyClientModule currency = container.Resolve<CurrencyClientModule>();
                currency.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();

                var vm = new ActiveRaceViewModel();
                ViewModelTestInject.InjectPrivateField(vm, "trackService", trackService);
                ViewModelTestInject.InjectPrivateField(vm, "engineService", engine);
                ViewModelTestInject.InjectPrivateField(vm, "trackFactory", factory);
                ViewModelTestInject.InjectPrivateField(vm, "raceManager", raceManager);
                ViewModelTestInject.InjectPrivateField(vm, "aiRunner", carRunner);
                ViewModelTestInject.InjectPrivateField(vm, "raceSessionDefaults", new CampaignRaceSessionDefaults(new RaceSessionConfig(), null));
                ViewModelTestInject.InjectNavigation(vm, navigation);

                ViewModelTestInject.InvokeInitialize(vm);

                Assert.That(engine.IsRunning, Is.True);
                Assert.That(raceManager.GetFirstRaceForDebug(), Is.SameAs(vm.Track.Session));
                Assert.That(vm.Track.Session, Is.Not.SameAs(initialSession));
            }

            Object.DestroyImmediate(carDef);
            Object.DestroyImmediate(trackDef);
            Object.DestroyImmediate(carRunnerConfig);
        }

        [Test]
        public void WhenTrackCompletes_OpensResultPopupAndCreditsCurrency()
        {
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            trackDef.Spline.Knots = new[] { new BezierKnot(Vector3.zero), new BezierKnot(Vector3.right * 10f) };
            trackDef.Spline.Closed = false;
            trackDef.SetScoreBandsForTests(new[] { new TrackScoreBand(9999f, 200) });

            RaceState initialSession = CampaignTestUtilities.CreateMinimalSession(carDef, trackDef);
            var engine = new FakeEngine();
            var factory = new TrackSimulationFactory();
            var navigation = new RecordingNavigation();

            var carRunnerConfig = ScriptableObject.CreateInstance<SplineCarRunnerConfigSO>();
            var carRunner = new SplineCarRunnerService(carRunnerConfig);
            var raceManager = new RaceManagerService(carRunner);

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

                var trackService = new FakeTrackService(trackDef, carDef, currencyClient: currency);

                var vm = new ActiveRaceViewModel();
                ViewModelTestInject.InjectPrivateField(vm, "trackService", trackService);
                ViewModelTestInject.InjectPrivateField(vm, "engineService", engine);
                ViewModelTestInject.InjectPrivateField(vm, "trackFactory", factory);
                ViewModelTestInject.InjectPrivateField(vm, "raceManager", raceManager);
                ViewModelTestInject.InjectPrivateField(vm, "aiRunner", carRunner);
                ViewModelTestInject.InjectPrivateField(vm, "raceSessionDefaults", new CampaignRaceSessionDefaults(new RaceSessionConfig(), null));
                ViewModelTestInject.InjectNavigation(vm, navigation);

                ViewModelTestInject.InvokeInitialize(vm);
                vm.Track.Complete();

                var deadline = DateTime.UtcNow.AddSeconds(2);
                while (DateTime.UtcNow < deadline && navigation.OpenedControllers.Count == 0)
                {
                    Thread.Sleep(10);
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

        private static CurrencyGameData BuildGameData(long gold)
        {
            var persistence = new CurrencyPersistence();
            persistence.Set("gold", gold);
            CurrencyConfig config = JsonConvert.DeserializeObject<CurrencyConfig>(
                "{\"entries\":[{\"id\":\"gold\",\"initial\":0}]}");
            return new CurrencyGameData(persistence, config);
        }

        private static IObjectResolver BuildCurrencyContainer(long initialGold, Func<object, CancellationToken, ModuleResponse> onCall = null)
        {
            var fake = new FakeLiveOpsService
            {
                ModuleData = BuildGameData(initialGold),
                CallImpl = onCall ?? ((_, _) => new AddCurrencyResponse("gold", 0, 0)),
            };

            var builder = new ContainerBuilder();
            builder.RegisterInstance<ILiveOpsService>(fake);
            builder.Register<CurrencyClientModule>(Lifetime.Singleton);
            return builder.Build();
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
