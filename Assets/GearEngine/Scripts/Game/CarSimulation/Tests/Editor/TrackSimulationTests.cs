using System.Reflection;
using System.Text.RegularExpressions;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using NUnit.Framework;
using Scaffold.MVVM;
using UnityEngine;
using UnityEngine.TestTools;

namespace GearEngine.CarSimulation.Tests
{
    public sealed class TrackSimulationTests
    {
        [Test]
        public void Factory_Create_ReturnsSimulationWithCarAndTrack()
        {
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            try
            {
                var factory = new TrackSimulationFactory();
                TrackSimulation sim = factory.Create(carDef, trackDef);
                Assert.That(sim.Track, Is.SameAs(trackDef));
                Assert.That(sim.Car, Is.Not.Null);
                Assert.That(sim.State != SimulationLifecycleState.Running);
            }
            finally
            {
                Object.DestroyImmediate(carDef);
                Object.DestroyImmediate(trackDef);
            }
        }

        [Test]
        public void Factory_Create_ThrowsOnNullCarDefinition()
        {
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            try
            {
                var factory = new TrackSimulationFactory();
                LogAssert.Expect(LogType.Error, new Regex(@"\[TrackSimulationFactory\] Create failed:.*"));
                Assert.Throws<System.ArgumentNullException>(() => factory.Create(null, trackDef));
            }
            finally
            {
                Object.DestroyImmediate(trackDef);
            }
        }

        [Test]
        public void Toggle_PausesAndResumes_IsRunning()
        {
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            try
            {
                TrackSimulation sim = new TrackSimulationFactory().Create(carDef, trackDef);
                sim.Toggle(true);
                Assert.That(sim.State == SimulationLifecycleState.Running);
                sim.Toggle(false);
                Assert.That(sim.State == SimulationLifecycleState.Paused);
                sim.Toggle(true);
                Assert.That(sim.State == SimulationLifecycleState.Running);
            }
            finally
            {
                Object.DestroyImmediate(carDef);
                Object.DestroyImmediate(trackDef);
            }
        }

        [Test]
        public void Complete_FromRunning_ThenToggleThrows()
        {
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            try
            {
                TrackSimulation sim = new TrackSimulationFactory().Create(carDef, trackDef);
                sim.Toggle(true);
                sim.Complete();
                Assert.That(sim.State != SimulationLifecycleState.Running);
                LogAssert.Expect(LogType.Error, new Regex(@"\[TrackSimulation\] Toggle failed:.*"));
                Assert.Throws<System.InvalidOperationException>(() => sim.Toggle(true));
            }
            finally
            {
                Object.DestroyImmediate(carDef);
                Object.DestroyImmediate(trackDef);
            }
        }

        [Test]
        public void Complete_ThrowsWhenNotRunningOrPaused()
        {
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            try
            {
                TrackSimulation sim = new TrackSimulationFactory().Create(carDef, trackDef);
                LogAssert.Expect(LogType.Error, new Regex(@"\[TrackSimulation\] Complete failed:.*"));
                Assert.Throws<System.InvalidOperationException>(() => sim.Complete());
            }
            finally
            {
                Object.DestroyImmediate(carDef);
                Object.DestroyImmediate(trackDef);
            }
        }
    }
}
