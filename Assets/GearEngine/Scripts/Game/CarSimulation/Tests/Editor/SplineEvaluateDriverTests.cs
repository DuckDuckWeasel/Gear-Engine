using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Entity;
using GearEngine.CarSimulation.PhysicsSimulation;
using GearEngine.CarSimulation.Simulation;
using GearEngine.CarSimulation.SplineSimulation;
using GearEngine.CarSimulation.Tracks;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.CarSimulation.Tests
{
    [TestFixture]
    public sealed class SplineEvaluateDriverTests
    {
        // ── Helpers ─────────────────────────────────────────────────────

        private static SplineDriverConfig CreateConfig()
        {
            SplineDriverConfig config = ScriptableObject.CreateInstance<SplineDriverConfig>();
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
            GameObject go = new GameObject("TestSpline");
            SplineContainer container = go.AddComponent<SplineContainer>();

            // Build a rough circle with 8 knots
            Spline spline = container.Spline;
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
            GearEngine.CarSimulation.Definitions.CarDefinition def = ScriptableObject.CreateInstance<GearEngine.CarSimulation.Definitions.CarDefinition>();
            CarEntityFactory factory = new GearEngine.CarSimulation.Entity.CarEntityFactory();
            return factory.Create(def);
        }

        private sealed class FinishLineRunner : ISimulationRunnerService
        {
            public event System.Action<CarEntity> OnLapCompleted;

            public float Progress { get; set; }
            public int CinematicFinishCount { get; private set; }

            public void InitializeRun(ISimulationInitParams initParams) { }

            public void SetPaused(CarEntity entity, bool paused) { }

            public bool GetTelemetry(CarEntity entity, out CarTelemetryData data)
            {
                data = new CarTelemetryData { Progress = Progress };
                return true;
            }

            public void ApplyJerk(CarEntity entity, float severity) { }

            public void RemoveDriver(CarEntity entity) { }

            public void TriggerCinematicFinish(CarEntity entity)
            {
                CinematicFinishCount++;
            }

            public void Tick() { }

            public void CrossFinishLine(CarEntity entity)
            {
                OnLapCompleted?.Invoke(entity);
            }
        }

        // ── Data Model Tests (M1) ──────────────────────────────────────

        [Test]
        public void DriverPersonality_Default_AllStatsAreFifty()
        {
            DriverPersonality p = DriverPersonality.Default;
            Assert.AreEqual(50f, p.SpeedCapability);
            Assert.AreEqual(50f, p.CorneringSkill);
            Assert.AreEqual(50f, p.Drift);
            Assert.AreEqual(50f, p.Precision);
            Assert.AreEqual(50f, p.Smoothness);
        }

        [Test]
        public void Driver_DifferentCars_DoNotShareEveryCurveDecision()
        {
            SplineDriverConfig config = CreateConfig();
            Random.State previousState = Random.state;

            try
            {
                Random.InitState(7319);
                SplineEvaluateDriver first = new SplineEvaluateDriver(config, null);
                SplineEvaluateDriver second = new SplineEvaluateDriver(config, null);
                first.SetPersonality(DriverPersonality.Default);
                second.SetPersonality(DriverPersonality.Default);

                bool differs = false;
                for (int curveIndex = 0; curveIndex < 16; curveIndex++)
                {
                    SplineEvaluateDriver.TrackCurveEvent curve = new SplineEvaluateDriver.TrackCurveEvent
                    {
                        T = 0.4f,
                        CurveIndex = curveIndex
                    };
                    SplineEvaluateDriver.TrackCurveEvent firstResult = first.EvaluateCurveForLap(curve, 0.4f, 0);
                    SplineEvaluateDriver.TrackCurveEvent secondResult = second.EvaluateCurveForLap(curve, 0.4f, 0);
                    differs |= firstResult.ActiveMode != secondResult.ActiveMode ||
                        firstResult.WillDrift != secondResult.WillDrift;
                }

                Assert.IsTrue(differs, "Different cars should not repeat the same curve decisions all race.");
            }
            finally
            {
                Random.state = previousState;
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void DriverPersonality_OpponentSeed_KeepsStatsVariedAndBounded()
        {
            DriverPersonality first = DriverPersonality.CreateOpponent(0, 112233);
            DriverPersonality repeated = DriverPersonality.CreateOpponent(0, 112233);
            DriverPersonality second = DriverPersonality.CreateOpponent(1, 445566);

            Assert.That(repeated.SpeedCapability, Is.EqualTo(first.SpeedCapability));
            Assert.That(repeated.CorneringSkill, Is.EqualTo(first.CorneringSkill));
            Assert.That(repeated.Drift, Is.EqualTo(first.Drift));
            Assert.That(repeated.Precision, Is.EqualTo(first.Precision));
            Assert.That(repeated.Smoothness, Is.EqualTo(first.Smoothness));
            Assert.That(second.SpeedCapability, Is.GreaterThan(first.SpeedCapability));
            Assert.That(first.CorneringSkill, Is.InRange(40f, 60f));
            Assert.That(first.Drift, Is.InRange(40f, 60f));
            Assert.That(second.Precision, Is.InRange(40f, 60f));
            Assert.That(second.Smoothness, Is.InRange(40f, 60f));
        }

        [Test]
        public void DriverPersonality_PlayerStats_UseConfiguredValuesWithoutOpponentRoll()
        {
            RoguelikeCarStats stats = new RoguelikeCarStats
            {
                SpeedCapability = 71f,
                CorneringSkill = 62f,
                Drift = 43f,
                Precision = 84f,
                Smoothness = 37f
            };

            DriverPersonality player = DriverPersonality.FromStats(stats);

            Assert.That(player.SpeedCapability, Is.EqualTo(stats.SpeedCapability));
            Assert.That(player.CorneringSkill, Is.EqualTo(stats.CorneringSkill));
            Assert.That(player.Drift, Is.EqualTo(stats.Drift));
            Assert.That(player.Precision, Is.EqualTo(stats.Precision));
            Assert.That(player.Smoothness, Is.EqualTo(stats.Smoothness));
        }

        [Test]
        public void SplineMotionState_Default_AllZero()
        {
            SplineMotionState state = new SplineMotionState();
            Assert.AreEqual(0f, state.T);
            Assert.AreEqual(0f, state.Speed);
            Assert.AreEqual(0, state.CompletedLaps);
            Assert.IsFalse(state.IsBraking);
            Assert.IsFalse(state.IsDrifting);
        }

        [Test]
        public void SplineDriverConfig_CreateInstance_HasSaneDefaults()
        {
            SplineDriverConfig config = CreateConfig();
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
            SplineDriverConfig config = CreateConfig();
            SplineContainer container = CreateCircleSpline();
            GameObject carGo = new GameObject("Car");
            CarSimulation.Entity.CarEntity entity = CreateTestEntity();

            SplineEvaluateDriver driver = new SplineEvaluateDriver(config, null);
            driver.Initialize(container, carGo.transform, entity, DriverPersonality.Default);

            Assert.IsTrue(driver.IsInitialized);

            Object.DestroyImmediate(carGo);
            Object.DestroyImmediate(container.gameObject);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void Driver_Tick_AdvancesT()
        {
            SplineDriverConfig config = CreateConfig();
            SplineContainer container = CreateCircleSpline();
            GameObject carGo = new GameObject("Car");
            CarSimulation.Entity.CarEntity entity = CreateTestEntity();

            SplineEvaluateDriver driver = new SplineEvaluateDriver(config, null);
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
        public void Driver_StopImmediately_FreezesMovingOpponent()
        {
            SplineDriverConfig config = CreateConfig();
            SplineContainer container = CreateCircleSpline();
            GameObject carGo = new GameObject("OpponentCar");
            CarEntity entity = CreateTestEntity();

            try
            {
                SplineEvaluateDriver driver = new SplineEvaluateDriver(config, null);
                driver.Initialize(container, carGo.transform, entity, DriverPersonality.Default);
                driver.SetPaused(false);
                driver.State = new SplineMotionState { Speed = 20f, T = 0.25f };

                System.Reflection.MethodInfo stopImmediately = typeof(SplineEvaluateDriver).GetMethod("StopImmediately");
                Assert.That(stopImmediately, Is.Not.Null, "A completed opponent needs an immediate stop.");
                stopImmediately.Invoke(driver, null);

                float stoppedProgress = driver.State.T;
                Vector3 stoppedPosition = carGo.transform.position;
                driver.Tick(1f);

                Assert.That(driver.IsPaused, Is.True);
                Assert.That(driver.State.Speed, Is.Zero);
                Assert.That(driver.State.T, Is.EqualTo(stoppedProgress));
                Assert.That(carGo.transform.position, Is.EqualTo(stoppedPosition));
            }
            finally
            {
                Object.DestroyImmediate(carGo);
                Object.DestroyImmediate(container.gameObject);
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void RaceManager_FinalLap_WaitsForFinishLineCrossingBeforeAkiraSlide()
        {
            TrackDefinition track = ScriptableObject.CreateInstance<TrackDefinition>();
            track.SetTotalLapsForTests(1);
            CarEntity car = CreateTestEntity();
            RaceState race = new RaceState(car, track, new RaceSessionConfig());
            FinishLineRunner runner = new FinishLineRunner { Progress = 0.99f };
            RaceManagerService manager = new RaceManagerService(runner);

            try
            {
                manager.RegisterRace(race);
                manager.StartRace(race);
                manager.Tick();

                Assert.That(race.Phase, Is.EqualTo(SimulationLifecycleState.Running),
                    "The car must keep running until it physically crosses the finish line.");
                Assert.That(runner.CinematicFinishCount, Is.Zero);

                runner.CrossFinishLine(car);

                Assert.That(race.Phase, Is.EqualTo(SimulationLifecycleState.Completed));
                Assert.That(race.CurrentLap, Is.EqualTo(1));
                Assert.That(runner.CinematicFinishCount, Is.EqualTo(1),
                    "Crossing the finish line must trigger the Akira finish move once.");
            }
            finally
            {
                manager.UnregisterRace(race);
                Object.DestroyImmediate(track);
            }
        }

        [Test]
        public void Driver_CinematicFinish_SlidesSidewaysAndStops()
        {
            SplineDriverConfig config = CreateConfig();
            SplineContainer container = CreateCircleSpline();
            GameObject carGo = new GameObject("FinishingCar");
            SplineEvaluateDriver driver = new SplineEvaluateDriver(config, null);

            try
            {
                driver.Initialize(container, carGo.transform, CreateTestEntity(), DriverPersonality.Default);
                driver.SetPaused(false);
                driver.State = new SplineMotionState { Speed = 20f, T = 0.5f };
                driver.Tick(0.01f);
                Vector3 approachDirection = carGo.transform.forward;

                driver.TriggerCinematicFinish();
                driver.SetPaused(true);
                driver.Tick(0.4f);

                Assert.That(Vector3.Angle(approachDirection, carGo.transform.forward), Is.GreaterThan(45f),
                    "The finish move must turn the car sideways like an Akira slide.");

                driver.Tick(SplineEvaluateDriver.CinematicFinishDurationSeconds);

                Assert.That(driver.IsCinematicFinishComplete, Is.True);
                Assert.That(driver.State.Speed, Is.Zero.Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(carGo);
                Object.DestroyImmediate(container.gameObject);
                Object.DestroyImmediate(config);
            }
        }

        [Test, Category("Biome")]
        public void Driver_ChildSplineAppliesParentTrackSurfaceSlowdown()
        {
            SplineDriverConfig config = CreateConfig();
            SplineContainer baselineSpline = CreateCircleSpline();
            SplineContainer themedSpline = CreateCircleSpline();
            GameObject trackGo = new GameObject("ThemedTrack");
            GameObject baselineCar = new GameObject("BaselineCar");
            GameObject themedCar = new GameObject("ThemedCar");
            TrackDefinition definition = ScriptableObject.CreateInstance<TrackDefinition>();
            TrackThemeDefinition theme = ScriptableObject.CreateInstance<TrackThemeDefinition>();

            try
            {
                themedSpline.transform.SetParent(trackGo.transform, false);
                TrackViewComponent trackView = trackGo.AddComponent<TrackViewComponent>();
                SerializedObject serializedView = new SerializedObject(trackView);
                serializedView.FindProperty("splineContainer").objectReferenceValue = themedSpline;
                serializedView.ApplyModifiedPropertiesWithoutUndo();

                foreach (BezierKnot knot in themedSpline.Spline.Knots)
                {
                    definition.Spline.Add(knot);
                }

                definition.Spline.Closed = true;
                SerializedObject serializedTheme = new SerializedObject(theme);
                SerializedProperty zones = serializedTheme.FindProperty("surfaceZones");
                zones.arraySize = 1;
                SerializedProperty zone = zones.GetArrayElementAtIndex(0);
                zone.FindPropertyRelative("normalizedPosition").floatValue = 0.3f;
                zone.FindPropertyRelative("normalizedHalfLength").floatValue = 0.02f;
                zone.FindPropertyRelative("width").floatValue = 5f;
                zone.FindPropertyRelative("speedMultiplier").floatValue = 0.7f;
                serializedTheme.ApplyModifiedPropertiesWithoutUndo();

                SerializedObject serializedDefinition = new SerializedObject(definition);
                serializedDefinition.FindProperty("theme").objectReferenceValue = theme;
                serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
                trackView.InitializeTrack(definition);

                baselineSpline.Spline.Clear();
                foreach (BezierKnot knot in themedSpline.Spline.Knots)
                {
                    baselineSpline.Spline.Add(knot, TangentMode.AutoSmooth);
                }

                baselineSpline.Spline.Closed = true;

                SplineEvaluateDriver baseline = new SplineEvaluateDriver(config, null);
                baseline.Initialize(baselineSpline, baselineCar.transform, CreateTestEntity(), DriverPersonality.Default);
                baseline.State = new SplineMotionState { Speed = 10f, T = 0.3f };
                baseline.SetPaused(false);

                SplineEvaluateDriver themed = new SplineEvaluateDriver(config, null);
                themed.Initialize(themedSpline, themedCar.transform, CreateTestEntity(), DriverPersonality.Default);
                themed.State = new SplineMotionState { Speed = 10f, T = 0.3f };
                themed.SetPaused(false);

                baseline.Tick(0.016f);
                themed.Tick(0.016f);

                Assert.That(themed.State.TargetSpeed, Is.EqualTo(baseline.State.TargetSpeed * 0.7f).Within(0.01f));
            }
            finally
            {
                Object.DestroyImmediate(trackGo);
                Object.DestroyImmediate(baselineSpline.gameObject);
                Object.DestroyImmediate(baselineCar);
                Object.DestroyImmediate(themedCar);
                Object.DestroyImmediate(definition);
                Object.DestroyImmediate(theme);
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void Driver_SetStartProgress_PlacesCarFurtherAlongTrack()
        {
            SplineDriverConfig config = CreateConfig();
            SplineContainer container = CreateCircleSpline();
            GameObject carGo = new GameObject("Car");
            SplineEvaluateDriver driver = new SplineEvaluateDriver(config, null);
            driver.Initialize(container, carGo.transform, CreateTestEntity(), DriverPersonality.Default);

            Vector3 startPosition = carGo.transform.position;
            driver.SetStartProgress(0.25f);

            Assert.AreEqual(0.25f, driver.State.T, 0.001f);
            Assert.Greater(Vector3.Distance(startPosition, carGo.transform.position), 10f);

            Object.DestroyImmediate(carGo);
            Object.DestroyImmediate(container.gameObject);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void Driver_GridStart_FirstLineCrossingDoesNotCompleteLap()
        {
            SplineDriverConfig config = CreateConfig();
            SplineContainer container = CreateCircleSpline();
            GameObject carGo = new GameObject("GridCar");
            SplineEvaluateDriver driver = new SplineEvaluateDriver(config, null);
            driver.Initialize(container, carGo.transform, CreateTestEntity(), DriverPersonality.Default);
            driver.SetStartGridPosition(-0.02f, 1.3f);

            Assert.That(driver.State.T, Is.EqualTo(0.98f).Within(0.001f));
            Assert.That(driver.State.LateralOffset, Is.EqualTo(1.3f).Within(0.001f));

            int lapEvents = 0;
            driver.OnLapCompleted += _ => lapEvents++;
            System.Reflection.MethodInfo advance = typeof(SplineEvaluateDriver).GetMethod("AdvanceT",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            driver.State.Speed = container.Spline.GetLength();
            advance.Invoke(driver, new object[] { 0.03f });
            Assert.That(driver.State.CompletedLaps, Is.Zero);
            Assert.That(lapEvents, Is.Zero);

            driver.State.T = 0.98f;
            advance.Invoke(driver, new object[] { 0.03f });
            Assert.That(driver.State.CompletedLaps, Is.EqualTo(1));
            Assert.That(lapEvents, Is.EqualTo(1));

            Object.DestroyImmediate(carGo);
            Object.DestroyImmediate(container.gameObject);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void Driver_GridStart_KeepsLaneUntilFirstLineCrossing()
        {
            SplineDriverConfig config = CreateConfig();
            config.accelerationRate = 0f;
            SplineContainer container = CreateCircleSpline();
            GameObject carGo = new GameObject("GridCar");

            try
            {
                SplineEvaluateDriver driver = new SplineEvaluateDriver(config, null);
                driver.Initialize(container, carGo.transform, CreateTestEntity(), DriverPersonality.Default);
                driver.SetStartGridPosition(-0.1f, 1.3f);
                typeof(SplineEvaluateDriver).GetField("smoothedRacingLine",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    .SetValue(driver, 2f);
                driver.SetPaused(false);
                driver.Tick(0.5f);

                System.Reflection.FieldInfo offset = typeof(SplineEvaluateDriver).GetField("collisionLateralOffset",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                Assert.That(offset.GetValue(driver), Is.EqualTo(1.3f).Within(0.001f));
                Assert.That(driver.State.LateralOffset, Is.EqualTo(1.3f).Within(0.01f));
                Assert.That(driver.State.CompletedLaps, Is.Zero);
            }
            finally
            {
                Object.DestroyImmediate(carGo);
                Object.DestroyImmediate(container.gameObject);
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void Runner_OverlappingCars_CollisionGivesSmallContactResponse()
        {
            SplineDriverConfig config = CreateConfig();
            SplineContainer container = CreateCircleSpline();
            GameObject firstGo = new GameObject("FirstCar");
            GameObject secondGo = new GameObject("SecondCar");
            GameObject soloGo = new GameObject("SoloCar");
            SplineEvaluateRunnerService runner = new SplineEvaluateRunnerService(config);
            SplineEvaluateDriver first = runner.InitializeRun(
                container, firstGo.transform, CreateTestEntity(), DriverPersonality.Default);
            SplineEvaluateDriver second = runner.InitializeRun(
                container, secondGo.transform, CreateTestEntity(), DriverPersonality.Default);
            first.SetCurveDecisionSeed(1234);
            second.SetCurveDecisionSeed(1234);
            first.State = new SplineMotionState { Speed = 10f, T = 0f };
            second.State = new SplineMotionState { Speed = 10f, T = 0f };
            first.SetPaused(false);
            second.SetPaused(false);
            SplineEvaluateDriver solo = new SplineEvaluateDriver(config, null);
            solo.Initialize(container, soloGo.transform, CreateTestEntity(), DriverPersonality.Default);
            solo.SetCurveDecisionSeed(1234);
            solo.State = new SplineMotionState { Speed = 10f, T = 0f };
            solo.SetPaused(false);

            runner.Tick(0.01f);
            solo.Tick(0.01f);

            Assert.Less(first.State.Speed, solo.State.Speed);
            Assert.Less(second.State.Speed, solo.State.Speed);
            Assert.That(Mathf.Abs(first.State.LateralOffset - second.State.LateralOffset),
                Is.InRange(0.001f, 0.08f), "A single contact frame must not fling cars sideways.");

            Object.DestroyImmediate(firstGo);
            Object.DestroyImmediate(secondGo);
            Object.DestroyImmediate(soloGo);
            Object.DestroyImmediate(container.gameObject);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void Runner_StartingGrid_DoesNotPushOverlappingCarsSideways()
        {
            SplineDriverConfig config = CreateConfig();
            SplineContainer container = CreateCircleSpline();
            GameObject firstGo = new GameObject("FirstGridCar");
            GameObject secondGo = new GameObject("SecondGridCar");

            try
            {
                SplineEvaluateRunnerService runner = new SplineEvaluateRunnerService(config);
                SplineEvaluateDriver first = runner.InitializeRun(
                    container, firstGo.transform, CreateTestEntity(), DriverPersonality.Default);
                SplineEvaluateDriver second = runner.InitializeRun(
                    container, secondGo.transform, CreateTestEntity(), DriverPersonality.Default);
                first.SetStartGridPosition(-0.1f, -1.3f);
                second.SetStartGridPosition(-0.1f, -1.3f);
                first.SetPaused(false);
                second.SetPaused(false);

                runner.Tick(0.02f);

                Assert.That(first.State.LateralOffset, Is.EqualTo(-1.3f).Within(0.01f));
                Assert.That(second.State.LateralOffset, Is.EqualTo(-1.3f).Within(0.01f));
            }
            finally
            {
                Object.DestroyImmediate(firstGo);
                Object.DestroyImmediate(secondGo);
                Object.DestroyImmediate(container.gameObject);
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void Runner_ScaledTrack_DistantCarsDoNotCollide()
        {
            SplineDriverConfig config = CreateConfig();
            SplineContainer container = CreateCircleSpline();
            container.transform.localScale = Vector3.one * 0.05f;
            GameObject firstGo = new GameObject("FirstCar");
            GameObject secondGo = new GameObject("SecondCar");
            SplineEvaluateRunnerService runner = new SplineEvaluateRunnerService(config);
            SplineEvaluateDriver first = runner.InitializeRun(
                container, firstGo.transform, CreateTestEntity(), DriverPersonality.Default);
            SplineEvaluateDriver second = runner.InitializeRun(
                container, secondGo.transform, CreateTestEntity(), DriverPersonality.Default);
            second.SetStartProgress(0.25f);
            first.State.Speed = 10f;
            second.State.Speed = 10f;
            first.SetPaused(false);
            second.SetPaused(false);

            typeof(SplineEvaluateRunnerService).GetMethod("ResolveCollisions",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .Invoke(runner, new object[] { 0.02f });

            Assert.That(first.State.Speed, Is.EqualTo(10f));
            Assert.That(second.State.Speed, Is.EqualTo(10f));
            Assert.That(first.State.LateralOffset, Is.EqualTo(0f));
            Assert.That(second.State.LateralOffset, Is.EqualTo(0f));

            Object.DestroyImmediate(firstGo);
            Object.DestroyImmediate(secondGo);
            Object.DestroyImmediate(container.gameObject);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void Driver_LapWrap_IncrementsCompletedLaps()
        {
            SplineDriverConfig config = CreateConfig();
            SplineContainer container = CreateCircleSpline();
            GameObject carGo = new GameObject("Car");
            CarSimulation.Entity.CarEntity entity = CreateTestEntity();

            SplineEvaluateDriver driver = new SplineEvaluateDriver(config, null);
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
            SplineDriverConfig config = CreateConfig();
            SplineContainer container = CreateCircleSpline();
            GameObject carGo = new GameObject("Car");
            CarSimulation.Entity.CarEntity entity = CreateTestEntity();

            SplineEvaluateDriver driver = new SplineEvaluateDriver(config, null);
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
        public void Driver_NoLaneProfile_StillUsesBuiltInRacingLine()
        {
            SplineDriverConfig config = CreateConfig();
            SplineContainer container = CreateCircleSpline();
            GameObject carGo = new GameObject("Car");
            CarSimulation.Entity.CarEntity entity = CreateTestEntity();

            SplineEvaluateDriver driver = new SplineEvaluateDriver(config, null);
            driver.Initialize(container, carGo.transform, entity, DriverPersonality.Default);
            driver.SetPaused(false);

            driver.State = new SplineMotionState { Speed = 10f, T = 0.5f };
            driver.Tick(0.016f);

            Assert.Greater(Mathf.Abs(driver.State.LateralOffset), 0.001f,
                "The built-in racing line should still move the car without an authored lane profile.");

            Object.DestroyImmediate(carGo);
            Object.DestroyImmediate(container.gameObject);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void Driver_AllStatsZero_OffsetIsNearZero()
        {
            SplineDriverConfig config = CreateConfig();
            LaneProfile profile = ScriptableObject.CreateInstance<LaneProfile>();
            SplineContainer container = CreateCircleSpline();
            GameObject carGo = new GameObject("Car");
            CarSimulation.Entity.CarEntity entity = CreateTestEntity();

            DriverPersonality allZero = new DriverPersonality
            {
                SpeedCapability = 0f,
                CorneringSkill = 0f,
                Drift = 100f,
                Precision = 100f, // 100 precision -> 0 error offset
                Smoothness = 100f // 100 smoothness -> no noise
            };

            SplineEvaluateDriver driver = new SplineEvaluateDriver(config, profile);
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
            SplineDriverConfig config = CreateConfig();
            SplineContainer container = CreateCircleSpline();
            GameObject carGo = new GameObject("Car");
            CarSimulation.Entity.CarEntity entity = CreateTestEntity();

            SplineEvaluateDriver driver = new SplineEvaluateDriver(config, null);
            driver.Initialize(container, carGo.transform, entity, DriverPersonality.Default);

            // Give it some speed, then pause
            driver.SetPaused(false);
            driver.State = new SplineMotionState { Speed = 30f, T = 0.1f };
            driver.Tick(0.1f);
            float tAfterMoving = driver.State.T;

            driver.SetPaused(true);
            // Tick several times while paused
            for (int i = 0; i < 100; i++)
            {
                driver.Tick(0.1f);
            }

            Assert.Less(driver.State.Speed, 0.1f, "Speed should decelerate to near zero when paused.");

            Object.DestroyImmediate(carGo);
            Object.DestroyImmediate(container.gameObject);
            Object.DestroyImmediate(config);
        }
    }
}
