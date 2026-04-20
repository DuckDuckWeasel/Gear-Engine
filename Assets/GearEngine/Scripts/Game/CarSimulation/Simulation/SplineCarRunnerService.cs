using System;
using System.Collections.Generic;
using System.Reflection;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Entity;
using UnityEngine;
using UnityEngine.Splines;
using VContainer.Unity;

namespace GearEngine.CarSimulation.Simulation
{
    public class SplineCarRunnerService : ITickable
    {
        public event Action<CarEntity> OnLapCompleted;

        private readonly SplineCarRunnerConfigSO config;
        private readonly List<SplineCarRunnerContext> activeRunners = new List<SplineCarRunnerContext>();

        public SplineCarRunnerService(SplineCarRunnerConfigSO config)
        {
            this.config = config;
        }

        public void SetPaused(CarEntity entity, bool paused)
        {
            SplineCarRunnerContext ctx = activeRunners.Find(c => c.entity == entity);
            if (ctx != null)
            {
                ctx.isPaused = paused;
            }
        }

        public void InitializeRun(PrometeoCarController car, SplineContainer trackContainer, RoguelikeCarStats stats, GearEngine.CarSimulation.Entity.CarEntity entity)
        {
            if (trackContainer == null || trackContainer.Spline.Count == 0 || car == null || entity == null)
            {
                Debug.LogError("[SplineCarRunnerService] Missing parameters on initialization.");
                return;
            }

            SplineCarRunnerContext ctx = new SplineCarRunnerContext
            {
                entity = entity,
                track = trackContainer,
                targetCar = car,
                targetCarRb = car.GetComponent<Rigidbody>(),
                splineLength = trackContainer.Spline.GetLength(),
                upcomingWaypoints = new Vector3[config.waypointCount],
                Variables = config.variables,
                sourceStats = stats
            };

            CarAreaSensor sensor = car.gameObject.AddComponent<CarAreaSensor>();
            sensor.service = this;
            sensor.car = car;
            
            EvaluateProgressionStats(ctx);
            SetupPrometeoTouchOverride(ctx);
            ApplyPhysicalStatsToCar(ctx);
            
            activeRunners.Add(ctx);

            if (config.variables != null)
            {
                SetEntityVariable(entity, config.variables.Speed, stats.statTopSpeed);
                SetEntityVariable(entity, config.variables.Acceleration, stats.statAcceleration);
                SetEntityVariable(entity, config.variables.Handling, stats.statSteeringGrip);
                SetEntityVariable(entity, config.variables.Stability, stats.statRacingLine);
                SetEntityVariable(entity, config.variables.Recovery, stats.statDriverReflexes);
                SetEntityVariable(entity, config.variables.DriftPenalty, stats.statDriftControl);

                Action<Scaffold.Entities.VariableValue> onVarChanged = _ => ReevaluateStats(ctx);
                entity.Subscribe(config.variables.Speed, onVarChanged);
                entity.Subscribe(config.variables.Acceleration, onVarChanged);
                entity.Subscribe(config.variables.Handling, onVarChanged);
                entity.Subscribe(config.variables.Stability, onVarChanged);
                entity.Subscribe(config.variables.Recovery, onVarChanged);
                entity.Subscribe(config.variables.DriftPenalty, onVarChanged);
            }
        }

        private void SetEntityVariable(CarEntity entity, Scaffold.Entities.VariableSO variable, float value)
        {
            if (variable == null) return;
            entity.AddVariable(variable, new Scaffold.Entities.FloatVariableValue { Value = value });
        }

        public void ReevaluateStats(SplineCarRunnerContext ctx)
        {
            if (ctx == null || ctx.targetCar == null || ctx.entity == null) return;
            EvaluateProgressionStats(ctx);
            ApplyPhysicalStatsToCar(ctx);
        }

