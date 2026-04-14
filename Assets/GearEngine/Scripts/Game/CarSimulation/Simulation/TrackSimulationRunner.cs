using GearEngine.CarSimulation.Entity;
using GearEngine.CarSimulation.Track;
using Scaffold.Entities;
using UnityEngine;
using VContainer.Unity;

namespace GearEngine.CarSimulation.Simulation
{
    internal sealed class TrackSimulationRunner : ITickable, ITrackSimulationRunner
    {
        private const float curvatureEpsilon = 1e-5f;
        private const float driftPenaltyScale = 0.15f;
        private const float defaultAcceleration = 12f;
        private const float defaultBrake = 22f;
        private const float defaultHandling = 48f;
        private const float defaultStability = 1.1f;
        private const float defaultRecovery = 0.85f;
        private const float defaultTopSpeed = 32f;
        private const float gripScale = 0.12f;
        private const float lookAheadMinMetres = 8f;
        private const float lookAheadSpeedFactor = 0.75f;
        private const float aheadProbeStep = 0.25f;

        private TrackSimulation simulation;

        public void SetSimulation(TrackSimulation sim)
        {
            simulation = sim;
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
            if (sim.BakedProfile.TotalLength < 1e-4f)
            {
                return;
            }

            RunDynamicsStep(dt, sim);
        }

        private void RunDynamicsStep(float dt, TrackSimulation sim)
        {
            BakedTrackProfile profile = sim.BakedProfile;
            CarMotionState motion = sim.Motion;
            RaceRuntimeState race = sim.Race;
            float totalLength = profile.TotalLength;
            float topSpeed = BuildTopSpeedFromEntity(sim.Car);
            ApplySpeedImpulse(motion);
            TrackSample here = profile.Evaluate(motion.Distance);
            motion.SampleIndex = profile.FindSampleIndexNear(motion.Distance);
            IntegrateSpeedTowardTarget(motion, profile, here, topSpeed, dt);
            ApplyDriftIntensity(motion, here, dt);
            ApplyDriftVisuals(motion, here, dt);
            AdvanceDistanceAndRace(motion, race, profile, totalLength, dt);
        }

        private void ApplySpeedImpulse(CarMotionState motion)
        {
            motion.Speed += motion.PendingSpeedBoost;
            motion.PendingSpeedBoost = 0f;
        }

        private void IntegrateSpeedTowardTarget(CarMotionState motion, BakedTrackProfile profile, TrackSample here, float topSpeed, float dt)
        {
            float curveHere = BuildCornerSpeedLimit(defaultHandling, here.Curvature);
            float lookDist = Mathf.Max(lookAheadMinMetres, motion.Speed * lookAheadSpeedFactor);
            float minAhead = BuildMinCornerSpeedAhead(profile, motion.Distance, lookDist, defaultHandling);
            float targetSpeed = Mathf.Min(topSpeed, curveHere, minAhead);
            ApplyAccelDecelToward(motion, targetSpeed, dt);
        }

        private void ApplyAccelDecelToward(CarMotionState motion, float targetSpeed, float dt)
        {
            if (motion.Speed > targetSpeed)
            {
                motion.Speed = Mathf.Max(targetSpeed, motion.Speed - defaultBrake * dt);
            }
            else
            {
                motion.Speed = Mathf.Min(targetSpeed, motion.Speed + defaultAcceleration * dt);
            }
        }

        private void ApplyDriftIntensity(CarMotionState motion, TrackSample here, float dt)
        {
            float lateralDemand = motion.Speed * motion.Speed * Mathf.Max(here.Curvature, 0f);
            float gripCapacity = defaultHandling * gripScale;
            float driftDelta = dt * 2.5f / Mathf.Max(0.2f, defaultStability);
            if (lateralDemand > gripCapacity)
            {
                GrowDriftForOverGrip(motion, lateralDemand, gripCapacity, driftDelta);
            }
            else
            {
                DecayDrift(motion, defaultRecovery, dt);
            }
        }

