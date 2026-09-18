using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.PhysicsSimulation;
using GearEngine.CarSimulation.Presentation;
using GearEngine.CarSimulation.Simulation;
using GearEngine.CarSimulation.Tracks;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Splines;
using Object = UnityEngine.Object;

namespace GearEngine.CarSimulation.Tests
{
    public sealed class TrackViewComponentDynamicCarTests
    {
        [TestCase(typeof(SplinePropGenerator.All))]
        [TestCase(typeof(SplinePropGenerator.Curves))]
        [TestCase(typeof(SplinePropGenerator.Straights))]
        [TestCase(typeof(SplinePropGenerator.Inside))]
        [TestCase(typeof(SplinePropGenerator.Outside))]
        public void PropRuleType_IsPreservedForPlayerSerialization(System.Type ruleType)
        {
            Assert.That(
                System.Attribute.IsDefined(ruleType, typeof(PreserveAttribute)),
                Is.True,
                $"{ruleType.Name} must survive IL2CPP stripping for SerializeReference theme rules.");
        }

        [Test]
        public void Bind_WithTrackViewModel_InitializesSplineFromTrackDefinition()
        {
            GameObject trackGo = new GameObject("TrackHarnessBind");
            try
            {
                trackGo.AddComponent<SplineContainer>();
                TrackViewComponent track = trackGo.AddComponent<TrackViewComponent>();
                CarDefinition carDef = ScriptableObject.CreateInstance<CarDefinition>();
                TrackDefinition trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
                try
                {
                    SeedOpenSpline(trackDef);

                    PhysicsSimulationConfig carRunnerConfig = ScriptableObject.CreateInstance<PhysicsSimulationConfig>();
                    SplineCarRunnerService carRunner = new SplineCarRunnerService(carRunnerConfig);
                    RaceManagerService raceManager = new RaceManagerService(carRunner);
                    TrackSimulationFactory factory = new TrackSimulationFactory();

                    RaceState session = factory.Create(carDef, trackDef, null);
                    raceManager.RegisterRace(session);
                    TrackViewModel trackVm = new TrackViewModel(session, raceManager, carRunner, factory);
                    track.Bind(trackVm);

                    Assert.That(trackGo.GetComponent<SplineContainer>().Spline.Count, Is.GreaterThan(0));
                    track.Unbind();

                    Object.DestroyImmediate(carRunnerConfig);
                }
                finally
                {
                    Object.DestroyImmediate(carDef);
                    Object.DestroyImmediate(trackDef);
                }
            }
            finally
            {
                Object.DestroyImmediate(trackGo);
            }
        }

        private static void SeedOpenSpline(TrackDefinition trackDef)
        {
            trackDef.Spline.Knots = new[] { new BezierKnot(Vector3.zero), new BezierKnot(Vector3.right * 10f) };
            trackDef.Spline.Closed = false;
        }
    }
}
