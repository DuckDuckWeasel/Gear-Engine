using System;
using GearEngine.CarSimulation.Entity;
using GearEngine.SplineEvaluate.Definitions;
using UnityEngine;
using UnityEngine.Splines;
using VContainer.Unity;

namespace GearEngine.SplineEvaluate.Simulation
{
    /// <summary>
    /// Pure-spline car driver. Moves a car along a <see cref="SplineContainer"/> by
    /// advancing a normalized parameter <c>t</c> each frame. Position, rotation, and
    /// all visual effects are computed exclusively from spline evaluation — no
    /// Rigidbody, no physics forces, no PrometeoCarController.
    /// <para>
    /// The driver is a plain C# object ticked externally (e.g. via
    /// <see cref="SplineEvaluateRunnerService"/>). It writes to a
    /// <see cref="Transform"/> reference each tick.
    /// </para>
    /// </summary>
    public sealed class SplineEvaluateDriver
    {
        /// <summary>Fired when the car's <c>t</c> wraps past 1.0 (one lap completed).</summary>
        public event Action<CarEntity> OnLapCompleted;

        private readonly SplineDriverConfig config;
        private readonly LaneProfile laneProfile;
        private readonly float noiseSeed;

        private Spline spline;
        private Transform splineTransform;
        private Transform carTransform;
        private CarEntity carEntity;
        private DriverPersonality personality;
        private float splineLength;
        private bool isPaused = true;
        private bool isInitialized;

        public SplineMotionState State;

        public bool IsInitialized => isInitialized;
        public bool IsPaused => isPaused;
        public CarEntity CarEntity => carEntity;

        public SplineEvaluateDriver(SplineDriverConfig config, LaneProfile laneProfile)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.laneProfile = laneProfile;
            noiseSeed = UnityEngine.Random.Range(0f, 1000f);
        }

        /// <summary>
        /// Binds the driver to a specific spline and car transform. Must be called
        /// before the first <see cref="Tick"/>.
        /// </summary>
        public void Initialize(
            SplineContainer splineContainer,
            Transform carTransform,
            CarEntity carEntity,
            DriverPersonality personality)
        {
            if (splineContainer == null) throw new ArgumentNullException(nameof(splineContainer));
            if (splineContainer.Spline == null || splineContainer.Spline.Count < 2)
            {
                throw new ArgumentException("[SplineEvaluateDriver] Spline must have at least 2 knots.");
            }
            if (carTransform == null) throw new ArgumentNullException(nameof(carTransform));
            if (carEntity == null) throw new ArgumentNullException(nameof(carEntity));

            spline = splineContainer.Spline;
            splineTransform = splineContainer.transform;
            this.carTransform = carTransform;
            this.carEntity = carEntity;
            this.personality = personality;
            splineLength = spline.GetLength();

            State = new SplineMotionState();
            PlaceAtStart();

            isInitialized = true;
        }

        /// <summary>Updates the personality stats at runtime (e.g. from UI sliders).</summary>
        public void SetPersonality(DriverPersonality newPersonality)
        {
            personality = newPersonality;
        }

        public void SetPaused(bool paused)
        {
            isPaused = paused;
        }

        /// <summary>
        /// Advances the simulation by <paramref name="dt"/> seconds. This is the
        /// single entry point that computes position, speed, lateral offset, and
        /// all visual effects.
        /// </summary>
        public void Tick(float dt)
        {
            if (!isInitialized || dt <= 0f) return;

            if (isPaused)
            {
                TickPaused(dt);
                return;
            }

            // Store previous frame values for derivative computation
            State.PreviousT = State.T;
            State.PreviousLateralOffset = State.LateralOffset;

            // ── 1. Curvature analysis and target speed ──────────────────
            TickSpeedModel(dt);

            // ── 2. Advance t ────────────────────────────────────────────
            AdvanceT(dt);

            // ── 3. Lateral offset ───────────────────────────────────────
            TickLateralOffset(dt);

            // ── 4. Visual effects ───────────────────────────────────────
            TickVisuals(dt);

            // ── 5. Apply transform ──────────────────────────────────────
            ApplyTransform();
        }

        // ====================================================================
        // Speed Model (M3)
        // ====================================================================

