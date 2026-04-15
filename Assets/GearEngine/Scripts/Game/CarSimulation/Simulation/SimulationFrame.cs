using System;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Entity;
using GearEngine.CarSimulation.Tracks;
using Scaffold.Entities;

namespace GearEngine.CarSimulation.Simulation
{
    internal readonly struct SimulationFrame
    {
        private SimulationFrame(CarMotionState motion, RaceRuntimeState race, BakedTrackProfile profile, TrackSample here, float totalLength, float dt, float maxStraightSpeed, float maxCurveSpeed, float acceleration, float brake, float handling01, float overshootPenaltyScale, float activeCapCurvatureSpan, float lookAheadMin, float lookAheadSpeedFactor, float aheadProbeStep, float curvatureEpsilon, float curvatureStressMultiplier, float speedStressScale, float speedStressToLineErrorScale, float lineDifficultyFromCurvature, float lineDifficultyFromSpeed, float maxAbsorbableDifficulty, float lineErrorBuildRate, float lineErrorDecayRate, float slipAngleScale, float lateralOffsetScale, float slipBuildRate, float offsetBuildRate, float slipRecoveryRate, float offsetRecoveryRate, float isDriftingThreshold, float isOvershotThreshold)
        {
            Motion = motion;
            Race = race;
            Profile = profile;
            Here = here;
            TotalLength = totalLength;
            Dt = dt;
            MaxStraightSpeed = maxStraightSpeed;
            MaxCurveSpeed = maxCurveSpeed;
            Acceleration = acceleration;
            Brake = brake;
            Handling01 = handling01;
            OvershootPenaltyScale = overshootPenaltyScale;
            ActiveCapCurvatureSpan = activeCapCurvatureSpan;
            LookAheadMin = lookAheadMin;
            LookAheadSpeedFactor = lookAheadSpeedFactor;
            AheadProbeStep = aheadProbeStep;
            CurvatureEpsilon = curvatureEpsilon;
            CurvatureStressMultiplier = curvatureStressMultiplier;
            SpeedStressScale = speedStressScale;
            SpeedStressToLineErrorScale = speedStressToLineErrorScale;
            LineDifficultyFromCurvature = lineDifficultyFromCurvature;
            LineDifficultyFromSpeed = lineDifficultyFromSpeed;
            MaxAbsorbableDifficulty = maxAbsorbableDifficulty;
            LineErrorBuildRate = lineErrorBuildRate;
            LineErrorDecayRate = lineErrorDecayRate;
            SlipAngleScale = slipAngleScale;
            LateralOffsetScale = lateralOffsetScale;
            SlipBuildRate = slipBuildRate;
            OffsetBuildRate = offsetBuildRate;
            SlipRecoveryRate = slipRecoveryRate;
            OffsetRecoveryRate = offsetRecoveryRate;
            IsDriftingThreshold = isDriftingThreshold;
            IsOvershotThreshold = isOvershotThreshold;
        }

        public CarMotionState Motion { get; }

        public RaceRuntimeState Race { get; }

        public BakedTrackProfile Profile { get; }

        public TrackSample Here { get; }

        public float TotalLength { get; }

        public float Dt { get; }

        public float MaxStraightSpeed { get; }

        public float MaxCurveSpeed { get; }

        public float Acceleration { get; }

        public float Brake { get; }

        public float Handling01 { get; }

        public float OvershootPenaltyScale { get; }

        public float ActiveCapCurvatureSpan { get; }

        public float LookAheadMin { get; }

        public float LookAheadSpeedFactor { get; }

        public float AheadProbeStep { get; }

        public float CurvatureEpsilon { get; }

        public float CurvatureStressMultiplier { get; }

        public float SpeedStressScale { get; }

        public float SpeedStressToLineErrorScale { get; }

        public float LineDifficultyFromCurvature { get; }

        public float LineDifficultyFromSpeed { get; }

        public float MaxAbsorbableDifficulty { get; }

        public float LineErrorBuildRate { get; }

        public float LineErrorDecayRate { get; }

        public float SlipAngleScale { get; }

        public float LateralOffsetScale { get; }

        public float SlipBuildRate { get; }

        public float OffsetBuildRate { get; }

        public float SlipRecoveryRate { get; }

        public float OffsetRecoveryRate { get; }

        public float IsDriftingThreshold { get; }

        public float IsOvershotThreshold { get; }

        internal static SimulationFrame Create(TrackSimulation sim, float dt)
        {
            if (sim.Tuning == null)
            {
                throw new ArgumentNullException(nameof(sim.Tuning));
            }

            CarMotionState motion = sim.Motion;
            BakedTrackProfile profile = sim.Profile;
            TrackSimulationTuning tuningAsset = sim.Tuning;
            TuningScalars tuning = FromTuning(tuningAsset);
            CarStatScalars stats = FromCar(sim.Car, sim.Variables);
            TrackSample here = profile.Evaluate(motion.Distance);
            motion.SampleIndex = profile.FindSampleIndexNear(motion.Distance);
            float handling01 = stats.Handling / tuningAsset.HandlingNormalizationScale;
            return new SimulationFrame(motion, sim.Race, profile, here, profile.TotalLength, dt, stats.MaxStraightSpeed, stats.MaxCurveSpeed, stats.Acceleration, stats.Brake, handling01, tuning.OvershootPenaltyScale, tuning.ActiveCapCurvatureSpan, tuning.LookAheadMin, tuning.LookAheadSpeedFactor, tuning.AheadProbeStep, tuning.CurvatureEpsilon, tuning.CurvatureStressMultiplier, tuning.SpeedStressScale, tuning.SpeedStressToLineErrorScale, tuning.LineDifficultyFromCurvature, tuning.LineDifficultyFromSpeed, tuning.MaxAbsorbableDifficulty, tuning.LineErrorBuildRate, tuning.LineErrorDecayRate, tuning.SlipAngleScale, tuning.LateralOffsetScale, tuning.SlipBuildRate, tuning.OffsetBuildRate, tuning.SlipRecoveryRate, tuning.OffsetRecoveryRate, tuning.IsDriftingThreshold, tuning.IsOvershotThreshold);
        }

        private static TuningScalars FromTuning(TrackSimulationTuning t)
        {
            return new TuningScalars(t.OvershootPenaltyScale, t.ActiveCapCurvatureSpan, t.LookAheadMinMetres, t.LookAheadSpeedFactor, t.AheadProbeStep, t.CurvatureEpsilon, t.CurvatureStressMultiplier, t.SpeedStressScale, t.SpeedStressToLineErrorScale, t.LineDifficultyFromCurvature, t.LineDifficultyFromSpeed, t.MaxAbsorbableDifficulty, t.LineErrorBuildRate, t.LineErrorDecayRate, t.SlipAngleScale, t.LateralOffsetScale, t.SlipBuildRate, t.OffsetBuildRate, t.SlipRecoveryRate, t.OffsetRecoveryRate, t.IsDriftingThreshold, t.IsOvershotThreshold);
        }

        private static CarStatScalars FromCar(CarEntity car, CarVariableSet vars)
        {
            if (car == null || vars == null)
            {
                return new CarStatScalars(30f, 14f, 10f, 15f, 10f);
            }

            return new CarStatScalars(FromVariable(car, vars.MaxStraightSpeed), FromVariable(car, vars.MaxCurveSpeed), FromVariable(car, vars.Acceleration), FromVariable(car, vars.Brake), FromVariable(car, vars.Handling));
        }

        private static float FromVariable(CarEntity car, VariableSO variable)
        {
            if (variable == null)
            {
                return 0f;
            }

            return car.GetValue<float>(variable);
        }

        private readonly struct TuningScalars
        {
            internal TuningScalars(float overshootPenaltyScale, float activeCapCurvatureSpan, float lookAheadMin, float lookAheadSpeedFactor, float aheadProbeStep, float curvatureEpsilon, float curvatureStressMultiplier, float speedStressScale, float speedStressToLineErrorScale, float lineDifficultyFromCurvature, float lineDifficultyFromSpeed, float maxAbsorbableDifficulty, float lineErrorBuildRate, float lineErrorDecayRate, float slipAngleScale, float lateralOffsetScale, float slipBuildRate, float offsetBuildRate, float slipRecoveryRate, float offsetRecoveryRate, float isDriftingThreshold, float isOvershotThreshold)
            {
                OvershootPenaltyScale = overshootPenaltyScale;
                ActiveCapCurvatureSpan = activeCapCurvatureSpan;
                LookAheadMin = lookAheadMin;
                LookAheadSpeedFactor = lookAheadSpeedFactor;
                AheadProbeStep = aheadProbeStep;
                CurvatureEpsilon = curvatureEpsilon;
                CurvatureStressMultiplier = curvatureStressMultiplier;
                SpeedStressScale = speedStressScale;
                SpeedStressToLineErrorScale = speedStressToLineErrorScale;
                LineDifficultyFromCurvature = lineDifficultyFromCurvature;
                LineDifficultyFromSpeed = lineDifficultyFromSpeed;
                MaxAbsorbableDifficulty = maxAbsorbableDifficulty;
                LineErrorBuildRate = lineErrorBuildRate;
                LineErrorDecayRate = lineErrorDecayRate;
                SlipAngleScale = slipAngleScale;
                LateralOffsetScale = lateralOffsetScale;
                SlipBuildRate = slipBuildRate;
                OffsetBuildRate = offsetBuildRate;
                SlipRecoveryRate = slipRecoveryRate;
                OffsetRecoveryRate = offsetRecoveryRate;
                IsDriftingThreshold = isDriftingThreshold;
                IsOvershotThreshold = isOvershotThreshold;
            }

            internal float OvershootPenaltyScale { get; }
            internal float ActiveCapCurvatureSpan { get; }
            internal float LookAheadMin { get; }
            internal float LookAheadSpeedFactor { get; }
            internal float AheadProbeStep { get; }
            internal float CurvatureEpsilon { get; }
            internal float CurvatureStressMultiplier { get; }
            internal float SpeedStressScale { get; }
            internal float SpeedStressToLineErrorScale { get; }
            internal float LineDifficultyFromCurvature { get; }
            internal float LineDifficultyFromSpeed { get; }
            internal float MaxAbsorbableDifficulty { get; }
            internal float LineErrorBuildRate { get; }
            internal float LineErrorDecayRate { get; }
            internal float SlipAngleScale { get; }
            internal float LateralOffsetScale { get; }
            internal float SlipBuildRate { get; }
            internal float OffsetBuildRate { get; }
            internal float SlipRecoveryRate { get; }
            internal float OffsetRecoveryRate { get; }
            internal float IsDriftingThreshold { get; }
            internal float IsOvershotThreshold { get; }
        }

        private readonly struct CarStatScalars
        {
            internal CarStatScalars(float maxStraightSpeed, float maxCurveSpeed, float acceleration, float brake, float handling)
            {
                MaxStraightSpeed = maxStraightSpeed;
                MaxCurveSpeed = maxCurveSpeed;
                Acceleration = acceleration;
                Brake = brake;
                Handling = handling;
            }

            internal float MaxStraightSpeed { get; }
            internal float MaxCurveSpeed { get; }
            internal float Acceleration { get; }
            internal float Brake { get; }
            internal float Handling { get; }
        }
    }
}
