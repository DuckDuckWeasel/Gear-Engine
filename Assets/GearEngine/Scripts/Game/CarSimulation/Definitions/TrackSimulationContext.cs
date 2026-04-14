using GearEngine.CarSimulation.Entity;
using GearEngine.CarSimulation.Tracks;

namespace GearEngine.CarSimulation.Definitions
{
    internal sealed class TrackSimulationContext
    {
        public TrackSimulationContext(
            TrackDefinition track,
            CarEntity car,
            BakedTrackProfile profile,
            CarVariableSet variables,
            TrackSimulationTuning tuning)
        {
            Track = track;
            Car = car;
            Profile = profile;
            Variables = variables;
            Tuning = tuning;
        }

        public TrackDefinition Track { get; }

        public CarEntity Car { get; }

        public BakedTrackProfile Profile { get; }

        public CarVariableSet Variables { get; }

        public TrackSimulationTuning Tuning { get; }
    }
}
