using System;
using System.Collections.Generic;
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

        // VFX Reflection cache to bypass Assembly Definition constraints
        private Component prometeoComponent;
        private ParticleSystem rlParticle;
        private ParticleSystem rrParticle;
        private TrailRenderer rlSkid;
        private TrailRenderer rrSkid;
        private Transform frontLeftWheelTr;
        private Transform frontRightWheelTr;
        private Transform rearLeftWheelTr;
        private Transform rearRightWheelTr;
        private Transform steeringWheelTr;
        private bool hasPrometeoEffects;
        private float smoothedRacingLine; // Prevents the racing line from jumping
        private float visualSteerAngle; // Tracks the current steering wheel angle

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

        public struct TrackCurveEvent
        {
            public float T;
            public float Sign;
            public int CurveIndex;
            public CurveMode ActiveMode;
            public bool WillDrift;
        }

        private List<TrackCurveEvent> precalculatedCurves = new List<TrackCurveEvent>();

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

            State.PreviousT = State.T;
            State.IsDrifting = false;

            // Attempt to grab Prometeo via reflection to reuse its VFX setup without Assembly dependencies
            if (carTransform != null)
            {
                System.Type prometeoType = null;
                foreach (var comp in carTransform.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (comp != null && comp.GetType().Name == "PrometeoCarController")
                    {
                        prometeoComponent = comp;
                        prometeoType = comp.GetType();
                        break;
                    }
                }

                if (prometeoType != null && prometeoComponent != null)
                {
                        // Ignore the 'useEffects' boolean on the original script, we force VFX if the references exist
                        hasPrometeoEffects = true;
                        rlParticle = prometeoType.GetField("RLWParticleSystem")?.GetValue(prometeoComponent) as ParticleSystem;
                        rrParticle = prometeoType.GetField("RRWParticleSystem")?.GetValue(prometeoComponent) as ParticleSystem;
                        rlSkid = prometeoType.GetField("RLWTireSkid")?.GetValue(prometeoComponent) as TrailRenderer;
                        rrSkid = prometeoType.GetField("RRWTireSkid")?.GetValue(prometeoComponent) as TrailRenderer;

                        var flObj = prometeoType.GetField("frontLeftMesh")?.GetValue(prometeoComponent) as GameObject;
                        if (flObj != null) frontLeftWheelTr = flObj.transform;
                        
                        var frObj = prometeoType.GetField("frontRightMesh")?.GetValue(prometeoComponent) as GameObject;
                        if (frObj != null) frontRightWheelTr = frObj.transform;

                        var rlObj = prometeoType.GetField("rearLeftMesh")?.GetValue(prometeoComponent) as GameObject;
                        if (rlObj != null) rearLeftWheelTr = rlObj.transform;
                        
                        var rrObj = prometeoType.GetField("rearRightMesh")?.GetValue(prometeoComponent) as GameObject;
                        if (rrObj != null) rearRightWheelTr = rrObj.transform;
                        
                        var swObj = prometeoType.GetField("steeringWheel")?.GetValue(prometeoComponent) as GameObject;
                        if (swObj != null) steeringWheelTr = swObj.transform;

                        // Disable Prometeo so it stops fighting our VFX and wheel rotations!
                        var prometeoBehaviour = prometeoComponent as Behaviour;
                        if (prometeoBehaviour != null) prometeoBehaviour.enabled = false;
                    }
                }

                isInitialized = true;
            isPaused = false;
            
            GenerateTrackPlan();
        }
        /// <summary>Updates the personality stats at runtime (e.g. from UI sliders).</summary>
        public void SetPersonality(DriverPersonality newPersonality)
        {
            personality = newPersonality;
            GenerateTrackPlan(); // Regenerate curve strategies based on new personality
        }

        private void GenerateTrackPlan()
        {
            precalculatedCurves.Clear();
            if (splineLength <= 0f) return;

            float step = 0.005f;
            bool inCurve = false;
            float currentPeak = 0f;
            float peakT = 0f;
            float peakSign = 1f;

            int curveIndex = 0;

            for (float t = 0; t <= 1f; t += step)
            {
                float curvature = SplineCurvatureHelper.SampleCurvatureAt(spline, splineLength, t, out float sign);
                if (curvature > 0.04f)
                {
                    inCurve = true;
                    if (curvature > currentPeak)
                    {
                        currentPeak = curvature;
                        peakT = t;
                        peakSign = Mathf.Sign(sign);
                    }
                }
                else if (inCurve && curvature < 0.02f)
                {
                    AddCurveEvent(peakT, peakSign, curveIndex++);
                    inCurve = false;
                    currentPeak = 0f;
                }
            }
            if (inCurve) AddCurveEvent(peakT, peakSign, curveIndex++);
        }

        private void AddCurveEvent(float t, float sign, int curveIndex)
        {
            TrackCurveEvent ev = new TrackCurveEvent();
            ev.T = t;
            ev.Sign = sign;
            ev.CurveIndex = curveIndex;
            precalculatedCurves.Add(ev);
        }

        private float Hash(int value)
        {
            // Wang hash to properly avalanche small sequential integers
            uint hash = (uint)value;
            hash = (hash ^ 61) ^ (hash >> 16);
            hash = hash + (hash << 3);
            hash = hash ^ (hash >> 4);
            hash = hash * 0x27d4eb2d;
            hash = hash ^ (hash >> 15);
            return (hash % 10000) / 10000f;
        }

        public TrackCurveEvent GetActiveCurve(float currentT, int currentLap)
        {
            if (precalculatedCurves.Count == 0) return default;
            
            TrackCurveEvent best = precalculatedCurves[0];
            float minDist = float.MaxValue;
            foreach (var c in precalculatedCurves)
            {
                float dist = Mathf.Abs(c.T - currentT);
                if (dist > 0.5f) dist = 1f - dist; // wrap around loop
                if (dist < minDist)
                {
                    minDist = dist;
                    best = c;
                }
            }
            // Evaluate Entry and Exit modes deterministically based on Lap and CurveIndex
            int evaluateLap = currentLap;
            // If the curve peak is across the start/finish line relative to our currentT
            if (best.T < 0.2f && currentT > 0.8f) evaluateLap++;
            else if (best.T > 0.8f && currentT < 0.2f) evaluateLap--;
            
            int seed = evaluateLap * 1000 + best.CurveIndex;
            float rollPerfect = Hash(seed + 1);
            float rollMode = Hash(seed + 2);
            float rollDrift = Hash(seed + 3);

            float perfectChance = personality.CorneringSkill / 10f;
            
            // Randomly select one of the 5 Perfect modes or 5 Failed modes
            best.ActiveMode = (rollPerfect <= perfectChance) ? 
                (CurveMode)Mathf.Clamp(Mathf.FloorToInt(rollMode * 5), 0, 4) : 
                (CurveMode)Mathf.Clamp(5 + Mathf.FloorToInt(rollMode * 5), 5, 9);
            
            float traction = personality.Traction / 10f;
            float maxDriftChance = 0.4f;
            best.WillDrift = rollDrift <= (1f - traction) * maxDriftChance;

            return best;
        }

        public void CalculateCurveSeverities(float t, TrackCurveEvent activeCurve, out float currentSeverity, out float upcomingSeverity, out float exitSeverity)
        {
            float signedDistToPeak = (activeCurve.T - t);
            if (signedDistToPeak > 0.5f) signedDistToPeak -= 1f;
            if (signedDistToPeak < -0.5f) signedDistToPeak += 1f;
            float distMetersToPeak = signedDistToPeak * splineLength;

            float setupDistance = 20f;
            float currentSpread = 10f;

            // currentSeverity: Peaks at 0m. Starts at currentSpread, ends at -currentSpread.
            currentSeverity = Mathf.InverseLerp(currentSpread, 0f, Mathf.Abs(distMetersToPeak));

            upcomingSeverity = 0f;
            exitSeverity = 0f;

            if (distMetersToPeak > 0f)
            {
                float halfSetup = setupDistance * 0.5f;
                if (distMetersToPeak > halfSetup)
                    upcomingSeverity = Mathf.InverseLerp(setupDistance, halfSetup, distMetersToPeak);
                else
                    upcomingSeverity = Mathf.InverseLerp(0f, halfSetup, distMetersToPeak);
            }
            else
            {
                float halfSetup = -setupDistance * 0.5f;
                if (distMetersToPeak > halfSetup)
                    exitSeverity = Mathf.InverseLerp(0f, halfSetup, distMetersToPeak);
                else
                    exitSeverity = Mathf.InverseLerp(-setupDistance, halfSetup, distMetersToPeak);
            }
            
            // Apply smoothstep to remove sharp linear peaks and guarantee zero squiggles
            currentSeverity = Mathf.SmoothStep(0f, 1f, currentSeverity);
            upcomingSeverity = Mathf.SmoothStep(0f, 1f, upcomingSeverity);
            exitSeverity = Mathf.SmoothStep(0f, 1f, exitSeverity);
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
            ApplyTransform(dt);
        }

        // ====================================================================
        // Speed Model (M3)
        // ====================================================================

        private void TickSpeedModel(float dt)
        {
            float maxSpeed = Mathf.Lerp(10f, 200f, personality.SpeedCapability / 10f);

            // Sample current curvature
            State.Curvature = SplineCurvatureHelper.SampleCurvatureAt(spline, splineLength, State.T, out State.SignedCurvature);
            
            // Get the planned curve and calculate severities
            TrackCurveEvent activeCurve = GetActiveCurve(State.T, State.CompletedLaps);
            CalculateCurveSeverities(State.T, activeCurve, out float currentSeverity, out float upcomingSeverity, out float exitSeverity);
            
            // The speed limit is dictated by how deep we are into the curve OR how close we are to entering it!
            float speedSeverity = Mathf.Max(currentSeverity, upcomingSeverity);
            
            // Map curvature to speed cap. We heavily penalize the minCurveSpeed so the slowdown is very noticeable.
            float noticeableMinSpeed = config.minCurveSpeed * 0.5f; // Drop to 50% of the config's min speed!
            State.TargetSpeed = Mathf.Lerp(maxSpeed, noticeableMinSpeed, speedSeverity);

            // Penalize speed if we are in a failed curve mode (loss of control/momentum)
            if (State.IsInCurveSequence && (int)State.ActiveCurveMode >= 5)
            {
                float errorMagnitude = 1f - (personality.Precision / 10f);
                float speedPenalty = Mathf.Lerp(1f, 0.45f, errorMagnitude); // Lose up to 55% of target speed if precision is 0
                State.TargetSpeed *= speedPenalty;
            }

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

        private float GetRacingLineOffset(float t, TrackCurveEvent curve)
        {
            CalculateCurveSeverities(t, curve, out float cur, out float up, out float ex);
            float inside = curve.Sign;
            float outsd = -curve.Sign;
            float line = 0f;

            switch (curve.ActiveMode)
            {
                case CurveMode.PerfectOutInOut: line = outsd * up + inside * cur + outsd * ex; break;
                case CurveMode.PerfectLateApex: line = outsd * up + inside * cur; break;
                case CurveMode.PerfectEarlyApex: line = inside * cur + outsd * ex; break;
                case CurveMode.PerfectCenter: line = 0f; break;
                case CurveMode.PerfectHugInside: line = inside * (up * 0.5f + cur + ex * 0.5f); break;

                case CurveMode.FailedInOutIn: line = inside * up + outsd * cur; break;
                case CurveMode.FailedHugOutside: line = outsd * (up * 0.5f + cur + ex * 0.5f); break;
                case CurveMode.FailedWobble: line = outsd * up + inside * cur; break;
                case CurveMode.FailedBalk: line = outsd * cur; break;
                case CurveMode.FailedOvershoot: line = outsd * (cur + ex); break;
            }
            return line * config.maxLateralOffset;
        }

        private void TickLateralOffset(float dt)
        {
            float t = State.T;
            float rawOffset = 0f;

            TrackCurveEvent activeCurve = GetActiveCurve(t, State.CompletedLaps);
            State.ActiveCurveMode = activeCurve.ActiveMode;
            State.CurrentCurveSign = activeCurve.Sign;
            State.WillDriftCurrentCurve = activeCurve.WillDrift;
            
            CalculateCurveSeverities(t, activeCurve, out float currentSeverity, out float upcomingSeverity, out float exitSeverity);
            State.IsInCurveSequence = (currentSeverity > 0f || upcomingSeverity > 0f || exitSeverity > 0f);

            float dynamicRacingLineOffset = 0f;

            if (State.IsInCurveSequence)
            {
                dynamicRacingLineOffset = GetRacingLineOffset(t, activeCurve);
            }

            // Scale the line magnitude
            // If it's a perfect curve, we execute it precisely (multiplier 1.0)
            // If it's a failed curve, we scale the severity of the mistake by how low Precision is.
            float lineMultiplier = 1f;
            if ((int)State.ActiveCurveMode >= 5) // Failed modes
            {
                float errorMagnitude = 1f - (personality.Precision / 10f);
                // Exaggerate the visual error by up to 2.5x to make the mistake extremely visible
                lineMultiplier = errorMagnitude * 2.5f;
            }

            // Smooth the racing line calculation.
            // Even if the threshold triggers late and dynamicRacingLineOffset jumps,
            // this MoveTowards will ramp it up naturally over ~0.2s, simulating the driver steering in.
            smoothedRacingLine = Mathf.MoveTowards(smoothedRacingLine, dynamicRacingLineOffset, dt * 10f); // Max 10m/s lateral shift

            rawOffset = smoothedRacingLine * lineMultiplier;

            // ── Authored Lane Profile ───────────────────────────────────────────
            if (State.IsInCurveSequence && laneProfile != null)
            {
                float aggression = laneProfile.AggressionCurve.Evaluate(t) * 5f;
                float drift = laneProfile.DriftCurve.Evaluate(t) * 5f;
                float width = laneProfile.WidthCurve.Evaluate(t) * 5f;
                float riskEntry = laneProfile.RiskEntryCurve.Evaluate(t) * 5f;
                rawOffset += aggression + drift + width + riskEntry;
            }

            State.RawLateralOffset = Mathf.Clamp(rawOffset, -config.maxLateralOffset, config.maxLateralOffset);

            float targetSmoothRate = config.lateralSmoothRate;
            if (!State.IsInCurveSequence)
            {
                // When we are on a straight, we recover much more slowly (85% slower) for a very smooth and natural recentering
                targetSmoothRate = config.lateralSmoothRate * 0.15f; 
            }
            else
            {
                // We use a high smooth rate so the car stays pinned exactly on the evaluated trajectory.
                // The car's visual rotation (slip angle) will compensate for rapid lateral movements.
                targetSmoothRate = Mathf.Max(config.lateralSmoothRate, 15f);
            }

            State.LateralOffset = Mathf.Lerp(State.LateralOffset, State.RawLateralOffset, dt * targetSmoothRate);
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
                
            // Calculate the true velocity heading angle relative to the spline tangent
            float velocityAngle = Mathf.Atan2(offsetRate, Mathf.Max(State.Speed, 5f)) * Mathf.Rad2Deg;
            
            // Apply natural heading + config scaling
            float targetSlip = velocityAngle * config.slipAngleScale;

            float maxAllowedSlip = config.maxSlipAngleDeg;
            float driftSeverityForVFX = 0f;

            if (State.IsInCurveSequence)
            {
                TrackCurveEvent activeCurve = GetActiveCurve(State.T, State.CompletedLaps);
                CalculateCurveSeverities(State.T, activeCurve, out float currentSeverity, out float upcomingSeverity, out float exitSeverity);
                
                // ── NATURAL CURVE INCLINATION ──
                // Naturally leans the car into the corner even when not shifting laterally or explicitly drifting.
                // Gives the visual impression of "taking a curve".
                targetSlip += activeCurve.Sign * (currentSeverity + upcomingSeverity * 0.5f) * 15f; 

                // ── EXPLICIT DRIFT ──
                if (State.WillDriftCurrentCurve)
                {
                    // Maximize severity so the drift angle sets up BEFORE the apex.
                    driftSeverityForVFX = Mathf.Max(currentSeverity, upcomingSeverity);
                    
                    float explicitDrift = activeCurve.Sign * driftSeverityForVFX * 45f;
                    targetSlip += explicitDrift;
                    
                    maxAllowedSlip = 60f;
                }
            }

            targetSlip = Mathf.Clamp(targetSlip, -maxAllowedSlip, maxAllowedSlip);
            State.SlipAngle = Mathf.Lerp(State.SlipAngle, targetSlip, dt * config.slipAngleSmoothRate);

            // Trigger IsDrifting based on severity (which triggers early in the setup distance)
            // instead of waiting for the smoothed SlipAngle to catch up, fixing the "late VFX" issue.
            State.IsDrifting = Mathf.Abs(State.SlipAngle) > 2f || driftSeverityForVFX > 0.2f;

            // Handle Prometeo VFX via cached reflection fields
            if (hasPrometeoEffects)
            {
                // Drift VFX triggers if slip angle is high enough, or if we are in a wobble/drift trajectory
                bool isLateralDrift = Mathf.Abs(velocityAngle) > 1.5f;
                bool showDriftVfx = (State.IsDrifting || isLateralDrift) && State.Speed > 5f;
                
                // Particle Systems
                if (rlParticle != null)
                {
                    var em = rlParticle.emission;
                    if (showDriftVfx) { if (!rlParticle.isPlaying) rlParticle.Play(); em.enabled = true; }
                    else { if (rlParticle.isPlaying) rlParticle.Stop(); em.enabled = false; }
                }
                if (rrParticle != null)
                {
                    var em = rrParticle.emission;
                    if (showDriftVfx) { if (!rrParticle.isPlaying) rrParticle.Play(); em.enabled = true; }
                    else { if (rrParticle.isPlaying) rrParticle.Stop(); em.enabled = false; }
                }

                // Skid Trails
                if (rlSkid != null) rlSkid.emitting = showDriftVfx;
                if (rrSkid != null) rrSkid.emitting = showDriftVfx;
            }

            // Calculate Steer Angle (Visual Only)
            float targetSteerAngle = 0f;
            if (State.IsInCurveSequence)
            {
                // Normal steering into curve
                targetSteerAngle = State.SignedCurvature * 200f;
                
                // If drifting, counter-steer! The wheels must point opposite to the car's body yaw (SlipAngle).
                if (State.IsDrifting && State.WillDriftCurrentCurve)
                {
                    // Counter-steer aligns wheels with the velocity tangent by turning inverse to the body yaw.
                    targetSteerAngle = -State.SlipAngle * 0.85f; 
                }
            }
            
            targetSteerAngle = Mathf.Clamp(targetSteerAngle, -45f, 45f);
            visualSteerAngle = Mathf.Lerp(visualSteerAngle, targetSteerAngle, dt * 15f);

            // Manual wheel rolling (since we disabled Prometeo)
            float wheelRadius = 0.35f;
            float wheelCircumference = 2f * Mathf.PI * wheelRadius;
            float distanceCovered = State.Speed * dt;
            float spinAngle = (distanceCovered / wheelCircumference) * 360f;

            // Apply steering rotation to meshes while preserving their X-axis spinning
            if (frontLeftWheelTr != null)
            {
                Vector3 euler = frontLeftWheelTr.localEulerAngles;
                frontLeftWheelTr.localRotation = Quaternion.Euler(euler.x + spinAngle, visualSteerAngle, euler.z);
            }
            if (frontRightWheelTr != null)
            {
                Vector3 euler = frontRightWheelTr.localEulerAngles;
                frontRightWheelTr.localRotation = Quaternion.Euler(euler.x + spinAngle, visualSteerAngle, euler.z);
            }
            if (rearLeftWheelTr != null)
            {
                Vector3 euler = rearLeftWheelTr.localEulerAngles;
                rearLeftWheelTr.localRotation = Quaternion.Euler(euler.x + spinAngle, 0f, euler.z);
            }
            if (rearRightWheelTr != null)
            {
                Vector3 euler = rearRightWheelTr.localEulerAngles;
                rearRightWheelTr.localRotation = Quaternion.Euler(euler.x + spinAngle, 0f, euler.z);
            }
            if (steeringWheelTr != null)
            {
                Vector3 euler = steeringWheelTr.localEulerAngles;
                // The steering wheel rotates on the Z axis usually, and rotates much more than the actual wheels
                steeringWheelTr.localRotation = Quaternion.Euler(euler.x, euler.y, visualSteerAngle * -4f);
            }

            // Suspension bob (amplified by lack of Smoothness)
            float maxSpeed = Mathf.Lerp(10f, 200f, personality.SpeedCapability / 10f);
            float speedNorm = State.Speed / maxSpeed;
            float recklessness = 1f - (personality.Smoothness / 10f);
            float bounceMult = 1f + (recklessness * 5f);
            
            State.SuspensionOffset = Mathf.Sin(Time.time * config.suspensionBobFrequency * bounceMult * Mathf.Max(speedNorm, 0.1f))
                                     * config.suspensionBobAmplitude * speedNorm * bounceMult;
        }

        // ====================================================================
        // Transform Application
        // ====================================================================

        private void ApplyTransform(float dt)
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
            Quaternion targetRot = slipRot * rollRot * baseRot;

            carTransform.position = finalPos;
            
            // Smoothly interpolate rotation to prevent instant snapping on poorly constructed, sharp spline knots
            if (dt > 0f && Time.timeScale > 0f)
            {
                carTransform.rotation = Quaternion.Slerp(carTransform.rotation, targetRot, dt * 20f);
            }
            else
            {
                carTransform.rotation = targetRot;
            }
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
                ApplyTransform(dt);
            }
        }

        private void PlaceAtStart()
        {
            State.T = 0f;
            State.Speed = 0f;
            ApplyTransform(0f);
        }
    }
}