        private void EvaluateProgressionStats(SplineCarRunnerContext ctx)
        {
            if (config == null || ctx.entity == null) return;

            RoguelikeCarStats baseStats = ctx.sourceStats;
            float s = baseStats.statTopSpeed;
            float a = baseStats.statAcceleration;
            float h = baseStats.statSteeringGrip;
            float st = baseStats.statRacingLine;
            float r = baseStats.statDriverReflexes;
            float d = baseStats.statDriftControl;
            
            if (ctx.Variables != null)
            {
                ctx.entity.TryGetValue(ctx.Variables.Speed, out s);
                ctx.entity.TryGetValue(ctx.Variables.Acceleration, out a);
                ctx.entity.TryGetValue(ctx.Variables.Handling, out h);
                ctx.entity.TryGetValue(ctx.Variables.Stability, out st);
                ctx.entity.TryGetValue(ctx.Variables.Recovery, out r);
                ctx.entity.TryGetValue(ctx.Variables.DriftPenalty, out d);
            }

            float normTopSpeed = s / 100f;
            float normAcceleration = a / 100f;
            float normBrakingSystem = baseStats.statBrakingSystem / 100f; // Braking handles independently for now
            float normDriftControl = d / 100f;
            float normNitrousBoost = baseStats.statNitrousBoost / 100f;
            float normSteeringGrip = h / 100f;
            float normRacingLine = st / 100f;
            float normDriverReflexes = r / 100f;

            float effectiveSimulationStat = Mathf.Clamp01(normNitrousBoost + (normTopSpeed * 0.3f));
            ctx.currentSimulationMultiplier = Mathf.Lerp(1f, config.baseSimulationMultiplier, effectiveSimulationStat);

            ctx.curveAngleThreshold = Mathf.Lerp(config.boundsCurveAngle.x, config.boundsCurveAngle.y, normDriverReflexes);
            ctx.macroCurveAngleThreshold = Mathf.Lerp(config.boundsMacroCurveAngle.x, config.boundsMacroCurveAngle.y, normDriverReflexes);
            ctx.handbrakeAngleThreshold = Mathf.Lerp(config.boundsHandbrakeAngle.x, config.boundsHandbrakeAngle.y, normDriverReflexes);
            ctx.hairpinAngleThreshold = Mathf.Lerp(config.boundsHairpinAngle.x, config.boundsHairpinAngle.y, normDriverReflexes);
            
            ctx.baseWaypointDistance = Mathf.Lerp(config.boundsBaseWaypointDist.x, config.boundsBaseWaypointDist.y, normDriverReflexes);
            ctx.distanceSpeedMultiplier = Mathf.Lerp(config.boundsDistSpeedMult.x, config.boundsDistSpeedMult.y, normDriverReflexes);
            ctx.waypointArrivalRangeBase = Mathf.Lerp(config.boundsArrivalRangeBase.x, config.boundsArrivalRangeBase.y, normDriverReflexes);
            ctx.waypointArrivalSpeedMultiplier = Mathf.Lerp(config.boundsArrivalSpeedMult.x, config.boundsArrivalSpeedMult.y, normDriverReflexes);

            ctx.preCurveWideOffset = Mathf.Lerp(config.boundsPreCurveOffset.x, config.boundsPreCurveOffset.y, normRacingLine);
            ctx.postCurveWideOffset = Mathf.Lerp(config.boundsPostCurveOffset.x, config.boundsPostCurveOffset.y, normRacingLine);

            ctx.steerDeadzone = Mathf.Lerp(config.boundsSteerDeadzone.x, config.boundsSteerDeadzone.y, normSteeringGrip);
            ctx.safeCornerSpeed = Mathf.Lerp(config.boundsSafeCornerSpeed.x, config.boundsSafeCornerSpeed.y, normSteeringGrip) * ctx.currentSimulationMultiplier;
            ctx.arcadeSteerAssist = Mathf.Lerp(config.boundsArcadeSteerAssist.x, config.boundsArcadeSteerAssist.y, normSteeringGrip) * ctx.currentSimulationMultiplier;

            ctx.driftAccelerationMultiplier = Mathf.Lerp(config.boundsDriftAccMult.x, config.boundsDriftAccMult.y, normNitrousBoost);
            ctx.hairpinAccelerationBoost = Mathf.Lerp(config.boundsHairpinAccBoost.x, config.boundsHairpinAccBoost.y, normNitrousBoost);
            
            ctx.driftSteerAssistMultiplier = Mathf.Lerp(config.boundsDriftSteerMult.x, config.boundsDriftSteerMult.y, normDriftControl);
            ctx.hairpinSteerAssistBoost = Mathf.Lerp(config.boundsHairpinSteerBoost.x, config.boundsHairpinSteerBoost.y, normDriftControl);

            ctx.calculatedDriftGrip = Mathf.RoundToInt(Mathf.Lerp(1f, 10f, normDriftControl));
            float minSpeedLimit = config.baseMaxSpeed * config.minSpeedPercentage;
            ctx.maxSimulationSpeed = Mathf.Lerp(minSpeedLimit, config.baseMaxSpeed, normTopSpeed) * ctx.currentSimulationMultiplier;
            ctx.calculatedAcceleration = Mathf.RoundToInt(Mathf.Lerp(3f, config.baseAcceleration, normAcceleration) * ctx.currentSimulationMultiplier);
            ctx.calculatedBrakeForce = Mathf.RoundToInt(Mathf.Lerp(100f, config.baseBrakeForce, normBrakingSystem) * ctx.currentSimulationMultiplier);
        }

