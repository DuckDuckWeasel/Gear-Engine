using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Entity;
using Scaffold.Entities;

namespace GearEngine.CarSimulation.Simulation
{
    internal readonly struct ResolvedSimulationInputs
    {
        public readonly float TopSpeed;

        public readonly float Acceleration;

        public readonly float Brake;

        public readonly float Handling;

        public readonly float Stability;

        public readonly float Recovery;

        public readonly float DriftPenaltyScale;

        public readonly float GripScale;

        public readonly float LookAheadMin;

        public readonly float LookAheadSpeedFactor;

        public readonly float AheadProbeStep;

        public readonly float CurvatureEpsilon;

        private ResolvedSimulationInputs(
            float topSpeed,
            float acceleration,
            float brake,
            float handling,
            float stability,
            float recovery,
            float driftPenaltyScale,
            float gripScale,
            float lookAheadMin,
            float lookAheadSpeedFactor,
            float aheadProbeStep,
            float curvatureEpsilon)
        {
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

        internal static ResolvedSimulationInputs From(TrackSimulationContext ctx, CarEntity car)
        {
            CarVariableSet vars = ctx.Variables;
            TrackSimulationTuning t = ctx.Tuning;
            float gripScale = t != null ? t.GripScale : 0.12f;
            float lookAheadMin = t != null ? t.LookAheadMinMetres : 8f;
            float lookAheadSpeedFactor = t != null ? t.LookAheadSpeedFactor : 0.75f;
            float aheadProbeStep = t != null ? t.AheadProbeStep : 0.25f;
            float curvatureEpsilon = t != null ? t.CurvatureEpsilon : 1e-5f;

            return new ResolvedSimulationInputs(
                GetFloat(car, vars.Speed),
                GetFloat(car, vars.Acceleration),
                GetFloat(car, vars.Brake),
                GetFloat(car, vars.Handling),
                GetFloat(car, vars.Stability),
                GetFloat(car, vars.Recovery),
                GetFloat(car, vars.DriftPenalty),
                gripScale,
                lookAheadMin,
                lookAheadSpeedFactor,
                aheadProbeStep,
                curvatureEpsilon);
        }

        private static float GetFloat(CarEntity car, VariableSO variable)
        {
            return car.GetValue<float>(variable);
        }
    }
}
