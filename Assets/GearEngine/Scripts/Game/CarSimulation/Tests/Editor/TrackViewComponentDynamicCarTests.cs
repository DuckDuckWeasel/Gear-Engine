using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Presentation;
using GearEngine.CarSimulation.Simulation;
using GearEngine.CarSimulation.Tracks;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;
using Object = UnityEngine.Object;

namespace GearEngine.CarSimulation.Tests
{
    public sealed class TrackViewComponentDynamicCarTests
    {
        [Test]
        public void Bind_WithTrackViewModel_InitializesSplineFromTrackDefinition()
        {
            var trackGo = new GameObject("TrackHarnessBind");
            try
            {
                trackGo.AddComponent<SplineContainer>();
                TrackViewComponent track = trackGo.AddComponent<TrackViewComponent>();
                CarDefinition carDef = ScriptableObject.CreateInstance<CarDefinition>();
                TrackDefinition trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
                try
                {
                    SeedOpenSpline(trackDef);

                    var carRunnerConfig = ScriptableObject.CreateInstance<SplineCarRunnerConfigSO>();
                    var carRunner = new SplineCarRunnerService(carRunnerConfig);
                    var raceManager = new RaceManagerService(carRunner);
                    var factory = new TrackSimulationFactory();

                    RaceState session = factory.Create(carDef, trackDef, null);
                    raceManager.RegisterRace(session);
                    var trackVm = new TrackViewModel(session, raceManager, carRunner, factory);
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