        private void SetupPrometeoTouchOverride(SplineCarRunnerContext ctx)
        {
            ctx.aiThrottle = CreateDummyInput("AI_Throttle", ctx);
            ctx.aiReverse  = CreateDummyInput("AI_Reverse", ctx);
            ctx.aiLeft     = CreateDummyInput("AI_Left", ctx);
            ctx.aiRight    = CreateDummyInput("AI_Right", ctx);
            ctx.aiBrake    = CreateDummyInput("AI_Brake", ctx);

            ctx.targetCar.useTouchControls = true;

            Type type = typeof(PrometeoCarController);
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

            type.GetField("touchControlsSetup", flags)?.SetValue(ctx.targetCar, true);
            type.GetField("throttlePTI", flags)?.SetValue(ctx.targetCar, ctx.aiThrottle);
            type.GetField("reversePTI", flags)?.SetValue(ctx.targetCar, ctx.aiReverse);
            type.GetField("turnLeftPTI", flags)?.SetValue(ctx.targetCar, ctx.aiLeft);
            type.GetField("turnRightPTI", flags)?.SetValue(ctx.targetCar, ctx.aiRight);
            type.GetField("handbrakePTI", flags)?.SetValue(ctx.targetCar, ctx.aiBrake);
        }

        private PrometeoTouchInput CreateDummyInput(string name, SplineCarRunnerContext ctx)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(ctx.targetCar.transform);
            go.AddComponent<RectTransform>(); 
            return go.AddComponent<PrometeoTouchInput>();
        }

        private void ApplyPhysicalStatsToCar(SplineCarRunnerContext ctx)
        {
            ctx.targetCar.handbrakeDriftMultiplier = ctx.calculatedDriftGrip;
            ctx.targetCar.maxSpeed = Mathf.RoundToInt(ctx.maxSimulationSpeed);
            ctx.targetCar.accelerationMultiplier = ctx.calculatedAcceleration;
            ctx.targetCar.brakeForce = ctx.calculatedBrakeForce;
        }

        public void AddAreaModifier(PrometeoCarController car, CarAreaModifier modifier)
        {
            SplineCarRunnerContext ctx = activeRunners.Find(c => c.targetCar == car);
            if (ctx != null && !ctx.activeAreaModifiers.Contains(modifier))
            {
                ctx.activeAreaModifiers.Add(modifier);
            }
        }

        public void RemoveAreaModifier(PrometeoCarController car, CarAreaModifier modifier)
        {
            SplineCarRunnerContext ctx = activeRunners.Find(c => c.targetCar == car);
            if (ctx != null)
            {
                ctx.activeAreaModifiers.Remove(modifier);
            }
        }

