using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Entity;
using Scaffold.MVVM;
using Scaffold.MVVM.Binding;
using GearEngine.CarSimulation.Simulation;

namespace GearEngine.CarSimulation.Presentation
{
    public sealed partial class TrackViewModel : ViewModel
    {
        public RaceState Session { get; }

        public TrackDefinition Track => Session.Track;

        public CarEntity Car => Session.Car;

        public IReadOnlyList<CarViewModel> CarViewModels { get; private set; }

        [ObservableProperty]
        private SimulationLifecycleState state;

        [ObservableProperty]
        private float hudRaceTime;

        [ObservableProperty]
        private int hudCurrentLap;

        private readonly RaceManagerService raceManager;
        public SplineCarRunnerService AiRunner { get; }
        public TrackSimulationFactory Factory { get; }

        public TrackViewModel(RaceState session, RaceManagerService raceManager, SplineCarRunnerService aiRunner, TrackSimulationFactory factory)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
            this.raceManager = raceManager;
            AiRunner = aiRunner;
            Factory = factory;
        }

        protected override void Initialize()
        {
            base.Initialize();
            Session.PresentationChanged += OnSessionPresentationChanged;
            
            var list = new System.Collections.Generic.List<CarViewModel>();
            var cvm = new CarViewModel(Session, AiRunner);
            BindChildViewModel(cvm);
            list.Add(cvm);
            CarViewModels = list;

            RefreshUiState();
        }

        public void Toggle(bool running)
        {
            if (running)
            {
                if (Session.Phase == SimulationLifecycleState.Completed)
                {
                    Session.Reset();
                }

                raceManager?.StartRace(Session);
            }
            else
            {
                raceManager?.StopRace(Session);
            }

            RefreshUiState();
        }

        public void Complete()
        {
            raceManager?.ForceFinish(Session);
            RefreshUiState();
        }

        internal void TearDown()
        {
            Session.PresentationChanged -= OnSessionPresentationChanged;
        }

        private void OnSessionPresentationChanged()
        {
            RefreshUiState();
        }

        private void RefreshUiState()
        {
            State = Session.Phase;
            HudRaceTime = Session.RaceTime;
            HudCurrentLap = Session.CurrentLap;
        }
    }
}
