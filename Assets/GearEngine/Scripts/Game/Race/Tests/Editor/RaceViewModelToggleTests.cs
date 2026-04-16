using System;
using System.Reflection;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Bootstrap;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Manager;
using GearEngine.GearEngine.Nodes;
using GearEngine.Race.Rewards;
using NUnit.Framework;
using Scaffold.Navigation.Contracts;
using UnityEngine;
using UnityEngine.Splines;
using VContainer;
using Object = UnityEngine.Object;

namespace GearEngine.Race.Tests.Editor
{
    public sealed class RaceViewModelToggleTests
    {
        private sealed class NoOpNavigation : INavigation
        {
            public IViewController CurrentController => null;

            public void Open<TViewController>(TViewController controller, bool closeCurrent = false, NavigationOptions options = null)
                where TViewController : IViewController
            {
            }

            public void Close<TViewController>(TViewController controller) where TViewController : IViewController
            {
            }

            public IViewController Return()
            {
                return null;
            }
        }

        private sealed class FakeEngine : IGearEngineService
        {
            public bool IsRunning { get; set; }

            public void Play() => IsRunning = true;

            public void Stop() => IsRunning = false;
        }

        private sealed class RecordingTrackSimulationRunner : ITrackSimulationRunner
        {
            public TrackSimulation LastSimulation { get; private set; }

            public void SetSimulation(TrackSimulation simulation)
            {
                LastSimulation = simulation;
            }
        }

        [Test]
        public void Initialize_BindsTrackSimulationToRunner()
        {
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            RaceViewModel vm = null;
            try
            {
                var runnerSink = new RecordingTrackSimulationRunner();
                vm = CreateInitializedRaceViewModel(carDef, trackDef, out _, runnerSink);
                Assert.That(runnerSink.LastSimulation, Is.Not.Null);
                Assert.That(runnerSink.LastSimulation, Is.SameAs(vm.Track.Simulation));
            }
            finally
            {
                TearDownRaceViewModel(vm);
                Object.DestroyImmediate(carDef);
                Object.DestroyImmediate(trackDef);
            }
        }

        [Test]
        public void ToggleRace_WhenStopped_StartsEngineAndTrack()
        {
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            RaceViewModel vm = null;
            try
            {
                FakeEngine engine;
                vm = CreateInitializedRaceViewModel(carDef, trackDef, out engine, new RecordingTrackSimulationRunner());
                Assert.That(engine.IsRunning, Is.False);
                Assert.That(vm.Track.State, Is.EqualTo(SimulationLifecycleState.Created));

                vm.ToggleRace();

                Assert.That(engine.IsRunning, Is.True);
                Assert.That(vm.IsRaceRunning, Is.True);
                Assert.That(vm.Track.State, Is.EqualTo(SimulationLifecycleState.Running));
            }
            finally
            {
                TearDownRaceViewModel(vm);
                Object.DestroyImmediate(carDef);
                Object.DestroyImmediate(trackDef);
            }
        }

        [Test]
        public void ToggleRace_WhenRunning_StopsEngineAndPausesTrack()
        {
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            RaceViewModel vm = null;
            try
            {
                FakeEngine engine;
                vm = CreateInitializedRaceViewModel(carDef, trackDef, out engine, new RecordingTrackSimulationRunner());
                vm.ToggleRace();
                vm.ToggleRace();

                Assert.That(engine.IsRunning, Is.False);
                Assert.That(vm.IsRaceRunning, Is.False);
                Assert.That(vm.Track.State, Is.EqualTo(SimulationLifecycleState.Paused));
            }
            finally
            {
                TearDownRaceViewModel(vm);
                Object.DestroyImmediate(carDef);
                Object.DestroyImmediate(trackDef);
            }
        }

        [Test]
        public void ToggleRace_ToggleTwice_ResumesCorrectly()
        {
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            RaceViewModel vm = null;
            try
            {
                FakeEngine engine;
                vm = CreateInitializedRaceViewModel(carDef, trackDef, out engine, new RecordingTrackSimulationRunner());
                vm.ToggleRace();
                vm.ToggleRace();
                vm.ToggleRace();

                Assert.That(engine.IsRunning, Is.True);
                Assert.That(vm.Track.State, Is.EqualTo(SimulationLifecycleState.Running));
            }
            finally
            {
                TearDownRaceViewModel(vm);
                Object.DestroyImmediate(carDef);
                Object.DestroyImmediate(trackDef);
            }
        }

        private static RaceViewModel CreateInitializedRaceViewModel(
            CarDefinition carDef,
            TrackDefinition trackDef,
            out FakeEngine engine,
            ITrackSimulationRunner trackSimulationRunner)
        {
            trackDef.Spline.Knots = new[] { new BezierKnot(Vector3.zero), new BezierKnot(Vector3.right * 10f) };
            trackDef.Spline.Closed = false;
            var startData = new RaceStartData(trackDef, carDef);

            engine = new FakeEngine();
            var gridManager = new GridManager();
            BoardConfigSO boardConfig = ScriptableObject.CreateInstance<BoardConfigSO>();
            boardConfig.GridWidth = 5;
            boardConfig.GridHeight = 5;

            GearNodeFactory nodeFactory;
            var builder = new ContainerBuilder();
            builder.RegisterInstance(gridManager).As<IGridManager>();
            builder.RegisterInstance(boardConfig);
            builder.Register<BaseGearNode>(Lifetime.Transient);
            builder.Register<CoreGearNode>(Lifetime.Transient);
            builder.Register<AuraGearNode>(Lifetime.Transient);
            builder.Register<GearNodeFactory>(Lifetime.Singleton);
            using (IObjectResolver container = builder.Build())
            {
                nodeFactory = container.Resolve<GearNodeFactory>();
            }

            var trackFactory = new TrackSimulationFactory();
            var vm = new RaceViewModel(startData);
            InjectPrivateField(vm, "engineService", engine);
            InjectPrivateField(vm, "gridManager", gridManager);
            InjectPrivateField(vm, "nodeFactory", nodeFactory);
            InjectPrivateField(vm, "boardConfig", boardConfig);
            InjectPrivateField(vm, "trackFactory", trackFactory);
            InjectPrivateField(vm, "trackSimulationRunner", trackSimulationRunner);
            InjectPrivateField(vm, "raceRewardLiveOps", new StubRaceRewardLiveOpsClient());
            InjectPrivateFieldInHierarchy(vm, "navigation", new NoOpNavigation());

            InvokeProtectedInitialize(vm);
            return vm;
        }

        private static void TearDownRaceViewModel(RaceViewModel vm)
        {
            vm?.TearDown();
            if (vm?.Board?.BoardConfig != null)
            {
                Object.DestroyImmediate(vm.Board.BoardConfig);
            }
        }

        private static void InvokeProtectedInitialize(RaceViewModel vm)
        {
            MethodInfo init = typeof(RaceViewModel).GetMethod(
                "Initialize",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(init, Is.Not.Null);
            init.Invoke(vm, null);
        }

        private static void InjectPrivateField(object target, string name, object value)
        {
            FieldInfo field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(target, value);
        }

        private static void InjectPrivateFieldInHierarchy(object target, string name, object value)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            for (Type type = target.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(name, flags);
                if (field != null)
                {
                    field.SetValue(target, value);
                    return;
                }
            }

            Assert.Fail($"Field '{name}' not found on type hierarchy of {target.GetType()}.");
        }
    }
}
