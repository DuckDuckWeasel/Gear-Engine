using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Entity;
using GearEngine.CarSimulation.Tracks;
using Scaffold.Entities;
using UnityEngine;
using VContainer.Unity;

namespace GearEngine.CarSimulation.Simulation
{
    internal sealed class TrackSimulationRunner : ITickable, ITrackSimulationRunner
    {
        public TrackSimulation ActiveSimulation { get; private set; }

        private TrackSimulation simulation;

        public void SetSimulation(TrackSimulation sim)
        {
            simulation = sim;
            ActiveSimulation = sim;
        }

        public void Tick()
        {
            if (simulation?.State != SimulationLifecycleState.Running)
            {
                return;
            }

            Step(Time.deltaTime);
        }

        internal void Step(float dt)
        {
            if (simulation == null || simulation.State != SimulationLifecycleState.Running || dt <= 0f)
            {
                return;
            }

            StepCore(dt);
        }

        private void StepCore(float dt)
        {
            TrackSimulation sim = simulation;
            if (sim.WaypointPath == null || sim.WaypointPath.TotalLength < 1e-4f)
            {
                return;
            }

            CarMotionState motion = sim.Motion;
            motion.Speed += motion.PendingSpeedBoost;
            motion.PendingSpeedBoost = 0f;

            Transform trackTransform = sim.TrackRootTransform;
            if (trackTransform == null)
            {
                return;
            }

            SimpleWaypointDriver.Step(dt, sim, trackTransform, sim.DriverTuning);
        }
    }

    internal static class SimpleWaypointDriver
    {
        internal static void SeedStart(CarMotionState motion, SplineWaypointPath path, Transform trackTransform)
        {
            motion.Reset();
            if (path == null || path.Count < 2 || trackTransform == null)
            {
                return;
            }

            motion.Position = path.GetWorldPoint(0, trackTransform);
            int next = path.NextWaypointIndex(0);
            Vector3 to = path.GetWorldPoint(next, trackTransform) - motion.Position;
            to.y = 0f;
            motion.YawDegrees = to.sqrMagnitude > 1e-8f ? Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg : trackTransform.eulerAngles.y;
            motion.WaypointIndex = 0;
            motion.DistanceAlongPath = 0f;
        }

        internal static void Step(float dt, TrackSimulation sim, Transform trackTransform, SimpleTrackDriverTuning t)
        {
            if (dt <= 0f || trackTransform == null)
            {
                return;
            }

            SplineWaypointPath path = sim.WaypointPath;
            if (path == null || path.Count < 2 || path.TotalLength < 1e-4f)
            {
                return;
            }

            StepWithValidPath(dt, sim, trackTransform, t, path);
        }

        private static void StepWithValidPath(float dt, TrackSimulation sim, Transform trackTransform, SimpleTrackDriverTuning t, SplineWaypointPath path)
        {
            CarMotionState motion = sim.Motion;
            RaceRuntimeState race = sim.Race;
            CarEntity car = sim.Car;
            CarVariableSet vars = sim.Variables;
            float topSpeed = ResolveTopSpeed(car, vars);
            float accel = ResolveFloat(car, vars?.Acceleration, 12f);
            float brake = ResolveFloat(car, vars?.Brake, 18f);
            int targetWp = AdvanceWaypointIndex(path, motion.WaypointIndex, motion.Position, trackTransform, t.WaypointCaptureRadius, path.IsClosed);
            motion.WaypointIndex = targetWp;
            ApplyYawAndSpeed(motion, path, trackTransform, t, dt, topSpeed, accel, brake, out float forwardMul, out bool inDriftBand);
            float stepDist = AdvancePositionFromYaw(motion, dt, forwardMul);
            UpdateRaceFields(motion, race, path, trackTransform, dt, forwardMul, stepDist, inDriftBand);
        }

        internal static int AdvanceWaypointIndex(SplineWaypointPath path, int current, Vector3 worldPos, Transform trackTransform, float captureRadius, bool closed)
        {
            int idx = current;
            int guard = path.Count + 2;
            while (guard-- > 0 && TryAdvanceOneWaypoint(path, ref idx, worldPos, trackTransform, captureRadius, closed))
            {
            }

            return idx;
        }