        private void TickSpeedModel(float dt)
        {
            // Risk stat shortens effective lookahead → later braking
            float riskNorm = personality.Risk / 10f;
            float lookaheadMult = Mathf.Lerp(
                config.riskLookaheadMultiplier.x,
                config.riskLookaheadMultiplier.y,
                riskNorm);
            float effectiveLookahead = config.curvatureLookaheadMeters * lookaheadMult;

            // Sample curvature in the forward window
            State.Curvature = SplineCurvatureHelper.SampleCurvatureAt(spline, splineLength, State.T, out State.SignedCurvature);
            State.LookaheadMaxCurvature = SplineCurvatureHelper.SampleMaxCurvature(
                spline, splineLength, State.T, effectiveLookahead, config.curvatureSampleCount, out State.SignedLookaheadMaxCurvature);

            // Map curvature to speed cap
            float curvatureSeverity = Mathf.Clamp01(
                Mathf.InverseLerp(0f, config.maxCurvatureReference, State.LookaheadMaxCurvature));
            State.TargetSpeed = Mathf.Lerp(config.maxSpeed, config.minCurveSpeed, curvatureSeverity);

            // Integrate speed toward target
            if (State.Speed < State.TargetSpeed)
            {
                State.Speed = Mathf.MoveTowards(State.Speed, State.TargetSpeed, config.accelerationRate * dt);
                State.IsAccelerating = true;
                State.IsBraking = false;
            }
            else
            {
                State.Speed = Mathf.MoveTowards(State.Speed, State.TargetSpeed, config.brakeRate * dt);
                State.IsAccelerating = false;
                State.IsBraking = State.Speed > State.TargetSpeed + 0.5f;
            }
        }

        // ====================================================================
        // T Advancement (M2)
        // ====================================================================

