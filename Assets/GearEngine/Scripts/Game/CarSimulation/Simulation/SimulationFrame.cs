using System;
using GearEngine.CarSimulation;
using UnityEngine;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Entity;
using GearEngine.CarSimulation.Tracks;
using Scaffold.Entities;

namespace GearEngine.CarSimulation.Simulation
{
    internal readonly struct SimulationFrame
    {
        private SimulationFrame(
            CarMotionState motion,
            RaceRuntimeState race,
            BakedTrackProfile profile,
            TrackSample here,
            float totalLength,
            float dt,
            float maxStraightSpeed,
            float maxCurveSpeed,
            float acceleration,
            float brake,
            float handling01,
            float lookAheadMin,
            float lookAheadSpeedFactor,
            float aheadProbeStep,
            float handlingTurnRateDegPerSec,
            float recoveryRateDegPerSec,
            float maxHeadingErrorDeg,
            float speedPenaltyScale,
            float slipAngleScale,
            float lateralOffsetScale,
            float isDriftingThreshold,
            float isOvershotThreshold,
            CurveBandDefinition[] curveBands)
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
            LookAheadMin = lookAheadMin;
            LookAheadSpeedFactor = lookAheadSpeedFactor;
            AheadProbeStep = aheadProbeStep;
            HandlingTurnRateDegPerSec = handlingTurnRateDegPerSec;
            RecoveryRateDegPerSec = recoveryRateDegPerSec;
            MaxHeadingErrorDeg = maxHeadingErrorDeg;
            SpeedPenaltyScale = speedPenaltyScale;
            SlipAngleScale = slipAngleScale;
            LateralOffsetScale = lateralOffsetScale;
            IsDriftingThreshold = isDriftingThreshold;
            IsOvershotThreshold = isOvershotThreshold;
            CurveBands = curveBands;
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

        public float LookAheadMin { get; }

        public float LookAheadSpeedFactor { get; }

        public float AheadProbeStep { get; }

        public float HandlingTurnRateDegPerSec { get; }

        public float RecoveryRateDegPerSec { get; }

        public float MaxHeadingErrorDeg { get; }

        public float SpeedPenaltyScale { get; }

        public float SlipAngleScale { get; }

        public float LateralOffsetScale { get; }

        public float IsDriftingThreshold { get; }

        public float IsOvershotThreshold { get; }

        public CurveBandDefinition[] CurveBands { get; }

        internal static SimulationFrame Create(TrackSimulation sim, float dt)
        {
            if (sim.Tuning == null)
            {
                throw new ArgumentNullException(nameof(sim.Tuning));
            }

            CarMotionState motion = sim.Motion;
            BakedTrackProfile profile = sim.Profile;
            TrackSimulationTuning tuningAsset = sim.Tuning;
            CarStatScalars stats = FromCar(sim.Car, sim.Variables);
            TrackSample here = profile.Evaluate(motion.Distance);
            motion.SampleIndex = profile.FindSampleIndexNear(motion.Distance);
            float handling01 = Mathf.Clamp01(stats.Handling / Mathf.Max(1e-6f, tuningAsset.HandlingNormalizationScale));
            CurveBandDefinition[] bands = tuningAsset.CurveBands;
            if (bands == null || bands.Length == 0)
            {
                bands = Array.Empty<CurveBandDefinition>();
            }

            return new SimulationFrame(
                motion,
                sim.Race,
                profile,
                here,
                profile.TotalLength,
                dt,
                stats.MaxStraightSpeed,
                stats.MaxCurveSpeed,
                stats.Acceleration,
                stats.Brake,
                handling01,
                tuningAsset.LookAheadMinMetres,
                tuningAsset.LookAheadSpeedFactor,
                tuningAsset.AheadProbeStep,
                tuningAsset.HandlingTurnRateDegPerSec,
                tuningAsset.RecoveryRateDegPerSec,
                tuningAsset.MaxHeadingErrorDeg,
                tuningAsset.SpeedPenaltyScale,
                tuningAsset.SlipAngleScale,
                tuningAsset.LateralOffsetScale,
                tuningAsset.IsDriftingThreshold,
                tuningAsset.IsOvershotThreshold,
                bands);
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