        private void DecayDrift(CarMotionState motion, float recovery, float dt)
        {
            motion.DriftIntensity = Mathf.Clamp01(motion.DriftIntensity - recovery * dt);
        }

        private void GrowDriftForOverGrip(CarMotionState motion, float lateralDemand, float gripCapacity, float driftDelta)
        {
            float excessRatio = (lateralDemand - gripCapacity) / Mathf.Max(gripCapacity, 1e-4f);
            motion.DriftIntensity = Mathf.Clamp01(motion.DriftIntensity + excessRatio * driftDelta);
        }

        private void ApplyDriftVisuals(CarMotionState motion, TrackSample here, float dt)
        {
            float sideSign = Mathf.Sign(here.SignedCurvature);
            if (Mathf.Abs(sideSign) < 1e-4f)
            {
                sideSign = 1f;
            }

            float targetSlip = motion.DriftIntensity * 28f * sideSign;
            motion.SlipAngle = Mathf.Lerp(motion.SlipAngle, targetSlip, dt * 4f);
            motion.LateralOffset = Mathf.Lerp(motion.LateralOffset, motion.DriftIntensity * 0.45f * sideSign, dt * 3f);
        }

        private void AdvanceDistanceAndRace(CarMotionState motion, RaceRuntimeState race, BakedTrackProfile profile, float totalLength, float dt)
        {
            float driftPenalty = motion.DriftIntensity * driftPenaltyScale;
            float effectiveSpeed = motion.Speed * (1f - driftPenalty);
            float newDistance = motion.Distance + effectiveSpeed * dt;
            int lapIncrement = AdvanceDistanceCore(motion, profile, totalLength, ref newDistance, ref effectiveSpeed);
            motion.Distance = newDistance;
            race.CurrentTime += dt;
            race.DistanceTravelled += effectiveSpeed * dt;
            race.CurrentLap += lapIncrement;
            race.Progress01 = Mathf.Clamp01(motion.Distance / totalLength);
            race.CurrentSegmentIndex = motion.SampleIndex;
            race.CurrentSpeed = effectiveSpeed;
            race.IsDrifting = motion.DriftIntensity > 0.12f;
        }

        private int AdvanceDistanceCore(CarMotionState motion, BakedTrackProfile profile, float totalLength, ref float newDistance, ref float effectiveSpeed)
        {
            if (profile.IsClosed)
            {
                return StripClosedLaps(ref newDistance, totalLength);
            }

            if (newDistance >= totalLength)
            {
                newDistance = totalLength;
                motion.Speed = 0f;
                effectiveSpeed = 0f;
            }

            return 0;
        }

        private int StripClosedLaps(ref float newDistance, float totalLength)
        {
            int lapIncrement = 0;
            while (newDistance >= totalLength)
            {
                newDistance -= totalLength;
                lapIncrement++;
            }

            return lapIncrement;
        }

        private float BuildMinCornerSpeedAhead(BakedTrackProfile profile, float fromDistance, float windowMetres, float handling)
        {
            float minV = float.MaxValue;
            float walked = 0f;
            float d = fromDistance;
            while (walked < windowMetres)
            {
                TrackSample s = profile.Evaluate(d);
                float limit = BuildCornerSpeedLimit(handling, s.Curvature);
                minV = Mathf.Min(minV, limit);
                walked += aheadProbeStep;
                d += aheadProbeStep;
            }

            return minV;
        }

        private float BuildCornerSpeedLimit(float handling, float curvature)
        {
            float k = Mathf.Max(curvature, curvatureEpsilon);
            return Mathf.Sqrt(Mathf.Max(0f, handling / k));
        }

        private float BuildTopSpeedFromEntity(CarEntity car)
        {
            VariableSO speedVar = simulation.CarVariables?.Speed;
            if (speedVar != null && car.TryGetValue<float>(speedVar, out float v))
            {
                return Mathf.Max(0.1f, v);
            }

            return defaultTopSpeed;
        }
    }
}
