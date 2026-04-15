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

                VariableSO maxStraightVar = MakeFloatVar();
                VariableSO maxCurveVar = MakeFloatVar();
                VariableSO accelerationVar = MakeFloatVar();
                VariableSO brakeVar = MakeFloatVar();
                VariableSO handlingVar = MakeFloatVar();

                var carDef = ScriptableObject.CreateInstance<CarDefinition>();
                toDestroy.Add(carDef);
                var defSo = new SerializedObject(carDef);
                defSo.FindProperty("carPrefab").objectReferenceValue = null;
                SerializedProperty entries = defSo.FindProperty("bag").FindPropertyRelative("entries");
                entries.arraySize = 5;
                void SetBagEntry(int index, VariableSO variable, float value)
                {
                    SerializedProperty e = entries.GetArrayElementAtIndex(index);
                    e.FindPropertyRelative("variable").objectReferenceValue = variable;
                    e.FindPropertyRelative("baseValue").managedReferenceValue = new FloatVariableValue { Value = value };
                }

                SetBagEntry(0, maxStraightVar, 40f);
                SetBagEntry(1, accelerationVar, 12f);
                SetBagEntry(2, brakeVar, 22f);
                SetBagEntry(3, handlingVar, 48f);
                SetBagEntry(4, maxCurveVar, 18f);
                defSo.ApplyModifiedPropertiesWithoutUndo();

                var carVars = ScriptableObject.CreateInstance<CarVariableSet>();
                toDestroy.Add(carVars);
                carVars.AssignVariablesForTests(maxStraightVar, maxCurveVar, accelerationVar, brakeVar, handlingVar);

                var tuning = ScriptableObject.CreateInstance<TrackSimulationTuning>();
                toDestroy.Add(tuning);

                CarEntity car = new CarEntityFactory().Create(carDef);
                BakedTrackProfile profile = TrackProfileBaker.Bake(trackDef.Spline);
                var simulation = new TrackSimulation(trackDef, car, profile, carVars, tuning);
                simulation.Toggle(true);

                var runner = new TrackSimulationRunner(new UnityRaceRandom());
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
        public void SimulationFrame_Create_ReadsMaxStraightAndMaxCurveFromCar()
        {
            CarDefinition def = AssetDatabase.LoadAssetAtPath<CarDefinition>("Assets/GearEngine/Data/Cars/CarDefinition.asset");
            CarVariableSet vars = AssetDatabase.LoadAssetAtPath<CarVariableSet>("Assets/GearEngine/Data/Cars/CarVariableSet.asset");
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            var tuning = ScriptableObject.CreateInstance<TrackSimulationTuning>();
            try
            {
                trackDef.Spline.Knots = new[]
                {
                    new BezierKnot(new Vector3(0f, 0f, 0f)),
                    new BezierKnot(new Vector3(100f, 0f, 0f)),
                };
                trackDef.Spline.Closed = false;

                CarEntity car = new CarEntityFactory().Create(def);
                BakedTrackProfile profile = TrackProfileBaker.Bake(trackDef.Spline);
                var simulation = new TrackSimulation(trackDef, car, profile, vars, tuning);
                simulation.Toggle(true);

                float expectedStraight = car.GetValue<float>(vars.MaxStraightSpeed);
                float expectedCurve = car.GetValue<float>(vars.MaxCurveSpeed);
                SimulationFrame frame = SimulationFrame.Create(simulation, 0.05f);
                Assert.That(frame.MaxStraightSpeed, Is.EqualTo(expectedStraight).Within(1e-5));
                Assert.That(frame.MaxCurveSpeed, Is.EqualTo(expectedCurve).Within(1e-5));
            }
            finally
            {
                Object.DestroyImmediate(tuning);
                Object.DestroyImmediate(trackDef);
            }
        }

        [Test]
        public void Step_OnStraightLine_ApproachesMaxStraightSpeed()
        {
            var toDestroy = new List<Object>();
            try
            {
                (CarEntity car, CarVariableSet carVars, TrackDefinition trackDef) = BuildCarAndVars(toDestroy, maxStraight: 40f, maxCurve: 40f, acceleration: 80f, brake: 80f, handling: 25f);
                BakedTrackProfile profile = TrackProfileBaker.Bake(trackDef.Spline);
                var tuning = ScriptableObject.CreateInstance<TrackSimulationTuning>();
                toDestroy.Add(tuning);
                var simulation = new TrackSimulation(trackDef, car, profile, carVars, tuning);
                simulation.Toggle(true);
                var runner = new TrackSimulationRunner(new UnityRaceRandom());
                runner.SetSimulation(simulation);
                for (int i = 0; i < 80; i++)
                {
                    runner.Step(0.05f);
                }

                Assert.That(simulation.Motion.Speed, Is.EqualTo(40f).Within(0.75));
            }
            finally
            {
                foreach (Object o in toDestroy)
                {
                    Object.DestroyImmediate(o);
                }
            }
        }

        [Test]
        public void Step_OnTightTurn_SpeedStaysCloserToMaxCurveThanMaxStraight()
        {
            var toDestroy = new List<Object>();
            try
            {
                var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
                toDestroy.Add(trackDef);
                trackDef.Spline.Knots = new[]
                {
                    new BezierKnot(new Vector3(0f, 0f, 0f)),
                    new BezierKnot(new Vector3(5f, 0f, 0f)),
                    new BezierKnot(new Vector3(5f, 5f, 0f)),
                };
                trackDef.Spline.Closed = false;

                (CarEntity car, CarVariableSet carVars, _) = BuildCarAndVars(toDestroy, maxStraight: 50f, maxCurve: 8f, acceleration: 200f, brake: 200f, handling: 40f, trackOverride: trackDef);
                BakedTrackProfile profile = TrackProfileBaker.Bake(trackDef.Spline);
                var tuning = ScriptableObject.CreateInstance<TrackSimulationTuning>();
                toDestroy.Add(tuning);
                SerializedObject tSo = new SerializedObject(tuning);
                tSo.FindProperty("activeCapCurvatureSpan").floatValue = 0.02f;
                tSo.ApplyModifiedPropertiesWithoutUndo();

                var simulation = new TrackSimulation(trackDef, car, profile, carVars, tuning);
                simulation.Toggle(true);
                simulation.Motion.Distance = profile.TotalLength * 0.45f;
                var runner = new TrackSimulationRunner(new UnityRaceRandom());
                runner.SetSimulation(simulation);
                for (int i = 0; i < 120; i++)
                {
                    runner.Step(0.05f);
                }

                Assert.That(simulation.Motion.Speed, Is.LessThan(35f));
            }
            finally
            {
                foreach (Object o in toDestroy)
                {
                    Object.DestroyImmediate(o);
                }
            }
        }

        [Test]
        public void LineError_HigherHandling_ReducesLineError_AfterShortCorner()
        {
            float wide = RunLineErrorAtEnd(5f);
            float narrow = RunLineErrorAtEnd(95f);
            Assert.That(narrow, Is.LessThanOrEqualTo(wide));
        }

        [Test]
        public void LineError_Decays_WhenNoLongerStressed()
        {
            float afterStress = RunLineErrorAtEnd(4f);
            Assert.That(afterStress, Is.GreaterThan(0.05f));

            var toDestroy = new List<Object>();
            try
            {
                (CarEntity car, CarVariableSet carVars, TrackDefinition trackDef) = BuildCarAndVars(toDestroy, maxStraight: 40f, maxCurve: 10f, acceleration: 5f, brake: 5f, handling: 4f);
                BakedTrackProfile profile = TrackProfileBaker.Bake(trackDef.Spline);
                var tuning = ScriptableObject.CreateInstance<TrackSimulationTuning>();
                toDestroy.Add(tuning);
                SerializedObject tSo = new SerializedObject(tuning);
                tSo.FindProperty("lineErrorDecayRate").floatValue = 2f;
                tSo.ApplyModifiedPropertiesWithoutUndo();

                var simulation = new TrackSimulation(trackDef, car, profile, carVars, tuning);
                simulation.Toggle(true);
                simulation.Motion.Speed = 10f;
                simulation.Motion.LineError = 0.9f;
                var runner = new TrackSimulationRunner(new UnityRaceRandom());
                runner.SetSimulation(simulation);
                for (int i = 0; i < 40; i++)
                {
                    runner.Step(0.05f);
                }

                Assert.That(simulation.Motion.LineError, Is.LessThan(0.4f));
            }
            finally
            {
                foreach (Object o in toDestroy)
                {
                    Object.DestroyImmediate(o);
                }
            }
        }

        [Test]
        public void AdvanceRace_ReducesEffectiveSpeed_WhenLineErrorNonZero()
        {
            var toDestroy = new List<Object>();
            try
            {
                (CarEntity car, CarVariableSet carVars, TrackDefinition trackDef) = BuildCarAndVars(toDestroy, maxStraight: 30f, maxCurve: 10f, acceleration: 0f, brake: 0f, handling: 50f);
                BakedTrackProfile profile = TrackProfileBaker.Bake(trackDef.Spline);
                var tuning = ScriptableObject.CreateInstance<TrackSimulationTuning>();
                toDestroy.Add(tuning);
                SerializedObject tSo = new SerializedObject(tuning);
                tSo.FindProperty("overshootPenaltyScale").floatValue = 0.5f;
                tSo.ApplyModifiedPropertiesWithoutUndo();

                var simulation = new TrackSimulation(trackDef, car, profile, carVars, tuning);
                simulation.Toggle(true);
                simulation.Motion.Speed = 20f;
                simulation.Motion.LineError = 0.5f;
                var runner = new TrackSimulationRunner(new UnityRaceRandom());
                runner.SetSimulation(simulation);
                runner.Step(0.1f);
                Assert.That(simulation.Race.CurrentSpeed, Is.LessThan(simulation.Motion.Speed));
            }
            finally
            {
                foreach (Object o in toDestroy)
                {
                    Object.DestroyImmediate(o);
                }
            }
        }

        [Test]
        public void LineError_BuildsFromHandlingAlone_WhenCurveAndStraightSpeedAreEqual()
        {
            var toDestroy = new List<Object>();
            try
            {
                var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
                toDestroy.Add(trackDef);
                trackDef.Spline.Knots = new[]
                {
                    new BezierKnot(new Vector3(0f, 0f, 0f)),
                    new BezierKnot(new Vector3(4f, 0f, 0f)),
                    new BezierKnot(new Vector3(4f, 4f, 0f)),
                };
                trackDef.Spline.Closed = false;

                (CarEntity car, CarVariableSet carVars, _) = BuildCarAndVars(toDestroy, maxStraight: 50f, maxCurve: 50f, acceleration: 0f, brake: 0f, handling: 10f, trackOverride: trackDef);
                BakedTrackProfile profile = TrackProfileBaker.Bake(trackDef.Spline);
                var tuning = ScriptableObject.CreateInstance<TrackSimulationTuning>();
                toDestroy.Add(tuning);
                SerializedObject tSo = new SerializedObject(tuning);
                tSo.FindProperty("activeCapCurvatureSpan").floatValue = 0.02f;
                tSo.FindProperty("maxAbsorbableDifficulty").floatValue = 0.2f;
                tSo.ApplyModifiedPropertiesWithoutUndo();

                var simulation = new TrackSimulation(trackDef, car, profile, carVars, tuning);
                simulation.Toggle(true);
                simulation.Motion.Distance = profile.TotalLength * 0.48f;
                simulation.Motion.Speed = 35f;
                var runner = new TrackSimulationRunner(new UnityRaceRandom());
                runner.SetSimulation(simulation);
                for (int i = 0; i < 20; i++)
                {
                    runner.Step(0.05f);
                }

                Assert.That(simulation.Motion.LineError, Is.GreaterThan(0.02f));
            }
            finally
            {
                foreach (Object o in toDestroy)
                {
                    Object.DestroyImmediate(o);
                }
            }
        }

        [Test]
        public void LineError_DoesNotBuild_WhenHandlingAbsorbsAllDifficulty()
        {
            var toDestroy = new List<Object>();
            try
            {
                (CarEntity car, CarVariableSet carVars, TrackDefinition trackDef) = BuildCarAndVars(toDestroy, maxStraight: 40f, maxCurve: 40f, acceleration: 0f, brake: 0f, handling: 95f);
                BakedTrackProfile profile = TrackProfileBaker.Bake(trackDef.Spline);
                var tuning = ScriptableObject.CreateInstance<TrackSimulationTuning>();
                toDestroy.Add(tuning);
                SerializedObject tSo = new SerializedObject(tuning);
                tSo.FindProperty("activeCapCurvatureSpan").floatValue = 200f;
                tSo.FindProperty("maxAbsorbableDifficulty").floatValue = 1f;
                tSo.ApplyModifiedPropertiesWithoutUndo();

                var simulation = new TrackSimulation(trackDef, car, profile, carVars, tuning);
                simulation.Toggle(true);
                simulation.Motion.Distance = profile.TotalLength * 0.5f;
                simulation.Motion.Speed = 25f;
                var runner = new TrackSimulationRunner(new UnityRaceRandom());
                runner.SetSimulation(simulation);
                for (int i = 0; i < 30; i++)
                {
                    runner.Step(0.05f);
                }

                Assert.That(simulation.Motion.LineError, Is.LessThanOrEqualTo(0.001f));
            }
            finally
            {
                foreach (Object o in toDestroy)
                {
                    Object.DestroyImmediate(o);
                }
            }
        }

        [Test]
        public void Step_AccelerationModifier_DoesNotExceedActiveCap()
        {
            var toDestroy = new List<Object>();
            try
            {
                (CarEntity car, CarVariableSet carVars, TrackDefinition trackDef) = BuildCarAndVars(toDestroy, maxStraight: 25f, maxCurve: 25f, acceleration: 4f, brake: 20f, handling: 30f);
                VariableSO accelVar = SerializedObjectFromCarVars(carVars);
                BakedTrackProfile profile = TrackProfileBaker.Bake(trackDef.Spline);
                var tuning = ScriptableObject.CreateInstance<TrackSimulationTuning>();
                toDestroy.Add(tuning);
                SerializedObject tuneSo = new SerializedObject(tuning);
                tuneSo.FindProperty("activeCapCurvatureSpan").floatValue = 200f;
                tuneSo.ApplyModifiedPropertiesWithoutUndo();
                var simulation = new TrackSimulation(trackDef, car, profile, carVars, tuning);
                simulation.Toggle(true);
                car.AddModifier(new EntityModifierEntry(accelVar, new FloatVariableValue { Value = 80f }));
                var runner = new TrackSimulationRunner(new UnityRaceRandom());
                runner.SetSimulation(simulation);
                for (int i = 0; i < 80; i++)
                {
                    runner.Step(0.05f);
                }

                float cap = car.GetValue<float>(carVars.MaxStraightSpeed);
                Assert.That(simulation.Motion.Speed, Is.LessThanOrEqualTo(cap + 0.02f));
            }
            finally
            {
                foreach (Object o in toDestroy)
                {
                    Object.DestroyImmediate(o);
                }
            }
        }

        [Test]
        public void CarDefinition_WithRemodeledVariableBag_CreatesEntity()
        {
            CarDefinition def = AssetDatabase.LoadAssetAtPath<CarDefinition>("Assets/GearEngine/Data/Cars/CarDefinition.asset");
            CarVariableSet vars = AssetDatabase.LoadAssetAtPath<CarVariableSet>("Assets/GearEngine/Data/Cars/CarVariableSet.asset");
            Assert.That(def, Is.Not.Null);
            Assert.That(vars, Is.Not.Null);
            CarEntity car = null;
            Assert.DoesNotThrow(() => car = new CarEntityFactory().Create(def));
            Assert.That(car.GetValue<float>(vars.MaxStraightSpeed), Is.GreaterThan(0f));
            Assert.That(car.GetValue<float>(vars.MaxCurveSpeed), Is.GreaterThan(0f));
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
                var runner = new TrackSimulationRunner(new UnityRaceRandom());
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

        private static float RunLineErrorAtEnd(float handling)
        {
            var toDestroy = new List<Object>();
            try
            {
                var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
                toDestroy.Add(trackDef);
                trackDef.Spline.Knots = new[]
                {
                    new BezierKnot(new Vector3(0f, 0f, 0f)),
                    new BezierKnot(new Vector3(4f, 0f, 0f)),
                    new BezierKnot(new Vector3(4f, 4f, 0f)),
                };
                trackDef.Spline.Closed = false;

                (CarEntity car, CarVariableSet carVars, _) = BuildCarAndVars(toDestroy, maxStraight: 60f, maxCurve: 6f, acceleration: 0f, brake: 0f, handling: handling, trackOverride: trackDef);
                BakedTrackProfile profile = TrackProfileBaker.Bake(trackDef.Spline);
                var tuning = ScriptableObject.CreateInstance<TrackSimulationTuning>();
                toDestroy.Add(tuning);
                SerializedObject tSo = new SerializedObject(tuning);
                tSo.FindProperty("activeCapCurvatureSpan").floatValue = 0.02f;
                tSo.ApplyModifiedPropertiesWithoutUndo();

                var simulation = new TrackSimulation(trackDef, car, profile, carVars, tuning);
                simulation.Toggle(true);
                simulation.Motion.Distance = profile.TotalLength * 0.48f;
                simulation.Motion.Speed = 22f;
                var runner = new TrackSimulationRunner(new UnityRaceRandom());
                runner.SetSimulation(simulation);
                for (int i = 0; i < 6; i++)
                {
                    runner.Step(0.05f);
                }

                return simulation.Motion.LineError;
            }
            finally
            {
                foreach (Object o in toDestroy)
                {
                    Object.DestroyImmediate(o);
                }
            }
        }

        private static VariableSO SerializedObjectFromCarVars(CarVariableSet carVars)
        {
            return carVars.Acceleration;
        }

        private static (CarEntity car, CarVariableSet carVars, TrackDefinition trackDef) BuildCarAndVars(List<Object> toDestroy, float maxStraight, float maxCurve, float acceleration, float brake, float handling, TrackDefinition trackOverride = null)
        {
            TrackDefinition trackDef = trackOverride;
            if (trackDef == null)
            {
                trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
                toDestroy.Add(trackDef);
                trackDef.Spline.Knots = new[]
                {
                    new BezierKnot(new Vector3(0f, 0f, 0f)),
                    new BezierKnot(new Vector3(300f, 0f, 0f)),
                };
                trackDef.Spline.Closed = false;
            }

            VariableSO MakeFloatVar()
            {
                var v = ScriptableObject.CreateInstance<VariableSO>();
                var vSo = new SerializedObject(v);
                vSo.FindProperty("valueType").enumValueIndex = (int)VariableValueType.Float;
                vSo.ApplyModifiedPropertiesWithoutUndo();
                toDestroy.Add(v);
                return v;
            }

            VariableSO maxStraightVar = MakeFloatVar();
            VariableSO maxCurveVar = MakeFloatVar();
            VariableSO accelerationVar = MakeFloatVar();
            VariableSO brakeVar = MakeFloatVar();
            VariableSO handlingVar = MakeFloatVar();

            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            toDestroy.Add(carDef);
            var defSo = new SerializedObject(carDef);
            defSo.FindProperty("carPrefab").objectReferenceValue = null;
            SerializedProperty entries = defSo.FindProperty("bag").FindPropertyRelative("entries");
            entries.arraySize = 5;
            void SetBagEntry(int index, VariableSO variable, float value)
            {
                SerializedProperty e = entries.GetArrayElementAtIndex(index);
                e.FindPropertyRelative("variable").objectReferenceValue = variable;
                e.FindPropertyRelative("baseValue").managedReferenceValue = new FloatVariableValue { Value = value };
            }

            SetBagEntry(0, maxStraightVar, maxStraight);
            SetBagEntry(1, accelerationVar, acceleration);
            SetBagEntry(2, brakeVar, brake);
            SetBagEntry(3, handlingVar, handling);
            SetBagEntry(4, maxCurveVar, maxCurve);
            defSo.ApplyModifiedPropertiesWithoutUndo();

            var carVars = ScriptableObject.CreateInstance<CarVariableSet>();
            toDestroy.Add(carVars);
            carVars.AssignVariablesForTests(maxStraightVar, maxCurveVar, accelerationVar, brakeVar, handlingVar);

            CarEntity car = new CarEntityFactory().Create(carDef);
            return (car, carVars, trackDef);
        }
    }
}