        private void AdvanceT(float dt)
        {
            if (splineLength <= 0f) return;

            float distanceThisFrame = State.Speed * dt;
            State.T += distanceThisFrame / splineLength;

            // Lap wrap
            if (State.T >= 1f)
            {
                State.T -= 1f;
                State.CompletedLaps++;

                try
                {
                    OnLapCompleted?.Invoke(carEntity);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SplineEvaluateDriver] OnLapCompleted handler error: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }

        // ====================================================================
        // Lateral Offset (M4)
        // ====================================================================

        private void TickLateralOffset(float dt)
        {
            float t = State.T;
            float rawOffset = 0f;

            // ── Auto Racing Line (Out-In-Out) ───────────────────────────
            float currentSeverity = Mathf.InverseLerp(0.01f, config.maxCurvatureReference, State.Curvature);
            float lookaheadSeverity = Mathf.InverseLerp(0.01f, config.maxCurvatureReference, State.LookaheadMaxCurvature);

            // Move towards the inside of the *current* curve (Apex)
            float insideOffset = Mathf.Sign(State.SignedCurvature) * currentSeverity * config.maxLateralOffset;

            // Move towards the outside of the *upcoming* curve (Entry)
            float upcomingSeverity = Mathf.Max(0f, lookaheadSeverity - currentSeverity);
            float outsideOffset = -Mathf.Sign(State.SignedLookaheadMaxCurvature) * upcomingSeverity * config.maxLateralOffset;

            // Base dynamic offset based on curves
            float dynamicRacingLine = insideOffset + outsideOffset;
            
            // Scale by LineWidth personality stat (0 = drive in center, 10 = use full width)
            rawOffset += dynamicRacingLine * (personality.LineWidth / 10f);

            // ── Authored Lane Profile (optional overrides) ──────────────
            if (laneProfile != null)
            {
                float aggression = laneProfile.AggressionCurve.Evaluate(t) * (personality.Aggression / 10f);
                float drift = laneProfile.DriftCurve.Evaluate(t) * (personality.DriftTendency / 10f);
                float width = laneProfile.WidthCurve.Evaluate(t) * (personality.LineWidth / 10f);
                float riskEntry = laneProfile.RiskEntryCurve.Evaluate(t) * (personality.Risk / 10f);
                rawOffset += aggression + drift + width + riskEntry;
            }

            // ── Noise (Consistency) ─────────────────────────────────────
            float consistencyDamp = 1f - (personality.Consistency / 10f);
            float noise = ((Mathf.PerlinNoise(t * 50f, noiseSeed) * 2f) - 1f) * consistencyDamp * 0.5f;
            rawOffset += noise;

            State.RawLateralOffset = Mathf.Clamp(rawOffset, -config.maxLateralOffset, config.maxLateralOffset);

            // Smooth the transition
            State.LateralOffset = Mathf.Lerp(
                State.LateralOffset,
                State.RawLateralOffset,
                dt * config.lateralSmoothRate);
        }

        // ====================================================================
        // Visuals (M5)
        // ====================================================================

        private void TickVisuals(float dt)
        {
            // Body roll from centripetal acceleration: a_c = v² * curvature
            float centripetalAccel = State.Speed * State.Speed * State.Curvature;
            float targetRoll = -centripetalAccel * config.bodyRollScale;
            State.BodyRoll = Mathf.Clamp(targetRoll, -config.maxBodyRollDeg, config.maxBodyRollDeg);

            // Slip angle from rate of change of lateral offset + explicit drift curve
            float offsetRate = (dt > 0f)
                ? (State.LateralOffset - State.PreviousLateralOffset) / dt
                : 0f;
            float targetSlip = offsetRate * config.slipAngleScale;

            // Add explicit drift based on Drift Tendency and current curve
            // If turning right (SignedCurvature > 0), drift makes rear step out left (positive slip angle)
            float explicitDrift = State.SignedCurvature * 150f * (personality.DriftTendency / 10f);
            targetSlip += explicitDrift;

            targetSlip = Mathf.Clamp(targetSlip, -config.maxSlipAngleDeg, config.maxSlipAngleDeg);
            State.SlipAngle = Mathf.Lerp(State.SlipAngle, targetSlip, dt * config.slipAngleSmoothRate);

            State.IsDrifting = Mathf.Abs(State.SlipAngle) > 4f;

            // Suspension bob
            float speedNorm = (config.maxSpeed > 0f) ? State.Speed / config.maxSpeed : 0f;
            State.SuspensionOffset = Mathf.Sin(Time.time * config.suspensionBobFrequency * Mathf.Max(speedNorm, 0.1f))
                                     * config.suspensionBobAmplitude * speedNorm;
        }

        // ====================================================================
        // Transform Application
        // ====================================================================

        private void ApplyTransform()
        {
            float t = SplineCurvatureHelper.WrapT(State.T);

            // Evaluate spline in local space, then transform to world
            Vector3 localPos = SplineUtility.EvaluatePosition(spline, t);
            Vector3 localTangent = SplineUtility.EvaluateTangent(spline, t);
            Vector3 localUp = SplineUtility.EvaluateUpVector(spline, t);

            Vector3 worldPos = splineTransform.TransformPoint(localPos);
            Vector3 worldTangent = splineTransform.TransformDirection(localTangent).normalized;
            Vector3 worldUp = splineTransform.TransformDirection(localUp).normalized;
            Vector3 worldRight = Vector3.Cross(worldUp, worldTangent).normalized;

            // Apply lateral offset and suspension bob
            Vector3 finalPos = worldPos
                               + worldRight * State.LateralOffset
                               + worldUp * State.SuspensionOffset;

            // Build rotation: base orientation + body roll + slip angle
            Quaternion baseRot = Quaternion.LookRotation(worldTangent, worldUp);
            Quaternion rollRot = Quaternion.AngleAxis(State.BodyRoll, worldTangent);
            Quaternion slipRot = Quaternion.AngleAxis(State.SlipAngle, worldUp);

            carTransform.position = finalPos;
            carTransform.rotation = slipRot * rollRot * baseRot;
        }

        // ====================================================================
        // Helpers
        // ====================================================================

        private void TickPaused(float dt)
        {
            // Smoothly decelerate to zero when paused
            State.Speed = Mathf.MoveTowards(State.Speed, 0f, config.brakeRate * dt);
            State.IsAccelerating = false;
            State.IsBraking = State.Speed > 0.1f;

            if (State.Speed > 0.01f)
            {
                State.PreviousT = State.T;
                State.PreviousLateralOffset = State.LateralOffset;
                AdvanceT(dt);
                TickVisuals(dt);
                ApplyTransform();
            }
        }

        private void PlaceAtStart()
        {
            State.T = 0f;
            State.Speed = 0f;
            ApplyTransform();
        }
    }
}
