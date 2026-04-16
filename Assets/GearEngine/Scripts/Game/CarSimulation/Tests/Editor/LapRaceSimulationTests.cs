using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Simulation;
using NUnit.Framework;
using Scaffold.Entities;
using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.CarSimulation.Tests
{
    public sealed class LapRaceSimulationTests
    {
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
                cfg.Lap.TotalLaps = 5;
                cfg.Lap.CurveSlowdown = 0.1f;

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
                    while (session.RaceState.LapTimes.Count == 0 && guard++ < 200000)
                    {
                        session.Tick(0.02f);
                    }

                    Assert.That(session.RaceState.LapTimes.Count, Is.GreaterThan(0));
                    Assert.That(session.RaceState.LapTimes[0], Is.GreaterThan(0f));
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
                cfg.Lap.TotalLaps = -1;
                cfg.Lap.CurveSlowdown = 0.1f;

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

                    Assert.That(session.RaceState.Lifecycle, Is.EqualTo(RaceLifecycle.Running));
                    Assert.That(session.RaceState.CurrentLap, Is.GreaterThan(5));
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
        public void DisablingVisualPlayback_DoesNotChangeRaceProgress()
        {
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            try
            {
                trackDef.Spline.Knots = new[]
                {
                    new BezierKnot(new Vector3(0f, 0f, 0f)),
                    new BezierKnot(new Vector3(30f, 0f, 0f)),
                    new BezierKnot(new Vector3(30f, 0f, 30f)),
                    new BezierKnot(new Vector3(0f, 0f, 30f)),
                };
                trackDef.Spline.Closed = true;

                var cfg = new RaceSessionConfig();
                cfg.Lap.TotalLaps = 10;

                float ProgressAfterTicks(LapRaceSession s, int ticks, bool visual)
                {
                    var go = new GameObject("SplineHost");
                    try
                    {
                        SplineContainer container = go.AddComponent<SplineContainer>();
                        container.Spline.Knots = trackDef.Spline.Knots;
                        container.Spline.Closed = true;
                        s.BindSpline(container);
                        s.VisualPlaybackEnabled = visual;
                        s.Reset();
                        s.SetClockRunning(true);
                        for (int i = 0; i < ticks; i++)
                        {
                            s.Tick(0.02f);
                        }

                        return s.RaceState.ProgressDistance;
                    }
                    finally
                    {
                        Object.DestroyImmediate(go);
                    }
                }

                LapRaceSession a = new TrackSimulationFactory().Create(carDef, trackDef, cfg);
                LapRaceSession b = new TrackSimulationFactory().Create(carDef, trackDef, cfg);
                const int ticks = 400;
                float pVisualOn = ProgressAfterTicks(a, ticks, visual: true);
                float pVisualOff = ProgressAfterTicks(b, ticks, visual: false);

                Assert.That(pVisualOff, Is.EqualTo(pVisualOn).Within(1e-3f));
            }
            finally
            {
                Object.DestroyImmediate(trackDef);
                Object.DestroyImmediate(carDef);
            }
        }

        [Test]
        public void HigherMaxStraightSpeed_OnCar_IncreasesProgressOverSameTicks()
        {
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            VariableSO speedVar = ScriptableObject.CreateInstance<VariableSO>();
            VariableSO accelVar = ScriptableObject.CreateInstance<VariableSO>();
            VariableSO handleVar = ScriptableObject.CreateInstance<VariableSO>();
            var vars = ScriptableObject.CreateInstance<CarVariableSet>();
            try
            {
                trackDef.Spline.Knots = new[]
                {
                    new BezierKnot(Vector3.zero),
                    new BezierKnot(Vector3.right * 25f),
                };
                trackDef.Spline.Closed = false;

                vars.AssignVariablesForTests(speedVar, accelVar, handleVar);

                float RunWithMaxStraight(float maxStraight)
                {
                    var cfg = new RaceSessionConfig();
                    cfg.SetVariablesForTests(vars);
                    var go = new GameObject("OpenSpline");
                    try
                    {
                        LapRaceSession s = new TrackSimulationFactory().Create(carDef, trackDef, cfg);
                        s.Car.AddModifier(new EntityModifierEntry(speedVar, new FloatVariableValue { Value = maxStraight }));
                        s.Car.AddModifier(new EntityModifierEntry(accelVar, new FloatVariableValue { Value = 100f }));
                        s.Car.AddModifier(new EntityModifierEntry(handleVar, new FloatVariableValue { Value = 10f }));
                        SplineContainer container = go.AddComponent<SplineContainer>();
                        container.Spline.Knots = trackDef.Spline.Knots;
                        container.Spline.Closed = false;
                        s.BindSpline(container);
                        s.SetClockRunning(true);
                        for (int i = 0; i < 60; i++)
                        {
                            s.Tick(0.05f);
                        }

                        return s.RaceState.ProgressDistance;
                    }
                    finally
                    {
                        Object.DestroyImmediate(go);
                    }
                }

                float slow = RunWithMaxStraight(20f);
                float fast = RunWithMaxStraight(80f);
                Assert.That(fast, Is.GreaterThan(slow));
            }
            finally
            {
                Object.DestroyImmediate(trackDef);
                Object.DestroyImmediate(carDef);
                Object.DestroyImmediate(speedVar);
                Object.DestroyImmediate(accelVar);
                Object.DestroyImmediate(handleVar);
                Object.DestroyImmediate(vars);
            }
        }
    }
}