        public bool GetTelemetry(GearEngine.CarSimulation.Entity.CarEntity entity, out CarTelemetryData data)
        {
            SplineCarRunnerContext ctx = activeRunners.Find(c => c.entity == entity);
            if (ctx != null)
            {
                data = new CarTelemetryData
                {
                    Speed = ctx.currentSpeed,
                    Progress = ctx.previousProgressPercent,
                    IsBraking = ctx.aiBrake.buttonPressed || ctx.aiReverse.buttonPressed,
                    IsDrifting = ctx.targetCar.isDrifting,
                    IsAccelerating = ctx.aiThrottle.buttonPressed,
                    CurrentAcceleration = ctx.targetCar.accelerationMultiplier
                };
                return true;
            }
            data = default;
            return false;
        }

        public bool GetDebugTelemetry(GearEngine.CarSimulation.Entity.CarEntity entity, out Vector3[] waypoints, out bool requiresHandbrake, out bool isBrakingForCurve, out Transform carTransform)
        {
            SplineCarRunnerContext ctx = activeRunners.Find(c => c.entity == entity);
            if (ctx != null && ctx.upcomingWaypoints != null)
            {
                waypoints = ctx.upcomingWaypoints;
                requiresHandbrake = ctx.requiresHandbrake;
                isBrakingForCurve = ctx.isBrakingForCurve;
                carTransform = ctx.targetCar?.transform;
                return true;
            }
            waypoints = null;
            requiresHandbrake = false;
            isBrakingForCurve = false;
            carTransform = null;
            return false;
        }

        public SplineCarRunnerContext GetDebugContext(GearEngine.CarSimulation.Entity.CarEntity entity)
        {
            return activeRunners.Find(c => c.entity == entity);
        }

        public void Tick()
        {
            for (int i = activeRunners.Count - 1; i >= 0; i--)
            {
                SplineCarRunnerContext ctx = activeRunners[i];
                if (ctx.targetCar == null || ctx.track == null)
                {
                    activeRunners.RemoveAt(i);
                    continue;
                }
                TickRunner(ctx);
            }
        }

