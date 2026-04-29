using GearEngine.SplineEvaluate.Definitions;
using GearEngine.SplineEvaluate.Simulation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.SplineEvaluate.Tests
{
    [TestFixture]
    public sealed class SplineEvaluateDriverTests
    {
        // ── Helpers ─────────────────────────────────────────────────────

        private static SplineDriverConfig CreateConfig()
        {
            var config = ScriptableObject.CreateInstance<SplineDriverConfig>();
            config.maxSpeed = 50f;
            config.minCurveSpeed = 10f;
            config.accelerationRate = 20f;
            config.brakeRate = 40f;
            config.curvatureLookaheadMeters = 30f;
            config.curvatureSampleCount = 6;
            config.maxCurvatureReference = 0.15f;
            config.riskLookaheadMultiplier = new Vector2(1.2f, 0.6f);
            config.maxLateralOffset = 4f;
            config.lateralSmoothRate = 10f;
            config.bodyRollScale = 0.08f;
            config.maxBodyRollDeg = 8f;
            config.slipAngleScale = 3f;
            config.maxSlipAngleDeg = 15f;
            config.slipAngleSmoothRate = 8f;
            config.suspensionBobFrequency = 1.5f;
            config.suspensionBobAmplitude = 0.02f;
            return config;
        }

        /// <summary>
        /// Creates a simple closed-loop circle spline for testing.
        /// </summary>
        private static SplineContainer CreateCircleSpline()
        {
            var go = new GameObject("TestSpline");
            var container = go.AddComponent<SplineContainer>();

            // Build a rough circle with 8 knots
            var spline = container.Spline;
            spline.Clear();
            float radius = 20f;
            int segments = 8;
            for (int i = 0; i < segments; i++)
            {
                float angle = (float)i / segments * Mathf.PI * 2f;
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;
                spline.Add(new BezierKnot(new Unity.Mathematics.float3(x, 0f, z)));
            }
            spline.Closed = true;

            return container;
        }

        private static GearEngine.CarSimulation.Entity.CarEntity CreateTestEntity()
        {
            var def = ScriptableObject.CreateInstance<GearEngine.CarSimulation.Definitions.CarDefinition>();
            var factory = new GearEngine.CarSimulation.Entity.CarEntityFactory();
            return factory.Create(def);
        }

        // ── Data Model Tests (M1) ──────────────────────────────────────

        [Test]
        public void DriverPersonality_Default_AllStatsAreFive()
        {
            DriverPersonality p = DriverPersonality.Default;
            Assert.AreEqual(5f, p.SpeedCapability);
            Assert.AreEqual(5f, p.CorneringSkill);
            Assert.AreEqual(5f, p.Drift);
            Assert.AreEqual(5f, p.Precision);
            Assert.AreEqual(5f, p.Smoothness);
        }

        [Test]
        public void SplineMotionState_Default_AllZero()
        {
            var state = new SplineMotionState();
            Assert.AreEqual(0f, state.T);
            Assert.AreEqual(0f, state.Speed);
            Assert.AreEqual(0, state.CompletedLaps);
            Assert.IsFalse(state.IsBraking);
            Assert.IsFalse(state.IsDrifting);
        }

        [Test]
        public void SplineDriverConfig_CreateInstance_HasSaneDefaults()
        {
            var config = CreateConfig();
            Assert.Greater(config.maxSpeed, 0f);
            Assert.Greater(config.accelerationRate, 0f);
            Assert.Greater(config.brakeRate, 0f);
            Object.DestroyImmediate(config);
        }

        // ── Curvature Helper Tests ─────────────────────────────────────

        [Test]
        public void CurvatureHelper_WrapT_HandlesOverflow()
        {
            Assert.AreEqual(0.2f, SplineCurvatureHelper.WrapT(1.2f), 0.001f);
        }

        [Test]
        public void CurvatureHelper_WrapT_HandlesNegative()
        {
            Assert.AreEqual(0.8f, SplineCurvatureHelper.WrapT(-0.2f), 0.001f);
        }

        [Test]
        public void CurvatureHelper_CircleSpline_ReturnsPositiveCurvature()
        {
            SplineContainer container = CreateCircleSpline();
            float length = container.Spline.GetLength();

            float curvature = SplineCurvatureHelper.SampleCurvatureAt(container.Spline, length, 0.25f, out _);
            Assert.Greater(curvature, 0f, "A circle should have positive curvature.");

            Object.DestroyImmediate(container.gameObject);
        }

        [Test]
        public void CurvatureHelper_MaxCurvature_LookaheadReturnsNonNegative()
        {
            SplineContainer container = CreateCircleSpline();
            float length = container.Spline.GetLength();

            float maxCurv = SplineCurvatureHelper.SampleMaxCurvature(container.Spline, length, 0f, 30f, 6, out _);
            Assert.GreaterOrEqual(maxCurv, 0f);

            Object.DestroyImmediate(container.gameObject);
        }

        // ── Driver Core Tests (M2) ─────────────────────────────────────

        [Test]
        public void Driver_Initialize_SetsIsInitialized()
        {
            var config = CreateConfig();
            SplineContainer container = CreateCircleSpline();
            var carGo = new GameObject("Car");
            CarSimulation.Entity.CarEntity entity = CreateTestEntity();

            var driver = new SplineEvaluateDriver(config, null);
            driver.Initialize(container, carGo.transform, entity, DriverPersonality.Default);

            Assert.IsTrue(driver.IsInitialized);

            Object.DestroyImmediate(carGo);
            Object.DestroyImmediate(container.gameObject);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void Driver_Tick_AdvancesT()
        {
            var config = CreateConfig();
            SplineContainer container = CreateCircleSpline();
            var carGo = new GameObject("Car");
            CarSimulation.Entity.CarEntity entity = CreateTestEntity();

            var driver = new SplineEvaluateDriver(config, null);
            driver.Initialize(container, carGo.transform, entity, DriverPersonality.Default);
            driver.SetPaused(false);

            // Force a known speed
            driver.State = new SplineMotionState { Speed = 10f, T = 0f };
            driver.Tick(0.1f);

            Assert.Greater(driver.State.T, 0f, "T should advance after a tick with speed > 0.");

            Object.DestroyImmediate(carGo);
            Object.DestroyImmediate(container.gameObject);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void Driver_LapWrap_IncrementsCompletedLaps()
        {
            var config = CreateConfig();
            SplineContainer container = CreateCircleSpline();
            var carGo = new GameObject("Car");
            CarSimulation.Entity.CarEntity entity = CreateTestEntity();

            var driver = new SplineEvaluateDriver(config, null);
            driver.Initialize(container, carGo.transform, entity, DriverPersonality.Default);
            driver.SetPaused(false);

            float length = container.Spline.GetLength();
            // Set T near the end so a small tick wraps it
            driver.State = new SplineMotionState { Speed = length * 2f, T = 0.99f };

            int lapCount = 0;
            driver.OnLapCompleted += _ => lapCount++;

            driver.Tick(0.1f);

            Assert.AreEqual(1, driver.State.CompletedLaps);
            Assert.AreEqual(1, lapCount, "OnLapCompleted should have fired once.");

            Object.DestroyImmediate(carGo);
            Object.DestroyImmediate(container.gameObject);
            Object.DestroyImmediate(config);
        }

        // ── Speed Model Tests (M3) ─────────────────────────────────────

        [Test]
        public void Driver_SpeedModel_AcceleratesFromZero()
        {
            var config = CreateConfig();
            SplineContainer container = CreateCircleSpline();
            var carGo = new GameObject("Car");
            CarSimulation.Entity.CarEntity entity = CreateTestEntity();

            var driver = new SplineEvaluateDriver(config, null);
            driver.Initialize(container, carGo.transform, entity, DriverPersonality.Default);
            driver.SetPaused(false);

            // Start at zero speed on the circle
            driver.State = new SplineMotionState { Speed = 0f, T = 0f };
            driver.Tick(0.5f);

            Assert.Greater(driver.State.Speed, 0f, "Speed should increase from zero.");
            Assert.IsTrue(driver.State.IsAccelerating);

            Object.DestroyImmediate(carGo);
            Object.DestroyImmediate(container.gameObject);
            Object.DestroyImmediate(config);
        }

        // ── Lateral Offset Tests (M4) ──────────────────────────────────

        [Test]
        public void Driver_NoLaneProfile_OffsetIsZero()
        {
            var config = CreateConfig();
            SplineContainer container = CreateCircleSpline();
            var carGo = new GameObject("Car");
            CarSimulation.Entity.CarEntity entity = CreateTestEntity();

            var driver = new SplineEvaluateDriver(config, null);
            driver.Initialize(container, carGo.transform, entity, DriverPersonality.Default);
            driver.SetPaused(false);

            driver.State = new SplineMotionState { Speed = 10f, T = 0.5f };
            driver.Tick(0.016f);

            Assert.AreEqual(0f, driver.State.LateralOffset, 0.001f,
                "Without a LaneProfile, lateral offset should be zero.");

            Object.DestroyImmediate(carGo);
            Object.DestroyImmediate(container.gameObject);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void Driver_AllStatsZero_OffsetIsNearZero()
        {
            var config = CreateConfig();
            var profile = ScriptableObject.CreateInstance<LaneProfile>();
            SplineContainer container = CreateCircleSpline();
            var carGo = new GameObject("Car");
            CarSimulation.Entity.CarEntity entity = CreateTestEntity();

            var allZero = new DriverPersonality
            {
                SpeedCapability = 0f,
                CorneringSkill = 0f,
                Drift = 10f,
                Precision = 10f, // 10 precision -> 0 error offset
                Smoothness = 10f // 10 smoothness -> no noise
            };

            var driver = new SplineEvaluateDriver(config, profile);
            driver.Initialize(container, carGo.transform, entity, allZero);
            driver.SetPaused(false);

            driver.State = new SplineMotionState { Speed = 10f, T = 0.5f };
            driver.Tick(0.016f);

            // With flat curves (all default to constant 0) and all stats 0, offset should be ~0
            Assert.AreEqual(0f, driver.State.LateralOffset, 0.1f,
                "All stats at 0 with flat curves should produce near-zero offset.");

            Object.DestroyImmediate(carGo);
            Object.DestroyImmediate(container.gameObject);
            Object.DestroyImmediate(config);
            Object.DestroyImmediate(profile);
        }

        // ── Pause Tests ────────────────────────────────────────────────

        [Test]
        public void Driver_WhenPaused_SpeedDeceleratesAndTStops()
        {
            var config = CreateConfig();
            SplineContainer container = CreateCircleSpline();
            var carGo = new GameObject("Car");
            CarSimulation.Entity.CarEntity entity = CreateTestEntity();

            var driver = new SplineEvaluateDriver(config, null);
            driver.Initialize(container, carGo.transform, entity, DriverPersonality.Default);

            // Give it some speed, then pause
            driver.SetPaused(false);
            driver.State = new SplineMotionState { Speed = 30f, T = 0.1f };
            driver.Tick(0.1f);
            float tAfterMoving = driver.State.T;

            driver.SetPaused(true);
            // Tick several times while paused
            for (int i = 0; i < 100; i++) driver.Tick(0.1f);

            Assert.Less(driver.State.Speed, 0.1f, "Speed should decelerate to near zero when paused.");

            Object.DestroyImmediate(carGo);
            Object.DestroyImmediate(container.gameObject);
            Object.DestroyImmediate(config);
        }
    }
}
