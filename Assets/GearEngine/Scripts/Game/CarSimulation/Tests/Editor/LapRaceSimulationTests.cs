using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using NUnit.Framework;
using Scaffold.Entities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.CarSimulation.Tests
{
    public sealed class LapRaceSimulationTests
    {
        private static void ConfigureVariableAsFloat(VariableSO variable)
        {
            var so = new SerializedObject(variable);
            so.FindProperty("valueType").enumValueIndex = (int)VariableValueType.Float;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SeedCarDefinitionWithSpeedVariable(CarDefinition carDef, VariableSO speedVar)
        {
            var so = new SerializedObject(carDef);
            SerializedProperty bagProp = so.FindProperty("bag");
            SerializedProperty entries = bagProp.FindPropertyRelative("entries");
            entries.arraySize = 1;
            SerializedProperty e0 = entries.GetArrayElementAtIndex(0);
            e0.FindPropertyRelative("variable").objectReferenceValue = speedVar;
            e0.FindPropertyRelative("baseValue").managedReferenceValue = new FloatVariableValue { Value = 0f };
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        [Test]
        public void ClosedTrack_RecordsLapSplitAfterCrossingStartLine()
        {
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            try
            {
                trackDef.Spline.Knots = new[]
                {
                    new BezierKnot(new Vector3(0f, 0f, 0f)),
                    new BezierKnot(new Vector3(20f, 0f, 0f)),
                    new BezierKnot(new Vector3(20f, 0f, 20f)),
                    new BezierKnot(new Vector3(0f, 0f, 20f)),
                };
                trackDef.Spline.Closed = true;

                var cfg = new RaceSessionConfig();
                cfg.SetTotalLapsForTests(5);

                LapRaceSession session = new TrackSimulationFactory().Create(carDef, trackDef, cfg);
                var go = new GameObject("SplineHost");
                try
                {
                    SplineContainer container = go.AddComponent<SplineContainer>();
                    container.Spline.Knots = trackDef.Spline.Knots;
                    container.Spline.Closed = true;

                    session.BindSpline(container);
                    session.SetClockRunning(true);

                    int guard = 0;
                    while (session.LapTimes.Count == 0 && guard++ < 200000)
                    {
                        session.Tick(0.02f);
                    }

                    Assert.That(session.LapTimes.Count, Is.GreaterThan(0));
                    Assert.That(session.LapTimes[0], Is.GreaterThan(0f));
                }
                finally
                {
                    Object.DestroyImmediate(go);
                }
            }
            finally
            {
                Object.DestroyImmediate(trackDef);
                Object.DestroyImmediate(carDef);
            }
        }

        [Test]
        public void NegativeTotalLaps_DoesNotFinishFromLapCount()
        {
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            try
            {
                trackDef.Spline.Knots = new[]
                {
                    new BezierKnot(new Vector3(0f, 0f, 0f)),
                    new BezierKnot(new Vector3(20f, 0f, 0f)),
                    new BezierKnot(new Vector3(20f, 0f, 20f)),
                    new BezierKnot(new Vector3(0f, 0f, 20f)),
                };
                trackDef.Spline.Closed = true;

                var cfg = new RaceSessionConfig();
                cfg.SetTotalLapsForTests(-1);

                LapRaceSession session = new TrackSimulationFactory().Create(carDef, trackDef, cfg);
                var go = new GameObject("SplineHost");
                try
                {
                    SplineContainer container = go.AddComponent<SplineContainer>();
                    container.Spline.Knots = trackDef.Spline.Knots;
                    container.Spline.Closed = true;

                    session.BindSpline(container);
                    session.SetClockRunning(true);

                    for (int i = 0; i < 8000; i++)
                    {
                        session.Tick(0.02f);
                    }

                    Assert.That(session.Phase, Is.EqualTo(SimulationLifecycleState.Running));
                    Assert.That(session.CurrentLap, Is.GreaterThan(5));
                }
                finally
                {
                    Object.DestroyImmediate(go);
                }
            }
            finally
            {
                Object.DestroyImmediate(trackDef);
                Object.DestroyImmediate(carDef);
            }
        }

        [Test]
        public void HigherSpeedOnCar_IncreasesProgressOverSameTicks()
        {
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            VariableSO speedVar = ScriptableObject.CreateInstance<VariableSO>();
            ConfigureVariableAsFloat(speedVar);
            var vars = ScriptableObject.CreateInstance<CarVariableSet>();
            try
            {
                trackDef.Spline.Knots = new[]
                {
                    new BezierKnot(Vector3.zero),
                    new BezierKnot(Vector3.right * 500f),
                };
                trackDef.Spline.Closed = false;

                vars.AssignVariablesForTests(speedVar);
                SeedCarDefinitionWithSpeedVariable(carDef, speedVar);

                float RunWithSpeed(float speed)
                {
                    var cfg = new RaceSessionConfig();
                    cfg.SetVariablesForTests(vars);
                    var go = new GameObject("OpenSpline");
                    try
                    {
                        LapRaceSession s = new TrackSimulationFactory().Create(carDef, trackDef, cfg);
                        s.Car.AddModifier(new EntityModifierEntry(speedVar, new FloatVariableValue { Value = speed }));
                        SplineContainer container = go.AddComponent<SplineContainer>();
                        container.Spline.Knots = trackDef.Spline.Knots;
                        container.Spline.Closed = false;
                        s.BindSpline(container);
                        s.SetClockRunning(true);
                        for (int i = 0; i < 60; i++)
                        {
                            s.Tick(0.05f);
                        }

                        return s.ProgressDistance;
                    }
                    finally
                    {
                        Object.DestroyImmediate(go);
                    }
                }

                float slow = RunWithSpeed(20f);
                float fast = RunWithSpeed(80f);
                Assert.That(fast, Is.GreaterThan(slow));
            }
            finally
            {
                Object.DestroyImmediate(trackDef);
                Object.DestroyImmediate(carDef);
                Object.DestroyImmediate(speedVar);
                Object.DestroyImmediate(vars);
            }
        }
    }
}