        private void TickRunner(SplineCarRunnerContext ctx)
        {
            if (ctx.isPaused)
            {
                if (ctx.targetCarRb != null && ctx.targetCarRb.linearVelocity.magnitude > 0.1f)
                {
                    ctx.targetCarRb.linearVelocity = Vector3.Lerp(ctx.targetCarRb.linearVelocity, Vector3.zero, Time.deltaTime * 5f);
                }
                ctx.aiBrake.buttonPressed = true;
                ctx.aiThrottle.buttonPressed = false;
                ctx.targetCar.maxSpeed = 0;
                return;
            }

            float activeTempSpeedMult = 1f;
            float tempAccMult = 1f;
            float tempGripMult = 1f;

            ctx.activeAreaModifiers.RemoveAll(m => m == null);
            for (int i = 0; i < ctx.activeAreaModifiers.Count; i++)
            {
                activeTempSpeedMult *= ctx.activeAreaModifiers[i].speedMultiplier;
                tempAccMult *= ctx.activeAreaModifiers[i].accelerationMultiplier;
                tempGripMult *= ctx.activeAreaModifiers[i].splineGripMultiplier;
            }

            float effectiveMaxSimulationSpeed = ctx.maxSimulationSpeed * activeTempSpeedMult;
            ctx.targetCar.maxSpeed = Mathf.RoundToInt(effectiveMaxSimulationSpeed);

            if (ctx.targetCar.isDrifting) {
                float boost = ctx.isHairpinPowerDrift ? ctx.hairpinAccelerationBoost : 1f;
                ctx.targetCar.accelerationMultiplier = Mathf.RoundToInt(ctx.calculatedAcceleration * ctx.driftAccelerationMultiplier * boost * tempAccMult);
            } else {
                ctx.targetCar.accelerationMultiplier = Mathf.RoundToInt(ctx.calculatedAcceleration * tempAccMult);
            }

            float trueSpeed = ctx.targetCarRb != null ? ctx.targetCarRb.linearVelocity.magnitude * 3.6f : Mathf.Abs(ctx.targetCar.carSpeed);
            ctx.currentSpeed = Mathf.Lerp(ctx.currentSpeed, trueSpeed, Time.deltaTime * 10f);

            float dynamicArrivalRange = ctx.waypointArrivalRangeBase + (ctx.currentSpeed * ctx.waypointArrivalSpeedMultiplier);
            
            if (ctx.upcomingWaypoints == null || ctx.upcomingWaypoints.Length == 0) ctx.mustRecalculateWaypoints = true;

            SplineUtility.GetNearestPoint(ctx.track.Spline, ctx.track.transform.InverseTransformPoint(ctx.targetCar.transform.position), out _, out float tPosition);
            Vector3 geometricIdeal = ctx.track.transform.TransformPoint(ctx.track.Spline.EvaluatePosition(tPosition));
            ctx.currentDeviation = Vector3.Distance(ctx.targetCar.transform.position, geometricIdeal);

            // Detect lap completion
            if (tPosition < 0.15f && ctx.previousProgressPercent > 0.85f)
            {
                if (ctx.entity != null)
                {
                    OnLapCompleted?.Invoke(ctx.entity);
                }
            }
            ctx.previousProgressPercent = tPosition;

            if (!ctx.mustRecalculateWaypoints)
            {
                if (ctx.currentDeviation > config.maxDeviationDistance)
                {
                    ctx.mustRecalculateWaypoints = true;
                }
                else 
                {
                    Vector3 toWp = ctx.upcomingWaypoints[0] - ctx.targetCar.transform.position;
                    float dot = Vector3.Dot(ctx.targetCar.transform.forward, toWp.normalized);
                    
                    if (dot < -0.1f || Vector3.Distance(ctx.targetCar.transform.position, ctx.upcomingWaypoints[0]) <= dynamicArrivalRange)
                    {
                        ctx.mustRecalculateWaypoints = true;
                    }
                }
            }

            if (ctx.mustRecalculateWaypoints)
            {
                GenerateTrajectory(ctx);
                ctx.mustRecalculateWaypoints = false;
            }

            float targetSafeSpeed;
            ctx.isBrakingForCurve = CheckForApproachingCurveVector(ctx, out targetSafeSpeed, out ctx.requiresHandbrake, out ctx.isHairpinPowerDrift);

            ctx.aiBrake.buttonPressed = false;
            ctx.aiThrottle.buttonPressed = false;
            ctx.aiLeft.buttonPressed = false;
            ctx.aiRight.buttonPressed = false;
            ctx.aiReverse.buttonPressed = false;

            if (ctx.isHairpinPowerDrift)
            {
                ctx.aiBrake.buttonPressed = true;
                if (ctx.currentSpeed > ctx.safeCornerSpeed * activeTempSpeedMult * 1.5f) {
                    ctx.aiReverse.buttonPressed = true;
                } else if (ctx.currentSpeed < effectiveMaxSimulationSpeed) {
                    ctx.aiThrottle.buttonPressed = true;
                }
            }
            else if (ctx.isBrakingForCurve && ctx.currentSpeed > targetSafeSpeed)
            {
                if (ctx.requiresHandbrake) {
                    ctx.aiBrake.buttonPressed = true; 
                } else {
                    ctx.aiReverse.buttonPressed = true;
                }
            }
            else
            {
                if (ctx.currentSpeed < effectiveMaxSimulationSpeed) {
                    ctx.aiThrottle.buttonPressed = true;
                }
            }

            float steerDepth = ctx.targetCar.isDrifting ? 1f : 0.3f;
            Vector3 steeringTargetWorld = Vector3.Lerp(ctx.upcomingWaypoints[0], ctx.upcomingWaypoints[1], steerDepth);
            Vector3 localWaypoint = ctx.targetCar.transform.InverseTransformPoint(steeringTargetWorld);
            float angleToWaypoint = Mathf.Atan2(localWaypoint.x, localWaypoint.z) * Mathf.Rad2Deg;
            
            float steerInput = Mathf.Clamp(angleToWaypoint / ctx.targetCar.maxSteeringAngle, -1f, 1f);
            
            if (steerInput < -ctx.steerDeadzone) {
                ctx.aiLeft.buttonPressed = true;
            } else if (steerInput > ctx.steerDeadzone) {
                ctx.aiRight.buttonPressed = true;
            }

            if (ctx.arcadeSteerAssist > 0f && ctx.currentSpeed > 5f)
            {
                float assistStrength = ctx.targetCar.isDrifting ? ctx.arcadeSteerAssist * ctx.driftSteerAssistMultiplier : ctx.arcadeSteerAssist;
                if (ctx.isHairpinPowerDrift) assistStrength *= ctx.hairpinSteerAssistBoost;
                
                float deviationMultiplier = 1f + (ctx.currentDeviation / 5f);
                assistStrength *= deviationMultiplier;

                float extraRotation = steerInput * assistStrength * ctx.currentSpeed * Time.deltaTime;
                ctx.targetCar.transform.Rotate(0, extraRotation, 0, Space.Self);
            }

            if (ctx.targetCarRb != null && ctx.currentSpeed > 10f)
            {
                Vector3 vel = ctx.targetCarRb.linearVelocity;

                if (!ctx.targetCar.isDrifting)
                {
                    Vector3 toWaypoint = (ctx.upcomingWaypoints[0] - ctx.targetCar.transform.position).normalized;
                    Vector3 faithfulForward = Vector3.Lerp(ctx.targetCar.transform.forward, toWaypoint, 0.15f).normalized;
                    Vector3 idealVel = faithfulForward * vel.magnitude;
                    
                    float gripEnforcement = Mathf.Lerp(0f, 8f, statsGetGripNorm(ctx)) * tempGripMult * Time.deltaTime; // Assuming normalized grip from stats
                    ctx.targetCarRb.linearVelocity = Vector3.Lerp(vel, idealVel, gripEnforcement);
                }
                else
                {
                    Vector3 toWaypoint = (ctx.upcomingWaypoints[0] - ctx.targetCar.transform.position).normalized;
                    Vector3 idealVel = toWaypoint * vel.magnitude;
                    
                    float driftEnforcement = Mathf.Lerp(0f, 3f, statsGetGripNorm(ctx)) * tempGripMult * Time.deltaTime;
                    ctx.targetCarRb.linearVelocity = Vector3.Lerp(vel, idealVel, driftEnforcement);
                }
            }
        }

