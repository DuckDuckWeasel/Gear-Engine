using GearEngine.Campaign.Presentation;
using GearEngine.GearEngine;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Simulation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Splines;
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
            var wallet = new FakeWalletService();
            var engine = new FakeEngine();
            var factory = new TrackSimulationFactory();
            var navigation = new RecordingNavigation();

            var carRunnerConfig = ScriptableObject.CreateInstance<SplineCarRunnerConfigSO>();
            var carRunner = new SplineCarRunnerService(carRunnerConfig);
            var raceManager = new RaceManagerService(carRunner);

            var vm = new ActiveRaceViewModel();
            ViewModelTestInject.InjectPrivateField(vm, "trackService", trackService);
            ViewModelTestInject.InjectPrivateField(vm, "walletService", wallet);
            ViewModelTestInject.InjectPrivateField(vm, "engineService", engine);
            ViewModelTestInject.InjectPrivateField(vm, "trackFactory", factory);
            ViewModelTestInject.InjectPrivateField(vm, "raceManager", raceManager);
            ViewModelTestInject.InjectPrivateField(vm, "aiRunner", carRunner);
            ViewModelTestInject.InjectNavigation(vm, navigation);

            ViewModelTestInject.InvokeInitialize(vm);

            Assert.That(engine.IsRunning, Is.True);
            Assert.That(raceManager.GetFirstRaceForDebug(), Is.SameAs(vm.Track.Session));
            Assert.That(vm.Track.Session, Is.Not.SameAs(initialSession));

            Object.DestroyImmediate(carDef);
            Object.DestroyImmediate(trackDef);
            Object.DestroyImmediate(carRunnerConfig);
        }

        [Test]
        public void WhenTrackCompletes_OpensResultPopupAndCreditsWallet()
        {
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            trackDef.Spline.Knots = new[] { new BezierKnot(Vector3.zero), new BezierKnot(Vector3.right * 10f) };
            trackDef.Spline.Closed = false;
            trackDef.SetScoreBandsForTests(new[] { new TrackScoreBand(9999f, 200) });

            RaceState initialSession = CampaignTestUtilities.CreateMinimalSession(carDef, trackDef);
            var trackService = new FakeTrackService(trackDef, carDef);
            var wallet = new FakeWalletService();
            var engine = new FakeEngine();
            var factory = new TrackSimulationFactory();
            var navigation = new RecordingNavigation();

            var carRunnerConfig = ScriptableObject.CreateInstance<SplineCarRunnerConfigSO>();
            var carRunner = new SplineCarRunnerService(carRunnerConfig);
            var raceManager = new RaceManagerService(carRunner);

            var vm = new ActiveRaceViewModel();
            ViewModelTestInject.InjectPrivateField(vm, "trackService", trackService);
            ViewModelTestInject.InjectPrivateField(vm, "walletService", wallet);
            ViewModelTestInject.InjectPrivateField(vm, "engineService", engine);
            ViewModelTestInject.InjectPrivateField(vm, "trackFactory", factory);
            ViewModelTestInject.InjectPrivateField(vm, "raceManager", raceManager);
            ViewModelTestInject.InjectPrivateField(vm, "aiRunner", carRunner);
            ViewModelTestInject.InjectNavigation(vm, navigation);

            ViewModelTestInject.InvokeInitialize(vm);
            vm.Track.Complete();

            Assert.That(engine.IsRunning, Is.False);
            Assert.That(navigation.OpenedControllers.Count, Is.EqualTo(1));
            Assert.That(navigation.OpenedControllers[0], Is.InstanceOf<ResultPopupViewModel>());
            Assert.That(wallet.GetWallet().Gold, Is.GreaterThan(0));
            Assert.That(trackService.RecordResultCallCount, Is.EqualTo(1));

            Object.DestroyImmediate(carDef);
            Object.DestroyImmediate(trackDef);
            Object.DestroyImmediate(carRunnerConfig);
        }
    }
}
