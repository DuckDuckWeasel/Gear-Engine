using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Tracks;
using UnityEngine;
using VContainer.Unity;

namespace GearEngine.CarSimulation.Simulation
{
    internal sealed class TrackSimulationRunner : ITickable, ITrackSimulationRunner
    {
        private TrackSimulation simulation;

        public TrackSimulation ActiveSimulation { get; private set; }

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
            if (sim.Profile.TotalLength < 1e-4f)
            {
                return;
            }

            RunDynamicsStep(dt, sim);
        }

        private void RunDynamicsStep(float dt, TrackSimulation sim)
        {
            ApplySpeedImpulse(sim.Motion);
            SimulationFrame frame = SimulationFrame.Create(sim, dt);
            IntegrateSpeed(frame);
            ApplyDrift(frame);
            ApplyDriftVisuals(frame);
            AdvanceRace(frame);
        }

        private void ApplySpeedImpulse(CarMotionState motion)
        {
            motion.Speed += motion.PendingSpeedBoost;
            motion.PendingSpeedBoost = 0f;
        }

        private void IntegrateSpeed(SimulationFrame f)
        {
            float curveHere = BuildCornerSpeedLimit(f.Handling, f.Here.Curvature, f.CurvatureEpsilon);
            float lookDist = Mathf.Max(f.LookAheadMin, f.Motion.Speed * f.LookAheadSpeedFactor);
            float minAhead = BuildMinCornerSpeedAhead(f.Profile, f.Motion.Distance, lookDist, f.Handling, f.AheadProbeStep, f.CurvatureEpsilon);
            float targetSpeed = Mathf.Min(f.TopSpeed, curveHere, minAhead);
            ApplyAccelDecelToward(f.Motion, targetSpeed, f.Dt, f.Acceleration, f.Brake);
        }

        private void ApplyAccelDecelToward(CarMotionState motion, float targetSpeed, float dt, float acceleration, float brake)
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

        private void ApplyDrift(SimulationFrame f)
        {
            CarMotionState motion = f.Motion;
            TrackSample here = f.Here;
            float lateralDemand = motion.Speed * motion.Speed * Mathf.Max(here.Curvature, 0f);
            float gripCapacity = f.Handling * f.GripScale;
            float driftDelta = f.Dt * 2.5f / Mathf.Max(0.2f, f.Stability);
            if (lateralDemand > gripCapacity)
            {
                float excessRatio = (lateralDemand - gripCapacity) / Mathf.Max(gripCapacity, 1e-4f);
                motion.DriftIntensity = Mathf.Clamp01(motion.DriftIntensity + excessRatio * driftDelta);
            }
            else
            {
                motion.DriftIntensity = Mathf.Clamp01(motion.DriftIntensity - f.Recovery * f.Dt);
            }
        }

        private void ApplyDriftVisuals(SimulationFrame f)
        {
            CarMotionState motion = f.Motion;
            TrackSample here = f.Here;
            float sideSign = Mathf.Sign(here.SignedCurvature);
            if (Mathf.Abs(sideSign) < 1e-4f)
            {
                sideSign = 1f;
            }

            float targetSlip = motion.DriftIntensity * 28f * sideSign;
            motion.SlipAngle = Mathf.Lerp(motion.SlipAngle, targetSlip, f.Dt * 4f);
            motion.LateralOffset = Mathf.Lerp(motion.LateralOffset, motion.DriftIntensity * 0.45f * sideSign, f.Dt * 3f);
        }

        private void AdvanceRace(SimulationFrame f)
        {
            CarMotionState motion = f.Motion;
            RaceRuntimeState race = f.Race;
            BakedTrackProfile profile = f.Profile;
            float totalLength = f.TotalLength;
            float driftPenaltyScale = f.DriftPenaltyScale;
            float driftPenalty = motion.DriftIntensity * driftPenaltyScale;
            float effectiveSpeed = motion.Speed * (1f - driftPenalty);
            float newDistance = motion.Distance + effectiveSpeed * f.Dt;
            int lapIncrement = AdvanceDistanceCore(motion, profile, totalLength, ref newDistance, ref effectiveSpeed);
            motion.Distance = newDistance;
            race.CurrentTime += f.Dt;
            race.DistanceTravelled += effectiveSpeed * f.Dt;
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

        private float BuildMinCornerSpeedAhead(BakedTrackProfile profile, float fromDistance, float windowMetres, float handling, float aheadProbeStep, float curvatureEpsilon)
        {
            float minV = float.MaxValue;
            float walked = 0f;
            float d = fromDistance;
            while (walked < windowMetres)
            {
                TrackSample s = profile.Evaluate(d);
                float limit = BuildCornerSpeedLimit(handling, s.Curvature, curvatureEpsilon);
                minV = Mathf.Min(minV, limit);
                walked += aheadProbeStep;
                d += aheadProbeStep;
            }

            return minV;
        }

        private float BuildCornerSpeedLimit(float handling, float curvature, float curvatureEpsilon)
        {
            float k = Mathf.Max(curvature, curvatureEpsilon);
            return Mathf.Sqrt(Mathf.Max(0f, handling / k));
        }
    }
}