        private float statsGetGripNorm(SplineCarRunnerContext ctx)
        {
            // Derived reverse-engineer from ctx.arcadeSteerAssist... safe to use standard fallback if isolated
            return Mathf.InverseLerp(config.boundsArcadeSteerAssist.x, config.boundsArcadeSteerAssist.y, ctx.arcadeSteerAssist / (ctx.currentSimulationMultiplier == 0 ? 1 : ctx.currentSimulationMultiplier));
        }

        private bool CheckForApproachingCurveVector(SplineCarRunnerContext ctx, out float targetSpeed, out bool needsHandbrake, out bool isHairpin)
        {
            targetSpeed = ctx.maxSimulationSpeed;
            bool needsBrake = false;
            needsHandbrake = false;
            isHairpin = false;

            float tApprox;
            SplineUtility.GetNearestPoint(ctx.track.Spline, ctx.track.transform.InverseTransformPoint(ctx.targetCar.transform.position), out _, out tApprox);
            float baseDistance = tApprox * ctx.splineLength;

            float lookaheadDistance = Mathf.Clamp((ctx.currentSpeed / 3.6f) * 1.25f, 15f, 60f);
            float fixedSpacing = 5f; 
            int sampleCount = Mathf.CeilToInt(lookaheadDistance / fixedSpacing);
            if (sampleCount < 3) sampleCount = 3; 
            
            Vector3[] pureW = new Vector3[sampleCount];
            float accDist = 4f; 

            for (int i = 0; i < sampleCount; i++)
            {
                pureW[i] = GetPureSplinePoint(ctx, baseDistance + accDist);
                accDist += fixedSpacing;
            }

            float maxAngle = 0f;
            for (int i = 0; i < sampleCount - 2; i++)
            {
                Vector3 dir = (pureW[i + 1] - pureW[i]).normalized;
                Vector3 nextDir = (pureW[i + 2] - pureW[i + 1]).normalized;
                
                float angle = Vector3.Angle(dir, nextDir);
                if (angle > maxAngle) maxAngle = angle;
            }

            float macroAngle = 0f;
            Vector3 wFirst = pureW[0];
            Vector3 wMid = pureW[sampleCount / 2];
            Vector3 wLast = pureW[sampleCount - 1];

            Vector3 dir1 = (wMid - wFirst).normalized;
            Vector3 dir2 = (wLast - wMid).normalized;
            macroAngle = Vector3.Angle(dir1, dir2);

            if (maxAngle > ctx.curveAngleThreshold)
            {
                if (maxAngle >= ctx.hairpinAngleThreshold)
                {
                    isHairpin = true;
                    needsHandbrake = true;
                }
                else if (maxAngle >= ctx.handbrakeAngleThreshold)
                {
                    needsHandbrake = true;
                }

                if (!isHairpin)
                {
                    float curveSeverity = Mathf.InverseLerp(ctx.curveAngleThreshold, ctx.handbrakeAngleThreshold, maxAngle);
                    float speedScaling = 0.65f; // Approximated normalized stat curve for grip
                    float dynamicSafeSpeed = Mathf.Max(ctx.safeCornerSpeed, ctx.currentSpeed * speedScaling);
                    
                    float deviationPenalty = Mathf.Clamp01(ctx.currentDeviation / 10f);
                    dynamicSafeSpeed *= (1f - (deviationPenalty * 0.3f));
                    
                    float reqSpeed = Mathf.Lerp(ctx.maxSimulationSpeed, dynamicSafeSpeed, curveSeverity);
                    
                    if (reqSpeed < targetSpeed)
                    {
                        targetSpeed = reqSpeed;
                        needsBrake = true;
                    }
                }
            }
            
            if (macroAngle > ctx.macroCurveAngleThreshold)
            {
                float macroSeverity = Mathf.InverseLerp(ctx.macroCurveAngleThreshold, ctx.macroCurveAngleThreshold * 2f, macroAngle);
                float macroReqSpeed = Mathf.Lerp(ctx.maxSimulationSpeed, ctx.safeCornerSpeed, macroSeverity);

                if (macroReqSpeed < targetSpeed)
                {
                    targetSpeed = macroReqSpeed;
                    needsBrake = true;
                }
            }
            
            return needsBrake;
        }

