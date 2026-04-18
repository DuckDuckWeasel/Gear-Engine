using System.Collections.Generic;
using System.Reflection;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Presentation;
using GearEngine.CarSimulation.Simulation;
using NUnit.Framework;
using Scaffold.MVVM;
using UnityEngine;
using UnityEngine.Splines;
using Object = UnityEngine.Object;

namespace GearEngine.CarSimulation.Tests
{
    public sealed class CarTrackScreenViewModelTests
    {
        [Test]
        public void Initialize_DoesNotRegisterSessionsWithRaceManager()
        {
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            trackDef.Spline.Knots = new[] { new BezierKnot(Vector3.zero), new BezierKnot(Vector3.right * 10f) };
            trackDef.Spline.Closed = false;

            var carRunnerConfig = ScriptableObject.CreateInstance<SplineCarRunnerConfigSO>();
            var aiRunner = new SplineCarRunnerService(carRunnerConfig);
            var raceManager = new RaceManagerService(aiRunner);
            var factory = new TrackSimulationFactory();

            CarTrackScreenViewModel vm = null;
            try
            {
                vm = new CarTrackScreenViewModel(trackDef, new List<CarDefinition> { carDef }, null);
                Inject(vm, "factory", factory);
                Inject(vm, "raceManager", raceManager);
                Inject(vm, "aiRunner", aiRunner);
                InvokeInitialize(vm);

                Assert.That(raceManager.GetFirstRaceForDebug(), Is.Null);
                Assert.That(vm.Sessions.Count, Is.EqualTo(1));
                Assert.That(vm.Track, Is.Not.Null);
                Assert.That(vm.Cars.Count, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(carDef);
                Object.DestroyImmediate(trackDef);
                Object.DestroyImmediate(carRunnerConfig);
            }
        }

        [Test]
        public void ToggleRace_RegistersSessionsAndStartsPrimary()
        {
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            trackDef.Spline.Knots = new[] { new BezierKnot(Vector3.zero), new BezierKnot(Vector3.right * 10f) };
            trackDef.Spline.Closed = false;

            var carRunnerConfig = ScriptableObject.CreateInstance<SplineCarRunnerConfigSO>();
            var aiRunner = new SplineCarRunnerService(carRunnerConfig);
            var raceManager = new RaceManagerService(aiRunner);
            var factory = new TrackSimulationFactory();

            CarTrackScreenViewModel vm = null;
            try
            {
                vm = new CarTrackScreenViewModel(trackDef, new List<CarDefinition> { carDef }, null);
                Inject(vm, "factory", factory);
                Inject(vm, "raceManager", raceManager);
                Inject(vm, "aiRunner", aiRunner);
                InvokeInitialize(vm);

                vm.ToggleRace();

                Assert.That(raceManager.GetFirstRaceForDebug(), Is.SameAs(vm.Sessions[0]));
                Assert.That(vm.Sessions[0].Phase, Is.EqualTo(SimulationLifecycleState.Running));
            }
            finally
            {
                Object.DestroyImmediate(carDef);
                Object.DestroyImmediate(trackDef);
                Object.DestroyImmediate(carRunnerConfig);
            }
        }

        private static void InvokeInitialize(ViewModel viewModel)
        {
            MethodInfo init = typeof(ViewModel).GetMethod(
                "Initialize",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(init, Is.Not.Null);
            init.Invoke(viewModel, null);
        }

        private static void Inject(object target, string name, object value)
        {
            FieldInfo field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(target, value);
        }
    }
}
