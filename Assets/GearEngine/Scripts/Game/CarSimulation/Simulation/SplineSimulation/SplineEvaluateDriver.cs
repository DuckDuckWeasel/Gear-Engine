using System;
using System.Collections.Generic;
using GearEngine.CarSimulation.Entity;
using GearEngine.CarSimulation.SplineSimulation;
using UnityEngine;
using UnityEngine.Splines;
using VContainer.Unity;

namespace GearEngine.CarSimulation.SplineSimulation
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
            public float DynamicEntryDist;
            public float DynamicExitDist;
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

                        var rb = prometeoComponent.GetComponent<Rigidbody>();
                        if (rb == null) rb = carTransform.GetComponentInChildren<Rigidbody>();
                        if (rb != null)
                        {
                            rb.isKinematic = true;
                        }
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
            float startT = 0f;

            int curveIndex = 0;

            for (float t = 0; t <= 1f; t += step)
            {
                float curvature = SplineCurvatureHelper.SampleCurvatureAt(spline, splineLength, t, out float sign);
                if (curvature > 0.04f)
                {
                    if (!inCurve)
                    {
                        inCurve = true;
                        startT = t;
                    }
                    if (curvature > currentPeak)
                    {
                        currentPeak = curvature;
                        peakT = t;
                        peakSign = Mathf.Sign(sign);
                    }
                }
                else if (inCurve && curvature < 0.02f)
                {
                    AddCurveEvent(peakT, peakSign, curveIndex++, startT, t);
                    inCurve = false;
                    currentPeak = 0f;
                }
            }
            if (inCurve) AddCurveEvent(peakT, peakSign, curveIndex++, startT, 1f);
        }

        private void AddCurveEvent(float t, float sign, int curveIndex, float startT, float endT)
        {
            TrackCurveEvent ev = new TrackCurveEvent();
            ev.T = t;
            ev.Sign = sign;
            ev.CurveIndex = curveIndex;

            // Calculate physical distance from start of curve to apex, and apex to end
            float distStart = t - startT;
            if (distStart < 0f) distStart += 1f;
            
            float distEnd = endT - t;
            if (distEnd < 0f) distEnd += 1f;

            // Apply risk multiplier to lookahead based on CorneringSkill (100 skill = 0 risk = x mult, 0 skill = 100 risk = y mult)
            float riskMult = Mathf.Lerp(config.riskLookaheadMultiplier.x, config.riskLookaheadMultiplier.y, (100f - personality.CorneringSkill) / 100f);
            float effectiveLookahead = config.curvatureLookaheadMeters * riskMult;

            // The preparation distance is relative to the curve's actual entry size + dynamic lookahead based on risk
            ev.DynamicEntryDist = Mathf.Max((distStart * splineLength) + (effectiveLookahead * 0.5f), effectiveLookahead * 0.8f); 
            
            // The exit distance ensures we don't snap back instantly
            ev.DynamicExitDist = Mathf.Max(distEnd * splineLength, effectiveLookahead * 0.2f);

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

        public TrackCurveEvent EvaluateCurveForLap(TrackCurveEvent baseCurve, float currentT, int currentLap)
        {
            int evaluateLap = currentLap;
            if (baseCurve.T < 0.2f && currentT > 0.8f) evaluateLap++;
            else if (baseCurve.T > 0.8f && currentT < 0.2f) evaluateLap--;
            
            int seed = baseCurve.CurveIndex * 1337 + evaluateLap * 73;
            
            // Use Unity's Random generator for high-quality, perfectly deterministic rolls
            UnityEngine.Random.State oldState = UnityEngine.Random.state;
            UnityEngine.Random.InitState(seed);
            float rollPerfect = UnityEngine.Random.value;
            float rollMode = UnityEngine.Random.value;
            float rollDrift = UnityEngine.Random.value;
            UnityEngine.Random.state = oldState;

            float perfectChance = personality.CorneringSkill / 100f;
            
            TrackCurveEvent ev = baseCurve;
            ev.ActiveMode = (rollPerfect <= perfectChance) ? 
                (CurveMode)Mathf.Clamp(Mathf.FloorToInt(rollMode * 5), 0, 4) : 
                (CurveMode)Mathf.Clamp(5 + Mathf.FloorToInt(rollMode * 5), 5, 9);
            
            float drift = personality.Drift / 100f;
            float maxDriftChance = 1.0f; 
            ev.WillDrift = rollDrift <= drift * maxDriftChance;

            return ev;
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
            return EvaluateCurveForLap(best, currentT, currentLap);
        }

        public void CalculateCurveSeverities(float t, TrackCurveEvent activeCurve, out float currentSeverity, out float upcomingSeverity, out float exitSeverity)
        {
            float signedDistToPeak = (activeCurve.T - t);
            if (signedDistToPeak > 0.5f) signedDistToPeak -= 1f;
            if (signedDistToPeak < -0.5f) signedDistToPeak += 1f;
            float distMetersToPeak = signedDistToPeak * splineLength;

            // Calculate ONE time for the entire curve (entry to exit) to prevent overlapping bumps
            float totalRadius = (distMetersToPeak > 0f) ? activeCurve.DynamicEntryDist : activeCurve.DynamicExitDist;
            
            // Normalize the distance based on the physical size of the curve side (1 = edge, 0 = apex)
            float normalizedDist = Mathf.Clamp(Mathf.Abs(distMetersToPeak) / totalRadius, 0f, 1f);
            
            // Calculate a single perfect curve. 1 at the apex, 0 at the extreme edges.
            float baseSeverity = 1f - normalizedDist;
            
            // "Dá um smooth dos 80% pro fim porque tá dando uma batidinha"
            // SmoothStep guarantees that the curve approaches 0 perfectly horizontally, preventing any hard snaps or bumps at the edges!
            currentSeverity = Mathf.SmoothStep(0f, 1f, baseSeverity);

            // Upcoming and exit are just contextual halves of the same perfect curve
            upcomingSeverity = (distMetersToPeak > 0f) ? currentSeverity : 0f;
            exitSeverity = (distMetersToPeak < 0f) ? currentSeverity : 0f;
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
            // Calculate actual max capability based on stat and config limit (50% to 100% of max speed)
            float maxSpeed = Mathf.Lerp(config.maxSpeed * 0.5f, config.maxSpeed, personality.SpeedCapability / 100f);

            // Sample current curvature
            State.Curvature = SplineCurvatureHelper.SampleCurvatureAt(spline, splineLength, State.T, out State.SignedCurvature);
            
            // Get the planned curve and calculate severities
            TrackCurveEvent activeCurve = GetActiveCurve(State.T, State.CompletedLaps);
            CalculateCurveSeverities(State.T, activeCurve, out float currentSeverity, out float upcomingSeverity, out float exitSeverity);
            
            float speedSeverity = Mathf.Max(currentSeverity, upcomingSeverity);
            
            // Perfect curves drop speed smoothly, bottoming out at minCurveSpeed for sharpest curves
            float safeCurveSpeed = Mathf.Max(config.minCurveSpeed, maxSpeed * 0.6f);
            State.TargetSpeed = Mathf.Lerp(maxSpeed, safeCurveSpeed, speedSeverity);
            
            float signedDistToPeak = (activeCurve.T - State.T);
            if (signedDistToPeak > 0.5f) signedDistToPeak -= 1f;
            if (signedDistToPeak < -0.5f) signedDistToPeak += 1f;
            
            if (State.IsInCurveSequence && (int)State.ActiveCurveMode >= 5)
            {
                // Failed curves force a hard brake down to the minimum speed limits!
                State.TargetSpeed = Mathf.Lerp(maxSpeed, config.minCurveSpeed, currentSeverity);
            }

            // ── DELAYED EXIT EFFECTS (PENALTY) ──
            if (signedDistToPeak > 0f && currentSeverity < 0.2f)
            {
                // Entering a new curve (far from apex on the entry side), reset flags!
                State.HasFailedThisCurve = false;
            }

            // Trigger end-of-curve effects during the final 20% of the exit
            bool isAtEndOfExit = State.IsInCurveSequence && signedDistToPeak < 0f && currentSeverity < 0.2f;

            if (isAtEndOfExit)
            {
                bool isFailure = (int)State.ActiveCurveMode >= 5;
                
                if (isFailure && !State.HasFailedThisCurve)
                {
                    // "perder um pouco mais de velocidade" -> 10% speed drop at the physical exit!
                    State.Speed *= 0.90f; 
                    State.HasFailedThisCurve = true;
                }
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

        private float GetRacingLineOffset(float t, TrackCurveEvent curve, float cur)
        {
            float inside = curve.Sign;
            float outsd = -curve.Sign;
            float line = 0f;

            float signedDistToPeak = (curve.T - t);
            if (signedDistToPeak > 0.5f) signedDistToPeak -= 1f;
            if (signedDistToPeak < -0.5f) signedDistToPeak += 1f;
            float distMetersToPeak = signedDistToPeak * splineLength;

            float totalRadius = (distMetersToPeak > 0f) ? curve.DynamicEntryDist : curve.DynamicExitDist;
            // n is +1 at Start(Entry), 0 at Apex, -1 at End(Exit)
            float n = Mathf.Clamp(distMetersToPeak / totalRadius, -1f, 1f);

            switch (curve.ActiveMode)
            {
                // Hug Inside: dives to the inside at the apex.
                case CurveMode.PerfectHugInside: 
                    line = inside * cur; 
                    break;
                
                // Out-In-Out: Starts center, goes heavily outside during entry, dives inside at apex, heavily outside during exit.
                case CurveMode.PerfectOutInOut: 
                    line = inside * cur + outsd * (n * n * cur * 5f); 
                    break;

                // Late Apex: Exaggerated outside line during entry, cuts inside.
                case CurveMode.PerfectLateApex:
                    if (n > 0f) line = outsd * cur * n * 4f;
                    else line = inside * cur * -n * 4f;
                    break;

                // Early Apex: Exaggerated inside dive during entry, drifts outside.
                case CurveMode.PerfectEarlyApex:
                    if (n > 0f) line = inside * cur * n * 4f;
                    else line = outsd * cur * -n * 4f;
                    break;

                case CurveMode.PerfectCenter: 
                    line = 0f; 
                    break;

                // Failed Modes
                case CurveMode.FailedInOutIn: 
                    line = inside * cur + outsd * (n * n * cur * 6f);
                    break;
                
                case CurveMode.FailedHugOutside: 
                    line = outsd * cur * 2f; 
                    break;
                
                case CurveMode.FailedWobble: 
                    // Wobble rapidly between inside and outside heavily
                    line = Mathf.Sin(n * Mathf.PI * 4f) * cur * outsd * 3f; 
                    break;
                
                case CurveMode.FailedBalk: 
                    // Sudden violent jerk outside precisely at the apex
                    line = outsd * Mathf.Pow(cur, 3f) * 3f; 
                    break;
                
                case CurveMode.FailedOvershoot: 
                    // Misses the apex entirely and drifts massively outside during the exit
                    if (n < 0f) line = outsd * cur * -n * 6f;
                    else line = outsd * cur * n * 2f;
                    break;
            }
            return line * config.maxLateralOffset;
        }

        public float GetPredictedLateralOffset(float t, int currentLap)
        {
            if (precalculatedCurves == null || precalculatedCurves.Count == 0) return 0f;
            
            float totalDynamicOffset = 0f;
            foreach (var baseCurve in precalculatedCurves)
            {
                TrackCurveEvent evaluatedCurve = EvaluateCurveForLap(baseCurve, t, currentLap);
                CalculateCurveSeverities(t, evaluatedCurve, out float cur, out float up, out float ex);
                
                float severitySum = cur + up + ex;
                if (severitySum <= 0f) continue;

                float line = GetRacingLineOffset(t, evaluatedCurve, cur);
                
                float lineMultiplier = 1f;
                if ((int)evaluatedCurve.ActiveMode >= 5) 
                {
                    float errorMagnitude = 1f - (personality.Precision / 100f);
                    // "sair um pouquinho mais da curva ao errar"
                    lineMultiplier = 1f + (errorMagnitude * 4f); 
                }
                
                totalDynamicOffset += line * lineMultiplier;
            }

            return totalDynamicOffset;
        }

        private void TickLateralOffset(float dt)
        {
            float t = State.T;
            
            // Blend all overlapping curves smoothly instead of snapping when the closest curve changes
            float totalDynamicOffset = 0f;
            State.IsInCurveSequence = false;
            State.WillDriftCurrentCurve = false;
            
            float maxSeverity = 0f;
            TrackCurveEvent dominantCurve = default;

            foreach (var baseCurve in precalculatedCurves)
            {
                TrackCurveEvent evaluatedCurve = EvaluateCurveForLap(baseCurve, t, State.CompletedLaps);
                CalculateCurveSeverities(t, evaluatedCurve, out float cur, out float up, out float ex);
                
                float severitySum = cur + up + ex;
                if (severitySum <= 0f) continue;

                State.IsInCurveSequence = true;
                
                float line = GetRacingLineOffset(t, evaluatedCurve, cur);
                
                float lineMultiplier = 1f;
                if ((int)evaluatedCurve.ActiveMode >= 5) // Failed modes
                {
                    float errorMagnitude = 1f - (personality.Precision / 100f);
                    // "sair um pouquinho mais da curva ao errar"
                    lineMultiplier = 1f + (errorMagnitude * 4f); 
                }
                
                totalDynamicOffset += line * lineMultiplier;
                
                if (severitySum > maxSeverity)
                {
                    maxSeverity = severitySum;
                    dominantCurve = evaluatedCurve;
                }
            }

            if (State.IsInCurveSequence)
            {
                State.ActiveCurveMode = dominantCurve.ActiveMode;
                State.CurrentCurveSign = dominantCurve.Sign;
                State.WillDriftCurrentCurve = dominantCurve.WillDrift;
            }

            // Smooth the racing line calculation.
            smoothedRacingLine = Mathf.MoveTowards(smoothedRacingLine, totalDynamicOffset, dt * 10f); // Max 10m/s lateral shift

            float rawOffset = smoothedRacingLine;

            // ── Authored Lane Profile ───────────────────────────────────────────
            if (State.IsInCurveSequence && laneProfile != null)
            {
                float aggression = laneProfile.AggressionCurve.Evaluate(t) * 5f;
                float drift = laneProfile.DriftCurve.Evaluate(t) * 5f;
                float width = laneProfile.WidthCurve.Evaluate(t) * 5f;
                float riskEntry = laneProfile.RiskEntryCurve.Evaluate(t) * 5f;
                rawOffset += aggression + drift + width + riskEntry;
            }

            // Clamp the offset. If they failed the curve, allow them to slide up to 50% OUTSIDE the track bounds!
            float maxBounds = config.maxLateralOffset;
            if ((int)State.ActiveCurveMode >= 5) maxBounds *= 1.5f; 

            State.RawLateralOffset = Mathf.Clamp(rawOffset, -maxBounds, maxBounds);

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

            // Trigger IsDrifting purely based on explicit curve drift intent and depth
            // This prevents lateral lane shifts on straightaways from faking a drift
            State.IsDrifting = false;
            if (State.IsInCurveSequence && State.WillDriftCurrentCurve)
            {
                State.IsDrifting = driftSeverityForVFX > 0.05f; 
            }

            // Handle Prometeo VFX via cached reflection fields
            if (hasPrometeoEffects)
            {
                // Drift VFX strictly triggers only when performing a designated curve drift
                bool showDriftVfx = State.IsDrifting && State.Speed > 5f;

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
            float maxSpeed = Mathf.Lerp(config.maxSpeed * 0.5f, config.maxSpeed, personality.SpeedCapability / 100f);
            float speedNorm = State.Speed / maxSpeed;
            float recklessness = 1f - (personality.Smoothness / 100f);
            float bounceMult = 1f + (recklessness * 5f);
            
            State.SuspensionOffset = Mathf.Sin(Time.time * config.suspensionBobFrequency * bounceMult * Mathf.Max(speedNorm, 0.1f))
                                     * config.suspensionBobAmplitude * speedNorm * bounceMult;

            // ── FAILED CURVE VISUALS (VIOLENT SHUDDER) ──
            if (State.IsInCurveSequence && (int)State.ActiveCurveMode >= 5)
            {
                TrackCurveEvent activeCurve = GetActiveCurve(State.T, State.CompletedLaps);
                CalculateCurveSeverities(State.T, activeCurve, out float currentSeverity, out float _, out float _);
                
                // Add violent un-smoothed jitter to instantly readable variables to show loss of control
                State.BodyRoll += Mathf.Sin(Time.time * 60f) * (currentSeverity * 12f);
                State.SuspensionOffset += Mathf.Cos(Time.time * 75f) * (currentSeverity * 0.3f);
                visualSteerAngle += Mathf.Sin(Time.time * 50f) * (currentSeverity * 25f);
            }
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
