using System.Collections.Generic;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Entity;
using GearEngine.CarSimulation.Simulation;
using GearEngine.CarSimulation.Tracks;
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
        public void Step_WhenRunning_AdvancesRaceTimeAndDistanceTravelled()
        {
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            var toDestroy = new List<Object>();
            try
            {
                trackDef.Spline.Knots = new[]
                {
                    new BezierKnot(new Vector3(0f, 0f, 0f)),
                    new BezierKnot(new Vector3(200f, 0f, 0f)),
                };
                trackDef.Spline.Closed = false;

                VariableSO MakeFloatVar()
                {
                    var v = ScriptableObject.CreateInstance<VariableSO>();
                    var vSo = new SerializedObject(v);
                    vSo.FindProperty("valueType").enumValueIndex = (int)VariableValueType.Float;
                    vSo.ApplyModifiedPropertiesWithoutUndo();
                    toDestroy.Add(v);
                    return v;
                }

                VariableSO speedVar = MakeFloatVar();
                VariableSO accelerationVar = MakeFloatVar();
                VariableSO brakeVar = MakeFloatVar();
                VariableSO handlingVar = MakeFloatVar();
                VariableSO stabilityVar = MakeFloatVar();
                VariableSO recoveryVar = MakeFloatVar();
                VariableSO driftPenaltyVar = MakeFloatVar();

                var carDef = ScriptableObject.CreateInstance<CarDefinition>();
                toDestroy.Add(carDef);
                var defSo = new SerializedObject(carDef);
                defSo.FindProperty("carPrefab").objectReferenceValue = null;
                SerializedProperty entries = defSo.FindProperty("bag").FindPropertyRelative("entries");
                entries.arraySize = 7;
                void SetBagEntry(int index, VariableSO variable, float value)
                {
                    SerializedProperty e = entries.GetArrayElementAtIndex(index);
                    e.FindPropertyRelative("variable").objectReferenceValue = variable;
                    e.FindPropertyRelative("baseValue").managedReferenceValue = new FloatVariableValue { Value = value };
                }

                SetBagEntry(0, speedVar, 40f);
                SetBagEntry(1, accelerationVar, 12f);
                SetBagEntry(2, brakeVar, 22f);
                SetBagEntry(3, handlingVar, 48f);
                SetBagEntry(4, stabilityVar, 1.1f);
                SetBagEntry(5, recoveryVar, 0.85f);
                SetBagEntry(6, driftPenaltyVar, 0.15f);
                defSo.ApplyModifiedPropertiesWithoutUndo();

                var carVars = ScriptableObject.CreateInstance<CarVariableSet>();
                toDestroy.Add(carVars);
                var cvSo = new SerializedObject(carVars);
                cvSo.FindProperty("speed").objectReferenceValue = speedVar;
                cvSo.FindProperty("acceleration").objectReferenceValue = accelerationVar;
                cvSo.FindProperty("brake").objectReferenceValue = brakeVar;
                cvSo.FindProperty("handling").objectReferenceValue = handlingVar;
                cvSo.FindProperty("stability").objectReferenceValue = stabilityVar;
                cvSo.FindProperty("recovery").objectReferenceValue = recoveryVar;
                cvSo.FindProperty("driftPenalty").objectReferenceValue = driftPenaltyVar;
                cvSo.ApplyModifiedPropertiesWithoutUndo();

                var config = new TrackSimulationConfig();
                CarEntity car = new CarEntityFactory().Create(carDef);
                SplineWaypointPath path = SplineWaypointPath.Build(trackDef.Spline, config.Driver.WaypointSpacingMetres);
                var simulation = new TrackSimulation(trackDef, car, path, carVars, config.Driver);
                var trackGo = new GameObject("TrackRoot");
                toDestroy.Add(trackGo);
                trackGo.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                simulation.AttachTrackRoot(trackGo.transform);
                simulation.SeedMotionFromTrack();
                simulation.Toggle(true);

                var runner = new TrackSimulationRunner();
                runner.SetSimulation(simulation);
                float beforeTravel = simulation.Race.DistanceTravelled;
                runner.Step(0.2f);

                Assert.That(simulation.Race.DistanceTravelled, Is.GreaterThan(beforeTravel));
                Assert.That(simulation.Race.CurrentTime, Is.GreaterThan(0f));
            }
            finally
            {
                foreach (Object o in toDestroy)
                {
                    if (o != null)
                    {
                        Object.DestroyImmediate(o);
                    }
                }

                Object.DestroyImmediate(trackDef);
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
                var trackGo = new GameObject("TrackRoot");
                try
                {
                    simulation.AttachTrackRoot(trackGo.transform);
                    simulation.SeedMotionFromTrack();
                    var runner = new TrackSimulationRunner();
                    runner.SetSimulation(simulation);
                    float travelBefore = simulation.Race.DistanceTravelled;
                    runner.Step(0.5f);
                    Assert.That(simulation.Race.DistanceTravelled, Is.EqualTo(travelBefore));
                }
                finally
                {
                    Object.DestroyImmediate(trackGo);
                }
            }
            finally
            {
                Object.DestroyImmediate(trackDef);
                Object.DestroyImmediate(carDef);
            }
        }

        [Test]
        public void AdvanceWaypointIndex_IncrementsWhenInsideCaptureRadius()
        {
            var spline = new Spline();
            spline.Knots = new[]
            {
                new BezierKnot(new Vector3(0f, 0f, 0f)),
                new BezierKnot(new Vector3(10f, 0f, 0f)),
                new BezierKnot(new Vector3(20f, 0f, 0f)),
            };
            spline.Closed = false;

            SplineWaypointPath path = SplineWaypointPath.Build(spline, spacingMetres: 2f);
            var root = new GameObject("T").transform;
            root.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            try
            {
                Vector3 nearSecond = path.GetWorldPoint(1, root) + Vector3.left * 0.1f;
                int next = SimpleWaypointDriver.AdvanceWaypointIndex(
                    path,
                    0,
                    nearSecond,
                    root,
                    captureRadius: 5f,
                    closed: false);
                Assert.That(next, Is.GreaterThan(0));
            }
            finally
            {
                Object.DestroyImmediate(root.gameObject);
            }
        }
    }
}
