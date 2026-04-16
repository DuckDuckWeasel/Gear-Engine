using System.Collections.Generic;
using GearEngine.CarSimulation.Definitions;
using Scaffold.MVVM;

namespace GearEngine.CarSimulation.Presentation
{
    public sealed class TrackListViewModel : ViewModel
    {
        public TrackListViewModel(TrackDefinition trackDefinition, IReadOnlyList<LapRaceSession> sessions)
        {
            TrackDefinition = trackDefinition;
            Sessions = sessions;
        }

        public TrackDefinition TrackDefinition { get; }

        public IReadOnlyList<LapRaceSession> Sessions { get; }
    }
}
