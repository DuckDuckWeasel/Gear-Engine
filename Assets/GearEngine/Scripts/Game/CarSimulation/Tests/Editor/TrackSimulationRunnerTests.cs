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
        public void Step_WhenRunning_AdvancesDistanceAndRaceTime()
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

                var tuning = ScriptableObject.CreateInstance<TrackSimulationTuning>();
                toDestroy.Add(tuning);

                CarEntity car = new CarEntityFactory().Create(carDef);
                BakedTrackProfile profile = TrackProfileBaker.Bake(trackDef.Spline);
                var simulation = new TrackSimulation(trackDef, car, profile, carVars, tuning);
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
