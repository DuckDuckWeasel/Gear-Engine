using System.Reflection;
using System.Text.RegularExpressions;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using NUnit.Framework;
using Scaffold.MVVM;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.TestTools;

namespace GearEngine.CarSimulation.Tests
{
    public sealed class TrackSimulationTests
    {
        private static void SeedMinimalOpenTrack(TrackDefinition trackDef)
        {
            trackDef.Spline.Knots = new[] { new BezierKnot(Vector3.zero), new BezierKnot(Vector3.right * 10f) };
            trackDef.Spline.Closed = false;
        }

        [Test]
        public void Factory_Create_ThrowsOnNullCarDefinition()
        {
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            try
            {
                var factory = new TrackSimulationFactory();
                Assert.Throws<System.ArgumentNullException>(() => factory.Create(null, trackDef, null));
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
                SeedMinimalOpenTrack(trackDef);
                TrackSimulation sim = new TrackSimulationFactory().Create(carDef, trackDef, null);
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
                SeedMinimalOpenTrack(trackDef);
                TrackSimulation sim = new TrackSimulationFactory().Create(carDef, trackDef, null);
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
                SeedMinimalOpenTrack(trackDef);
                TrackSimulation sim = new TrackSimulationFactory().Create(carDef, trackDef, null);
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