        private void GenerateTrajectory(SplineCarRunnerContext ctx)
        {
            if (ctx.upcomingWaypoints == null || ctx.upcomingWaypoints.Length != config.waypointCount)
            {
                ctx.upcomingWaypoints = new Vector3[config.waypointCount];
            }

            Transform subjectTransform = ctx.targetCar.transform;
            SplineUtility.GetNearestPoint(ctx.track.Spline, ctx.track.transform.InverseTransformPoint(subjectTransform.position), out _, out float tApprox);
            
            float baseDistance = (tApprox * ctx.splineLength);
            float speedPush = Mathf.Clamp(ctx.currentSpeed * ctx.distanceSpeedMultiplier, 0f, 15f);
            
            float rawArrivalRange = ctx.waypointArrivalRangeBase + (ctx.currentSpeed * ctx.waypointArrivalSpeedMultiplier);
            float safeStartingDistance = Mathf.Max(ctx.baseWaypointDistance + speedPush, rawArrivalRange + 1f);

            float accTemp = safeStartingDistance;
            float curTemp = ctx.baseWaypointDistance;
            
            float startDist = baseDistance + accTemp;
            for (int i = 0; i < config.waypointCount / 2; i++) { accTemp += curTemp; curTemp *= config.waypointSpacingMultiplier; }
            float midDist = baseDistance + accTemp;
            for (int i = config.waypointCount / 2; i < config.waypointCount - 1; i++) { accTemp += curTemp; curTemp *= config.waypointSpacingMultiplier; }
            float endDist = baseDistance + accTemp;

            float p0 = (Mathf.PerlinNoise(startDist * config.laneChangeVariationSpeed, 0f) * 2f) - 1f;
            float p1 = (Mathf.PerlinNoise(midDist * config.laneChangeVariationSpeed, 0f) * 2f) - 1f;
            float p2 = (Mathf.PerlinNoise(endDist * config.laneChangeVariationSpeed, 0f) * 2f) - 1f;
            
            int midIndex = config.waypointCount / 2;
            int endIndex = config.waypointCount - 1;

            float accDist = safeStartingDistance;
            float curSpacing = ctx.baseWaypointDistance;

            for (int i = 0; i < config.waypointCount; i++)
            {
                float distAlongSpline = baseDistance + accDist;
                
                accDist += curSpacing;
                curSpacing *= config.waypointSpacingMultiplier;
                distAlongSpline %= ctx.splineLength;
                
                float tWaypoint = distAlongSpline / ctx.splineLength;
                
                Vector3 splLocalPos = SplineUtility.EvaluatePosition(ctx.track.Spline, tWaypoint);
                Vector3 splLocalTang = SplineUtility.EvaluateTangent(ctx.track.Spline, tWaypoint);
                Vector3 splLocalUp = SplineUtility.EvaluateUpVector(ctx.track.Spline, tWaypoint);
                
                Vector3 splinePos = ctx.track.transform.TransformPoint(splLocalPos);
                Vector3 splineTangent = ctx.track.transform.TransformDirection(splLocalTang);
                Vector3 splineUp = ctx.track.transform.TransformDirection(splLocalUp);
                Vector3 right = Vector3.Cross(splineUp, splineTangent).normalized;
                
                float perlinOffset = 0f;
                if (i <= midIndex)
                {
                    float t = midIndex > 0 ? (float)i / midIndex : 0f;
                    perlinOffset = Mathf.Lerp(p0, p1, t);
                }
                else
                {
                    float t = (endIndex - midIndex) > 0 ? (float)(i - midIndex) / (endIndex - midIndex) : 0f;
                    perlinOffset = Mathf.Lerp(p1, p2, t);
                }
                
                float absoluteOffset = perlinOffset * config.waypointMaxLateralOffset;
                
                if (ctx.preCurveWideOffset > 0f)
                {
                    float lookahead = ctx.baseWaypointDistance * 2f + (ctx.currentSpeed * ctx.distanceSpeedMultiplier * 1.5f);
                    float futureT = (distAlongSpline + lookahead) / ctx.splineLength;
                    futureT %= 1f;
                    Vector3 futureTang = ctx.track.transform.TransformDirection(SplineUtility.EvaluateTangent(ctx.track.Spline, futureT)).normalized;
                    
                    float curveBendInfluence = -Vector3.Dot(right, futureTang);
                    absoluteOffset += (curveBendInfluence * ctx.preCurveWideOffset);
                }

                if (ctx.postCurveWideOffset > 0f)
                {
                    float shortLookahead = 5f; 
                    float nearFutureT = (distAlongSpline + shortLookahead) / ctx.splineLength;
                    nearFutureT %= 1f;
                    Vector3 nearFutureTang = ctx.track.transform.TransformDirection(SplineUtility.EvaluateTangent(ctx.track.Spline, nearFutureT)).normalized;
                    
                    float immediateBendInfluence = Vector3.Dot(right, nearFutureTang);
                    absoluteOffset += (immediateBendInfluence * ctx.postCurveWideOffset);
                }
                
                ctx.upcomingWaypoints[i] = splinePos + (right * absoluteOffset);
            }
        }

        private Vector3 GetPureSplinePoint(SplineCarRunnerContext ctx, float distance)
        {
            distance %= ctx.splineLength;
            float t = distance / ctx.splineLength;
            Vector3 locPos = SplineUtility.EvaluatePosition(ctx.track.Spline, t);
            return ctx.track.transform.TransformPoint(locPos);
        }
    }
}
