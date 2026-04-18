using GearEngine.Campaign.Presentation;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine;
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
        }

        private sealed class RecordingRaceSessionRunner : IRaceSessionRunner
        {
            public LapRaceSession LastSetSession { get; private set; }

            public LapRaceSession ActiveSession => LastSetSession;

            public void SetSession(LapRaceSession session)
            {
                LastSetSession = session;
            }

            public void Tick()
            {
            }
        }

        [Test]
        public void Initialize_CreatesSessionRegistersRunnerAndStartsEngine()
        {
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            trackDef.Spline.Knots = new[] { new BezierKnot(Vector3.zero), new BezierKnot(Vector3.right * 10f) };
            trackDef.Spline.Closed = false;
            trackDef.SetScoreBandsForTests(new[] { new TrackScoreBand(9999f, 200) });

            LapRaceSession initialSession = CampaignTestUtilities.CreateMinimalSession(carDef, trackDef);
            var trackService = new FakeTrackService(trackDef, carDef, initialSession);
            var wallet = new FakeWalletService();
            var engine = new FakeEngine();
            var runner = new RecordingRaceSessionRunner();
            var factory = new TrackSimulationFactory();
            var navigation = new RecordingNavigation();

            var vm = new ActiveRaceViewModel();
            ViewModelTestInject.InjectPrivateField(vm, "trackService", trackService);
            ViewModelTestInject.InjectPrivateField(vm, "walletService", wallet);
            ViewModelTestInject.InjectPrivateField(vm, "engineService", engine);
            ViewModelTestInject.InjectPrivateField(vm, "trackFactory", factory);
            ViewModelTestInject.InjectPrivateField(vm, "raceSessionRunner", runner);
            ViewModelTestInject.InjectNavigation(vm, navigation);

            ViewModelTestInject.InvokeInitialize(vm);

            Assert.That(engine.IsRunning, Is.True);
            Assert.That(runner.LastSetSession, Is.Not.Null);
            Assert.That(trackService.CurrentSession, Is.SameAs(runner.LastSetSession));
            Assert.That(vm.Track.Session, Is.SameAs(trackService.CurrentSession));
            Assert.That(vm.Track.SpawnCarOnBindIfNoChild, Is.True);

            Object.DestroyImmediate(carDef);
            Object.DestroyImmediate(trackDef);
        }

        [Test]
        public void WhenTrackCompletes_OpensResultPopupAndCreditsWallet()
        {
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            trackDef.Spline.Knots = new[] { new BezierKnot(Vector3.zero), new BezierKnot(Vector3.right * 10f) };
            trackDef.Spline.Closed = false;
            trackDef.SetScoreBandsForTests(new[] { new TrackScoreBand(9999f, 200) });

            LapRaceSession initialSession = CampaignTestUtilities.CreateMinimalSession(carDef, trackDef);
            var trackService = new FakeTrackService(trackDef, carDef, initialSession);
            var wallet = new FakeWalletService();
            var engine = new FakeEngine();
            var runner = new RecordingRaceSessionRunner();
            var factory = new TrackSimulationFactory();
            var navigation = new RecordingNavigation();

            var vm = new ActiveRaceViewModel();
            ViewModelTestInject.InjectPrivateField(vm, "trackService", trackService);
            ViewModelTestInject.InjectPrivateField(vm, "walletService", wallet);
            ViewModelTestInject.InjectPrivateField(vm, "engineService", engine);
            ViewModelTestInject.InjectPrivateField(vm, "trackFactory", factory);
            ViewModelTestInject.InjectPrivateField(vm, "raceSessionRunner", runner);
            ViewModelTestInject.InjectNavigation(vm, navigation);

            ViewModelTestInject.InvokeInitialize(vm);
            vm.Track.Complete();

            Assert.That(engine.IsRunning, Is.False);
            Assert.That(navigation.OpenedControllers.Count, Is.EqualTo(1));
            Assert.That(navigation.OpenedControllers[0], Is.InstanceOf<ResultPopupViewModel>());
            Assert.That(wallet.CurrentGold, Is.GreaterThan(0));
            Assert.That(trackService.RecordResultCallCount, Is.EqualTo(1));

            Object.DestroyImmediate(carDef);
            Object.DestroyImmediate(trackDef);
        }
    }
}
