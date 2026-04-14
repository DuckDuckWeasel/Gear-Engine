using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Track;

namespace GearEngine.CarSimulation.Simulation
{
    internal readonly struct SimulationFrame
    {
        public CarMotionState Motion { get; }

        public RaceRuntimeState Race { get; }

        public BakedTrackProfile Profile { get; }

        public ResolvedSimulationInputs Inputs { get; }

        public TrackSample Here { get; }

        public float TotalLength { get; }

        public float Dt { get; }

        private SimulationFrame(
            CarMotionState motion,
            RaceRuntimeState race,
            BakedTrackProfile profile,
            ResolvedSimulationInputs inputs,
            TrackSample here,
            float totalLength,
            float dt)
        {
            Motion = motion;
            Race = race;
            Profile = profile;
            Inputs = inputs;
            Here = here;
            TotalLength = totalLength;
            Dt = dt;
        }

        internal static SimulationFrame Create(TrackSimulation sim, float dt)
        {
            TrackSimulationContext ctx = sim.Context;
            CarMotionState motion = sim.Motion;
            BakedTrackProfile profile = ctx.Profile;
            ResolvedSimulationInputs inputs = ResolvedSimulationInputs.From(ctx, ctx.Car);
            TrackSample here = profile.Evaluate(motion.Distance);
            motion.SampleIndex = profile.FindSampleIndexNear(motion.Distance);
            return new SimulationFrame(motion, sim.Race, profile, inputs, here, profile.TotalLength, dt);
        }
    }
}
