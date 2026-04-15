using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
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
            CarMotionState motion = frame.Motion;
            float lookAhead = Mathf.Max(frame.LookAheadMin, motion.Speed * frame.LookAheadSpeedFactor);
            CurveBandDefinition activeBand = ResolveMostSevereBandInWindow(
                frame.Profile,
                motion.Distance,
                lookAhead,
                frame.AheadProbeStep,
                frame.CurveBands);

            float targetCap = Mathf.Lerp(frame.MaxStraightSpeed, frame.MaxCurveSpeed, activeBand.Difficulty01);
            IntegrateAutomaticAccelDecelTowardCap(frame, targetCap);

            float handling01 = frame.Handling01;
            float requiredTurnDeg =
                Mathf.Abs(frame.Here.SignedCurvature)
                * motion.Speed
                * frame.Dt
                * Mathf.Rad2Deg
                * activeBand.Difficulty01;

            float handledTurnDeg = handling01 * frame.HandlingTurnRateDegPerSec * frame.Dt;
            motion.HeadingErrorDeg +=
                Mathf.Sign(frame.Here.SignedCurvature)
                * Mathf.Max(0f, requiredTurnDeg - handledTurnDeg);

            motion.HeadingErrorDeg = Mathf.MoveTowards(
                motion.HeadingErrorDeg,
                0f,
                frame.RecoveryRateDegPerSec * handling01 * frame.Dt);

            float maxErr = Mathf.Max(1e-6f, frame.MaxHeadingErrorDeg);
            float error01 = Mathf.Clamp01(Mathf.Abs(motion.HeadingErrorDeg) / maxErr);
            float effectiveSpeed = motion.Speed * (1f - error01 * frame.SpeedPenaltyScale);

            float sideSign = Mathf.Abs(motion.HeadingErrorDeg) < 1e-6f ? 0f : Mathf.Sign(motion.HeadingErrorDeg);
            motion.SlipAngle = error01 * frame.SlipAngleScale * sideSign;
            motion.LateralOffset = error01 * frame.LateralOffsetScale * sideSign;

            AdvanceRace(frame, motion, effectiveSpeed, error01);
        }

        private static CurveBandDefinition ResolveMostSevereBandInWindow(
            BakedTrackProfile profile,
            float distance,
            float lookAheadMetres,
            float probeStep,
            CurveBandDefinition[] bands)
        {
            CurveBandDefinition severe = ResolveBand(profile.Evaluate(distance).Curvature, bands);
            if (lookAheadMetres <= 0f || probeStep <= 0f)
            {
                return severe;
            }

            for (float d = probeStep; d <= lookAheadMetres + 1e-4f; d += probeStep)
            {
                float k = profile.Evaluate(distance + d).Curvature;
                severe = MoreSevere(severe, ResolveBand(k, bands));
            }

            return severe;
        }

        private static CurveBandDefinition MoreSevere(CurveBandDefinition a, CurveBandDefinition b)
        {
            return a.Difficulty01 >= b.Difficulty01 ? a : b;
        }

        private static CurveBandDefinition ResolveBand(float curvature, CurveBandDefinition[] bands)
        {
            if (bands == null || bands.Length == 0)
            {
                bands = DefaultBandsFallback();
            }

            for (int i = 0; i < bands.Length; i++)
            {
                CurveBandDefinition b = bands[i];
                bool last = i == bands.Length - 1;
                if (curvature >= b.MinCurvature && (last || curvature < b.MaxCurvature))
                {
                    return b;
                }
            }

            return bands[bands.Length - 1];
        }

        private static CurveBandDefinition[] DefaultBandsFallback()
        {
            return new[]
            {
                new CurveBandDefinition { MinCurvature = 0f, MaxCurvature = 0.02f, Difficulty01 = 0f },
                new CurveBandDefinition { MinCurvature = 0.02f, MaxCurvature = 0.05f, Difficulty01 = 0.25f },
                new CurveBandDefinition { MinCurvature = 0.05f, MaxCurvature = 0.1f, Difficulty01 = 0.6f },
                new CurveBandDefinition { MinCurvature = 0.1f, MaxCurvature = 1e6f, Difficulty01 = 1f },
            };
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

        private void AdvanceRace(SimulationFrame f, CarMotionState motion, float effectiveSpeed, float error01)
        {
            ApplyRaceProgress(f, motion, effectiveSpeed, error01);
        }

        private void ApplyRaceProgress(SimulationFrame f, CarMotionState motion, float effectiveSpeed, float error01)
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
            race.IsDrifting = error01 > f.IsDriftingThreshold;
            race.IsOvershot = error01 > f.IsOvershotThreshold;
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
    }
}
