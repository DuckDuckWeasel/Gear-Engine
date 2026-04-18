using System.Collections.Generic;
using GearEngine.CarSimulation;
using Scaffold.MVVM;

namespace GearEngine.CarSimulation.Presentation
{
    public sealed class TrackListViewModel : ViewModel
    {
        public TrackListViewModel(IReadOnlyList<Simulation.RaceState> sessions, TrackSimulationFactory factory, Simulation.SplineCarRunnerService aiRunner, Simulation.RaceManagerService raceManager)
        {
            Sessions = sessions;
            Factory = factory;
            AiRunner = aiRunner;
            RaceManager = raceManager;
        }

        public IReadOnlyList<Simulation.RaceState> Sessions { get; }
        public TrackSimulationFactory Factory { get; }
        public Simulation.SplineCarRunnerService AiRunner { get; }
        public Simulation.RaceManagerService RaceManager { get; }

        public void ToggleRace()
        {
            if (Sessions.Count == 0) return;
            var session = Sessions[0];
            
            if (session.Phase == SimulationLifecycleState.Running)
            {
                RaceManager.StopRace(session);
            }
            else
            {
                if (session.Phase == SimulationLifecycleState.Completed)
                {
                    session.Reset();
                }
                RaceManager.StartRace(session);
            }
        }
    }
}
