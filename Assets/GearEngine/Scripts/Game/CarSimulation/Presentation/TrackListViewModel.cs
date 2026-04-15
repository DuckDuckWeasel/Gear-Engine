using System.Collections.Generic;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using Scaffold.MVVM;

namespace GearEngine.CarSimulation.Presentation
{
    public sealed class TrackListViewModel : ViewModel
    {
        public TrackDefinition TrackDefinition { get; }

        public IReadOnlyList<TrackSimulation> Simulations { get; }

        public TrackListViewModel(TrackDefinition trackDefinition, IReadOnlyList<TrackSimulation> simulations)
        {
            TrackDefinition = trackDefinition;
            Simulations = simulations;
        }
    }
}
