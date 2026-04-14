using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Drivers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.CarSimulation.Tests
{
    public sealed class CarEntityAndDriverTests
    {

        [Test]
        public void CarSplineDriver_Bind_DoesNotThrowBeforeUnityStart()
        {
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();

            var go = new GameObject("DriverBindTest");
            try
            {
                trackDef.Spline.Knots = new[]
                {
                    new BezierKnot(new Vector3(0f, 0f, 0f)),
                    new BezierKnot(new Vector3(20f, 0f, 0f)),
                };
                trackDef.Spline.Closed = false;

                var driver = go.AddComponent<CarSplineDriver>();
                var container = go.AddComponent<SplineContainer>();
                TrackSimulation simulation = new TrackSimulationFactory().Create(carDef, trackDef, null);

                Assert.DoesNotThrow(() => driver.Bind(simulation, container));
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