        private static bool TryAdvanceOneWaypoint(SplineWaypointPath path, ref int idx, Vector3 worldPos, Transform trackTransform, float captureRadius, bool closed)
        {
            float dist = path.HorizontalDistanceToWaypoint(worldPos, idx, trackTransform);
            if (dist > captureRadius)
            {
                return false;
            }

            int next = path.NextWaypointIndex(idx);
            if (!closed && next == idx)
            {
                return false;
            }

            idx = next;
            return closed || idx < path.Count - 1;
        }

        private static void ApplyYawAndSpeed(CarMotionState motion, SplineWaypointPath path, Transform trackTransform, SimpleTrackDriverTuning t, float dt, float topSpeed, float accel, float brake, out float forwardMul, out bool inDriftBand)
        {
            float desiredYaw = ComputeDesiredYaw(motion, path, trackTransform, t);
            float yawError = Mathf.DeltaAngle(motion.YawDegrees, desiredYaw);
            float demandDegPerSec = Mathf.Abs(yawError) / Mathf.Max(dt, 1e-4f);
            float maxYawRate = t.BaseMaxYawRateDegreesPerSecond * YawScaleFromSpeed(motion.Speed, topSpeed);
            float targetSpeed = ComputeTargetSpeedForCorner(topSpeed, demandDegPerSec, maxYawRate, t);
            StepSpeedToward(motion, targetSpeed, dt, accel, brake);
            float yawStep = Mathf.Sign(yawError) * Mathf.Min(Mathf.Abs(yawError), maxYawRate * dt);
            motion.YawDegrees = motion.YawDegrees + yawStep;
            inDriftBand = ApplyDriftVisuals(motion, t, dt, yawError, out forwardMul);
        }

        private static float ComputeDesiredYaw(CarMotionState motion, SplineWaypointPath path, Transform trackTransform, SimpleTrackDriverTuning t)
        {
            Vector3 seekPoint = path.EvaluateLookaheadWorld(motion.WaypointIndex, trackTransform, t.LookaheadMetres);
            Vector3 to = seekPoint - motion.Position;
            to.y = 0f;
            return to.sqrMagnitude > 1e-8f ? Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg : motion.YawDegrees;
        }

        private static float ComputeTargetSpeedForCorner(float topSpeed, float demandDegPerSec, float maxYawRate, SimpleTrackDriverTuning t)
        {
            float targetSpeed = topSpeed;
            if (demandDegPerSec > maxYawRate)
            {
                float excess = demandDegPerSec - maxYawRate;
                targetSpeed = Mathf.Max(0f, topSpeed - excess * t.CornerSlowdownYawDemandScale);
            }

            return targetSpeed;
        }

        private static bool ApplyDriftVisuals(CarMotionState motion, SimpleTrackDriverTuning t, float dt, float yawError, out float forwardMul)
        {
            float absErr = Mathf.Abs(yawError);
            bool inDriftBand = absErr >= t.DriftErrorMinDegrees && absErr <= t.DriftErrorMaxDegrees;
            bool perfect = absErr <= t.PerfectLineErrorDegrees;
            if (perfect)
            {
                ApplyPerfectLineVisuals(motion, t, dt);
            }
            else
            {
                ApplyNonPerfectLineVisuals(motion, t, dt, yawError, inDriftBand);
            }

            forwardMul = ComputeForwardMultiplier(inDriftBand, t);
            return inDriftBand;
        }

        private static void ApplyPerfectLineVisuals(CarMotionState motion, SimpleTrackDriverTuning t, float dt)
        {
            motion.DriftIntensity = Mathf.MoveTowards(motion.DriftIntensity, 0f, dt * 3f);
            motion.SlipAngle = Mathf.Lerp(motion.SlipAngle, 0f, dt * t.SlipVisualLerpSpeed);
        }

        private static void ApplyNonPerfectLineVisuals(CarMotionState motion, SimpleTrackDriverTuning t, float dt, float yawError, bool inDriftBand)
        {
            if (inDriftBand)
            {
                ApplyDriftBandSlip(motion, t, dt, yawError);
            }
            else
            {
                motion.DriftIntensity = Mathf.MoveTowards(motion.DriftIntensity, 0f, dt * 3f);
                motion.SlipAngle = Mathf.Lerp(motion.SlipAngle, 0f, dt * t.SlipVisualLerpSpeed * 0.5f);
            }
        }

