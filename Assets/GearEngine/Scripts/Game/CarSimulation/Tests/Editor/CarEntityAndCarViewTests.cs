using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.CarSimulation.Tests
{
    public sealed class CarEntityAndCarViewTests
    {
        [Test]
        public void CarView_Initialize_DoesNotThrowBeforeUnityStart()
        {
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();

            var go = new GameObject("CarViewBindTest");
            try
            {
                trackDef.Spline.Knots = new[]
                {
                    new BezierKnot(new Vector3(0f, 0f, 0f)),
                    new BezierKnot(new Vector3(20f, 0f, 0f)),
                };
                trackDef.Spline.Closed = false;

                var carView = go.AddComponent<CarView>();
                var container = go.AddComponent<SplineContainer>();
                LapRaceSession session = new TrackSimulationFactory().Create(carDef, trackDef, null);

                Assert.DoesNotThrow(() => carView.Initialize(session.Car, container, session));
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(trackDef);
                Object.DestroyImmediate(carDef);
            }
        }
    }
}
