using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Entity;
using GearEngine.CarSimulation.Tracks;
using Scaffold.Entities;

namespace GearEngine.CarSimulation.Simulation
{
    internal readonly struct SimulationFrame
    {
        private SimulationFrame(CarMotionState motion, RaceRuntimeState race, BakedTrackProfile profile, TrackSample here, float totalLength, float dt, float topSpeed, float acceleration, float brake, float handling, float stability, float recovery, float driftPenaltyScale, float gripScale, float lookAheadMin, float lookAheadSpeedFactor, float aheadProbeStep, float curvatureEpsilon)
        {
            Motion = motion;
            Race = race;
            Profile = profile;
            Here = here;
            TotalLength = totalLength;
            Dt = dt;
            TopSpeed = topSpeed;
            Acceleration = acceleration;
            Brake = brake;
            Handling = handling;
            Stability = stability;
            Recovery = recovery;
            DriftPenaltyScale = driftPenaltyScale;
            GripScale = gripScale;
            LookAheadMin = lookAheadMin;
            LookAheadSpeedFactor = lookAheadSpeedFactor;
            AheadProbeStep = aheadProbeStep;
            CurvatureEpsilon = curvatureEpsilon;
        }

        public CarMotionState Motion { get; }

        public RaceRuntimeState Race { get; }

        public BakedTrackProfile Profile { get; }

        public TrackSample Here { get; }

        public float TotalLength { get; }

        public float Dt { get; }

        public float TopSpeed { get; }

        public float Acceleration { get; }

        public float Brake { get; }

        public float Handling { get; }

        public float Stability { get; }

        public float Recovery { get; }

        public float DriftPenaltyScale { get; }

        public float GripScale { get; }

        public float LookAheadMin { get; }

        public float LookAheadSpeedFactor { get; }

        public float AheadProbeStep { get; }

        public float CurvatureEpsilon { get; }

        internal static SimulationFrame Create(TrackSimulation sim, float dt)
        {
            CarMotionState motion = sim.Motion;
            BakedTrackProfile profile = sim.Profile;
            TuningScalars tuning = FromTuning(sim.Tuning);
            CarStatScalars stats = FromCar(sim.Car, sim.Variables);
            TrackSample here = profile.Evaluate(motion.Distance);
            motion.SampleIndex = profile.FindSampleIndexNear(motion.Distance);
            return new SimulationFrame(motion, sim.Race, profile, here, profile.TotalLength, dt, stats.TopSpeed, stats.Acceleration, stats.Brake, stats.Handling, stats.Stability, stats.Recovery, stats.DriftPenaltyScale, tuning.GripScale, tuning.LookAheadMin, tuning.LookAheadSpeedFactor, tuning.AheadProbeStep, tuning.CurvatureEpsilon);
        }

        private static TuningScalars FromTuning(TrackSimulationTuning t)
        {
            return t != null
                ? new TuningScalars(t.GripScale, t.LookAheadMinMetres, t.LookAheadSpeedFactor, t.AheadProbeStep, t.CurvatureEpsilon)
                : new TuningScalars(0.12f, 8f, 0.75f, 0.25f, 1e-5f);
        }

        private static CarStatScalars FromCar(CarEntity car, CarVariableSet vars)
        {
            return new CarStatScalars(FromVariable(car, vars.Speed), FromVariable(car, vars.Acceleration), FromVariable(car, vars.Brake), FromVariable(car, vars.Handling), FromVariable(car, vars.Stability), FromVariable(car, vars.Recovery), FromVariable(car, vars.DriftPenalty));
        }

        private static float FromVariable(CarEntity car, VariableSO variable)
        {
            return car.GetValue<float>(variable);
        }

        private readonly struct TuningScalars
        {
            internal TuningScalars(float gripScale, float lookAheadMin, float lookAheadSpeedFactor, float aheadProbeStep, float curvatureEpsilon)
            {
                GripScale = gripScale;
                LookAheadMin = lookAheadMin;
                LookAheadSpeedFactor = lookAheadSpeedFactor;
                AheadProbeStep = aheadProbeStep;
                CurvatureEpsilon = curvatureEpsilon;
            }

            internal float GripScale { get; }
            internal float LookAheadMin { get; }
            internal float LookAheadSpeedFactor { get; }
            internal float AheadProbeStep { get; }
            internal float CurvatureEpsilon { get; }
        }

        private readonly struct CarStatScalars
        {
            internal CarStatScalars(float topSpeed, float acceleration, float brake, float handling, float stability, float recovery, float driftPenaltyScale)
            {
                TopSpeed = topSpeed;
                Acceleration = acceleration;
                Brake = brake;
                Handling = handling;
                Stability = stability;
                Recovery = recovery;
                DriftPenaltyScale = driftPenaltyScale;
            }

            internal float TopSpeed { get; }
            internal float Acceleration { get; }
            internal float Brake { get; }
            internal float Handling { get; }
            internal float Stability { get; }
            internal float Recovery { get; }
            internal float DriftPenaltyScale { get; }
        }
    }
}
