using System.Reflection;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Bootstrap;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Manager;
using GearEngine.GearEngine.Nodes;
using NUnit.Framework;
using UnityEngine;
using VContainer;

namespace GearEngine.Race.Tests.Editor
{
    public sealed class RaceViewModelToggleTests
    {
        private sealed class FakeEngine : IGearEngineService
        {
            public bool IsRunning { get; set; }

            public void Play() => IsRunning = true;

            public void Stop() => IsRunning = false;
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
                vm = CreateInitializedRaceViewModel(carDef, trackDef, out engine);
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
                vm = CreateInitializedRaceViewModel(carDef, trackDef, out engine);
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
                vm = CreateInitializedRaceViewModel(carDef, trackDef, out engine);
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
            out FakeEngine engine)
        {
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

            InvokeProtectedInitialize(vm);
            return vm;
        }

        private static void TearDownRaceViewModel(RaceViewModel vm)
        {
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
    }
}
