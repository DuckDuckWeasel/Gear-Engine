using System.Collections.Generic;
using GearEngine.CarSimulation;
using Scaffold.MVVM;

namespace GearEngine.CarSimulation.Presentation
{
    public sealed class TrackListViewModel : ViewModel
    {
        public TrackListViewModel(IReadOnlyList<LapRaceSession> sessions)
        {
            Sessions = sessions;
        }

        public IReadOnlyList<LapRaceSession> Sessions { get; }
    }
}