        private static void ApplyDriftBandSlip(CarMotionState motion, SimpleTrackDriverTuning t, float dt, float yawError)
        {
            motion.DriftIntensity = Mathf.MoveTowards(motion.DriftIntensity, 1f, dt * 3f);
            float slipSign = Mathf.Sign(yawError);
            if (Mathf.Abs(slipSign) < 1e-4f)
            {
                slipSign = 1f;
            }

            motion.SlipAngle = Mathf.Lerp(motion.SlipAngle, slipSign * 25f, dt * t.SlipVisualLerpSpeed);
        }

        private static float ComputeForwardMultiplier(bool inDriftBand, SimpleTrackDriverTuning t)
        {
            return inDriftBand ? 1f - t.DriftSpeedPenalty : 1f;
        }

        private static float AdvancePositionFromYaw(CarMotionState motion, float dt, float forwardMul)
        {
            Quaternion yawRot = Quaternion.Euler(0f, motion.YawDegrees, 0f);
            Vector3 fwd = yawRot * Vector3.forward;
            float stepDist = motion.Speed * forwardMul * dt;
            motion.Position += fwd * stepDist;
            return stepDist;
        }

        private static void UpdateRaceFields(CarMotionState motion, RaceRuntimeState race, SplineWaypointPath path, Transform trackTransform, float dt, float forwardMul, float stepDist, bool inDriftBand)
        {
            race.IsDrifting = inDriftBand && motion.Speed > 1f;
            race.CurrentSpeed = motion.Speed * forwardMul;
            race.CurrentTime += dt;
            race.DistanceTravelled += stepDist;
            race.CurrentSegmentIndex = motion.WaypointIndex;
            UpdateProgressAndLaps(path, motion, race, trackTransform, stepDist);
        }

        private static void UpdateProgressAndLaps(SplineWaypointPath path, CarMotionState motion, RaceRuntimeState race, Transform trackTransform, float stepDist)
        {
            motion.DistanceAlongPath += stepDist;
            if (path.IsClosed && path.TotalLength > 1e-4f)
            {
                ApplyClosedLoopProgress(path, motion, race);
            }
            else
            {
                ApplyOpenPathProgress(path, motion, race, trackTransform);
            }
        }

        private static void ApplyClosedLoopProgress(SplineWaypointPath path, CarMotionState motion, RaceRuntimeState race)
        {
            while (motion.DistanceAlongPath >= path.TotalLength)
            {
                motion.DistanceAlongPath -= path.TotalLength;
                race.CurrentLap++;
            }

            race.Progress01 = Mathf.Clamp01(motion.DistanceAlongPath / path.TotalLength);
        }

        private static void ApplyOpenPathProgress(SplineWaypointPath path, CarMotionState motion, RaceRuntimeState race, Transform trackTransform)
        {
            float travelled = 0f;
            for (int i = 0; i < motion.WaypointIndex && i < path.Count - 1; i++)
            {
                Vector3 a = path.GetWorldPoint(i, trackTransform);
                Vector3 b = path.GetWorldPoint(i + 1, trackTransform);
                travelled += HorizontalDistance(a, b);
            }

            float denom = Mathf.Max(path.TotalLength, 1e-4f);
            race.Progress01 = Mathf.Clamp01(travelled / denom);
        }

        private static void StepSpeedToward(CarMotionState motion, float targetSpeed, float dt, float acceleration, float brake)
        {
            if (motion.Speed > targetSpeed)
            {
                motion.Speed = Mathf.Max(targetSpeed, motion.Speed - brake * dt);
            }
            else
            {
                motion.Speed = Mathf.Min(targetSpeed, motion.Speed + acceleration * dt);
            }
        }

        private static float YawScaleFromSpeed(float speed, float topSpeed)
        {
            if (topSpeed < 1e-4f)
            {
                return 1f;
            }

            float u = Mathf.Clamp01(speed / topSpeed);
            return Mathf.Lerp(1.15f, 0.8f, u);
        }

        private static float ResolveTopSpeed(CarEntity car, CarVariableSet vars)
        {
            return ResolveFloat(car, vars?.Speed, 20f);
        }

        private static float ResolveFloat(CarEntity car, VariableSO variable, float fallback)
        {
            if (car == null || variable == null)
            {
                return fallback;
            }

            return car.TryGetValue(variable, out float v) ? v : fallback;
        }

        private static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }
    }
}
