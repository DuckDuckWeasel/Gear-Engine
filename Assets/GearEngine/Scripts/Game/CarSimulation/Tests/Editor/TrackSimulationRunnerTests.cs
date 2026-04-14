using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Entity;
using GearEngine.CarSimulation.Simulation;
using GearEngine.CarSimulation.Track;
using NUnit.Framework;
using Scaffold.Entities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.CarSimulation.Tests
{
    public sealed class TrackSimulationRunnerTests
    {
        [Test]
        public void Step_WhenRunning_AdvancesDistanceAndRaceTime()
        {
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            var speed = ScriptableObject.CreateInstance<VariableSO>();
            var speedSo = new UnityEditor.SerializedObject(speed);
            speedSo.FindProperty("valueType").enumValueIndex = (int)VariableValueType.Float;
            speedSo.ApplyModifiedPropertiesWithoutUndo();

            CarVariableSet carVars = null;
            try
            {
                var defSo = new UnityEditor.SerializedObject(carDef);
                defSo.FindProperty("carPrefab").objectReferenceValue = null;
                SerializedProperty bagProp = defSo.FindProperty("bag");
                SerializedProperty entries = bagProp.FindPropertyRelative("entries");
                entries.arraySize = 1;
                SerializedProperty e0 = entries.GetArrayElementAtIndex(0);
                e0.FindPropertyRelative("variable").objectReferenceValue = speed;
                e0.FindPropertyRelative("baseValue").managedReferenceValue = new FloatVariableValue { Value = 40f };
                defSo.ApplyModifiedPropertiesWithoutUndo();

                trackDef.Spline.Knots = new[]
                {
                    new BezierKnot(new Vector3(0f, 0f, 0f)),
                    new BezierKnot(new Vector3(200f, 0f, 0f)),
                };
                trackDef.Spline.Closed = false;

                carVars = ScriptableObject.CreateInstance<CarVariableSet>();
                var cvSo = new SerializedObject(carVars);
                cvSo.FindProperty("speed").objectReferenceValue = speed;
                cvSo.ApplyModifiedPropertiesWithoutUndo();

                CarEntity car = new CarEntityFactory().Create(carDef);
                BakedTrackProfile profile = TrackProfileBaker.Bake(trackDef.Spline);
                var simulation = new TrackSimulation(trackDef, car, profile, carVars);
                simulation.Toggle(true);

                var runner = new TrackSimulationRunner();
                runner.SetSimulation(simulation);
                float before = simulation.Motion.Distance;
                runner.Step(0.2f);

                Assert.That(simulation.Motion.Distance, Is.GreaterThan(before));
                Assert.That(simulation.Race.CurrentTime, Is.GreaterThan(0f));
            }
            finally
            {
                if (carVars != null)
                {
                    Object.DestroyImmediate(carVars);
                }

                Object.DestroyImmediate(speed);
                Object.DestroyImmediate(trackDef);
                Object.DestroyImmediate(carDef);
            }
        }

        [Test]
        public void Step_WhenNotRunning_DoesNotAdvance()
        {
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            try
            {
                trackDef.Spline.Knots = new[]
                {
                    new BezierKnot(new Vector3(0f, 0f, 0f)),
                    new BezierKnot(new Vector3(50f, 0f, 0f)),
                };
                trackDef.Spline.Closed = false;

                TrackSimulation simulation = new TrackSimulationFactory().Create(carDef, trackDef, null);
                var runner = new TrackSimulationRunner();
                runner.SetSimulation(simulation);
                runner.Step(0.5f);
                Assert.That(simulation.Motion.Distance, Is.EqualTo(0f));
            }
            finally
            {
                Object.DestroyImmediate(trackDef);
                Object.DestroyImmediate(carDef);
            }
        }
    }
}
