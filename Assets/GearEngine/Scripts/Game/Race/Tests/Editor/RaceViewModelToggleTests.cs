using GearEngine.GearEngine.Nodes;
using System;
using System.Reflection;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Bootstrap;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Services;
using GearEngine.GearEngine.Services.Board;
using GearEngine.GearEngine.Services.Inventory;
using NUnit.Framework;
using Scaffold.MVVM;
using Scaffold.Navigation.Contracts;
using Scaffold.Events.Container;
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

            public System.Collections.Generic.IEnumerable<IGridNode> GetAllNodes() => new System.Collections.Generic.List<IGridNode>();
            public void ResetGridSimulationState()
            {
                throw new NotImplementedException();
            }

            public void Stop() => IsRunning = false;
        }

        [Test]
        public void Initialize_RegistersRaceStateToManager()
        {
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            RaceViewModel vm = null;
            IObjectResolver scope = null;
            try
            {
                var carRunnerConfig = ScriptableObject.CreateInstance<SplineCarRunnerConfigSO>();
                var carRunner = new SplineCarRunnerService(carRunnerConfig);
                var manager = new RaceManagerService(carRunner);

                (vm, scope) = CreateInitializedRaceViewModel(carDef, trackDef, out _, manager, carRunner);
                Assert.That(vm.Track.Session, Is.Not.Null);
                Assert.That(manager.GetFirstRaceForDebug(), Is.SameAs(vm.Track.Session));
            }
            finally
            {
                TearDownRaceViewModel(vm, scope);
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
            IObjectResolver scope = null;
            try
            {
                FakeEngine engine;
                var carRunnerConfig = ScriptableObject.CreateInstance<SplineCarRunnerConfigSO>();
                var carRunner = new SplineCarRunnerService(carRunnerConfig);
                var manager = new RaceManagerService(carRunner);
                (vm, scope) = CreateInitializedRaceViewModel(carDef, trackDef, out engine, manager, carRunner);
                Assert.That(engine.IsRunning, Is.False);
                Assert.That(vm.Track.State, Is.EqualTo(SimulationLifecycleState.Created));

                vm.ToggleRace();

                Assert.That(engine.IsRunning, Is.True);
                Assert.That(vm.IsRaceRunning, Is.True);
                Assert.That(vm.Track.State, Is.EqualTo(SimulationLifecycleState.Running));
            }
            finally
            {
                TearDownRaceViewModel(vm, scope);
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
            IObjectResolver scope = null;
            try
            {
                FakeEngine engine;
                var carRunnerConfig = ScriptableObject.CreateInstance<SplineCarRunnerConfigSO>();
                var carRunner = new SplineCarRunnerService(carRunnerConfig);
                var manager = new RaceManagerService(carRunner);
                (vm, scope) = CreateInitializedRaceViewModel(carDef, trackDef, out engine, manager, carRunner);
                vm.ToggleRace();
                vm.ToggleRace();

                Assert.That(engine.IsRunning, Is.False);
                Assert.That(vm.IsRaceRunning, Is.False);
                Assert.That(vm.Track.State, Is.EqualTo(SimulationLifecycleState.Paused));
            }
            finally
            {
                TearDownRaceViewModel(vm, scope);
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
            IObjectResolver scope = null;
            try
            {
                FakeEngine engine;
                var carRunnerConfig = ScriptableObject.CreateInstance<SplineCarRunnerConfigSO>();
                var carRunner = new SplineCarRunnerService(carRunnerConfig);
                var manager = new RaceManagerService(carRunner);
                (vm, scope) = CreateInitializedRaceViewModel(carDef, trackDef, out engine, manager, carRunner);
                vm.ToggleRace();
                vm.ToggleRace();
                vm.ToggleRace();

                Assert.That(engine.IsRunning, Is.True);
                Assert.That(vm.Track.State, Is.EqualTo(SimulationLifecycleState.Running));
            }
            finally
            {
                TearDownRaceViewModel(vm, scope);
                Object.DestroyImmediate(carDef);
                Object.DestroyImmediate(trackDef);
            }
        }

        private static (RaceViewModel vm, IObjectResolver scope) CreateInitializedRaceViewModel(
            CarDefinition carDef,
            TrackDefinition trackDef,
            out FakeEngine engine,
            RaceManagerService raceManager,
            SplineCarRunnerService aiRunner)
        {
            trackDef.Spline.Knots = new[] { new BezierKnot(Vector3.zero), new BezierKnot(Vector3.right * 10f) };
            trackDef.Spline.Closed = false;
            var startData = new RaceStartData(trackDef, carDef);

            engine = new FakeEngine();
            BoardRulesSO boardRules = ScriptableObject.CreateInstance<BoardRulesSO>();
            boardRules.GridWidth = 5;
            boardRules.GridHeight = 5;

            var builder = new ContainerBuilder();
            new EventsInstaller().Install(builder);
            new GearMechanicsInstaller(
                boardRules,
                null,
                GearInventoryLoadoutData.Empty(),
                new GearBoardLoadoutData()).Install(builder);
            IObjectResolver scope = builder.Build();
            IRaceInventoryService inventoryService = scope.Resolve<IRaceInventoryService>();
            IBoardService boardService = scope.Resolve<IBoardService>();
            IDragService dragService = scope.Resolve<IDragService>();

            var trackFactory = new TrackSimulationFactory(scope);
            var vm = new RaceViewModel(startData);
            InjectPrivateField(vm, "engineService", engine);
            InjectPrivateField(vm, "inventoryService", inventoryService);
            InjectPrivateField(vm, "boardService", boardService);
            InjectPrivateField(vm, "dragService", dragService);
            InjectPrivateField(vm, "trackFactory", trackFactory);
            InjectPrivateField(vm, "raceManager", raceManager);
            InjectPrivateField(vm, "aiRunner", aiRunner);
            InjectPrivateFieldInHierarchy(vm, "navigation", new NoOpNavigation());

            InvokeProtectedInitialize(vm);
            return (vm, scope);
        }

        private static void TearDownRaceViewModel(RaceViewModel vm, IObjectResolver scope)
        {
            BoardRulesSO boardRules = vm?.Board?.BoardRules;
            scope?.Dispose();
            if (boardRules != null)
            {
                Object.DestroyImmediate(boardRules);
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
