using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Tracks;
using UnityEngine;
using VContainer.Unity;

namespace GearEngine.CarSimulation.Simulation
{
    internal sealed class TrackSimulationRunner : ITickable, ITrackSimulationRunner
    {
        public TrackSimulationRunner(IRaceRandom raceRandom)
        {
            _ = raceRandom ?? throw new System.ArgumentNullException(nameof(raceRandom));
        }

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
            if (sim.Profile.TotalLength < 1e-4f)
            {
                return;
            }

            RunDynamicsStep(dt, sim);
        }

        private void RunDynamicsStep(float dt, TrackSimulation sim)
        {
            SimulationFrame frame = SimulationFrame.Create(sim, dt);
            float curveTargetSpeed = ComputeLocalGeometryCap(frame, frame.Here.Curvature);
            float degradedCap = Mathf.Lerp(frame.MaxStraightSpeed, curveTargetSpeed, frame.Handling01);
            float preIntegrateSpeed = frame.Motion.Speed;
            IntegrateAutomaticAccelDecelTowardCap(frame, degradedCap);
            float speedStress = ComputeSpeedStress(preIntegrateSpeed, curveTargetSpeed, frame);
            float lineFailure = ComputeLineFailure(frame);
            UpdateLineError(frame.Motion, speedStress, lineFailure, frame);
            ApplyCorneringVisuals(frame);
            AdvanceRace(frame);
        }

        private float ComputeLocalGeometryCap(SimulationFrame f, float curvature)
        {
            float span = Mathf.Max(f.ActiveCapCurvatureSpan, f.CurvatureEpsilon);
            float k = Mathf.Max(curvature, f.CurvatureEpsilon);
            float t = Mathf.Clamp01(k / span);
            return Mathf.Lerp(f.MaxStraightSpeed, f.MaxCurveSpeed, t);
        }

        private void IntegrateAutomaticAccelDecelTowardCap(SimulationFrame f, float activeCap)
        {
            ApplyAccelDecelToward(f.Motion, activeCap, f.Dt, f.Acceleration, f.Brake);
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

        private float ComputeSpeedStress(float preSpeed, float curveTargetSpeed, SimulationFrame f)
        {
            float overspeed = Mathf.Max(0f, preSpeed - curveTargetSpeed);
            float k = Mathf.Max(f.Here.Curvature, 0f);
            return overspeed * (1f + k * f.CurvatureStressMultiplier) * f.SpeedStressScale;
        }

        private float ComputeLineFailure(SimulationFrame f)
        {
            float cornerDifficulty = (f.Here.Curvature * f.LineDifficultyFromCurvature) + (f.Motion.Speed * f.LineDifficultyFromSpeed);
            float absorbable = f.Handling01 * f.MaxAbsorbableDifficulty;
            return Mathf.Max(0f, cornerDifficulty - absorbable);
        }

        private void UpdateLineError(CarMotionState motion, float speedStress, float lineFailure, SimulationFrame f)
        {
            float totalStress = lineFailure + speedStress * f.SpeedStressToLineErrorScale;
            if (totalStress > 0f)
            {
                motion.LineError = Mathf.Clamp01(motion.LineError + totalStress * f.LineErrorBuildRate * f.Dt);
            }
            else
            {
                motion.LineError = Mathf.Max(0f, motion.LineError - f.LineErrorDecayRate * f.Dt);
            }

            motion.SpeedStress = speedStress;
        }

        private void ApplyCorneringVisuals(SimulationFrame f)
        {
            CarMotionState motion = f.Motion;
            float sideSign = BuildCorneringSideSign(f);
            float targetSlip = motion.LineError * f.SlipAngleScale * sideSign;
            float targetOffset = motion.LineError * f.LateralOffsetScale * sideSign;
            bool recovering = motion.LineError < 0.01f;
            UpdateSlipAndLateralMotion(motion, f, targetSlip, targetOffset, recovering);
        }

        private void AdvanceRace(SimulationFrame f)
        {
            CarMotionState motion = f.Motion;
            float effectiveSpeed = BuildEffectiveSpeed(motion, f);
            ApplyRaceProgress(f, motion, effectiveSpeed);
        }

        private float BuildEffectiveSpeed(CarMotionState motion, SimulationFrame f)
        {
            float penalty = motion.LineError * f.OvershootPenaltyScale;
            return motion.Speed * (1f - penalty);
        }

        private void ApplyRaceProgress(SimulationFrame f, CarMotionState motion, float effectiveSpeed)
        {
            RaceRuntimeState race = f.Race;
            BakedTrackProfile profile = f.Profile;
            float totalLength = f.TotalLength;
            float newDistance = motion.Distance + effectiveSpeed * f.Dt;
            int lapIncrement = AdvanceDistanceCore(motion, profile, totalLength, ref newDistance, ref effectiveSpeed);
            motion.Distance = newDistance;
            race.CurrentTime += f.Dt;
            race.DistanceTravelled += effectiveSpeed * f.Dt;
            race.CurrentLap += lapIncrement;
            race.Progress01 = Mathf.Clamp01(motion.Distance / totalLength);
            race.CurrentSegmentIndex = motion.SampleIndex;
            race.CurrentSpeed = effectiveSpeed;
            race.IsDrifting = motion.LineError > f.IsDriftingThreshold;
            race.IsOvershot = motion.LineError > f.IsOvershotThreshold;
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

        private float BuildCorneringSideSign(SimulationFrame f)
        {
            float sideSign = Mathf.Sign(f.Here.SignedCurvature);
            return Mathf.Abs(sideSign) < 1e-4f ? 1f : sideSign;
        }

        private void UpdateSlipAndLateralMotion(CarMotionState motion, SimulationFrame f, float targetSlip, float targetOffset, bool recovering)
        {
            if (!recovering)
            {
                motion.SlipAngle = Mathf.MoveTowards(motion.SlipAngle, targetSlip, f.SlipBuildRate * f.Dt);
                motion.LateralOffset = Mathf.MoveTowards(motion.LateralOffset, targetOffset, f.OffsetBuildRate * f.Dt);
            }
            else
            {
                motion.SlipAngle = Mathf.MoveTowards(motion.SlipAngle, 0f, f.SlipRecoveryRate * f.Dt);
                motion.LateralOffset = Mathf.MoveTowards(motion.LateralOffset, 0f, f.OffsetRecoveryRate * f.Dt);
            }
        }
    }
}
