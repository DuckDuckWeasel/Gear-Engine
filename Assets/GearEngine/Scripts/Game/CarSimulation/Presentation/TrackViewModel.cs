using System;
using GearEngine.CarSimulation.PhysicsSimulation;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Entity;
using Scaffold.MVVM;
using Scaffold.MVVM.Binding;
using GearEngine.CarSimulation.Simulation;

namespace GearEngine.CarSimulation.Presentation
{
    public sealed partial class TrackViewModel : ViewModel, ITrackDefinitionSource
    {
        public RaceState Session { get; }

        public TrackDefinition Track => Session.Track;

        public CarEntity Car => Session.Car;

        public IReadOnlyList<CarViewModel> CarViewModels { get; private set; }

        [ObservableProperty]
        private SimulationLifecycleState state;

        private readonly RaceManagerService raceManager;
        public ISimulationRunnerService AiRunner { get; }
        public TrackSimulationFactory Factory { get; }

        public TrackViewModel(RaceState session, RaceManagerService raceManager, ISimulationRunnerService aiRunner, TrackSimulationFactory factory)
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
            CarViewModels = Array.Empty<CarViewModel>();

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
        }
    }
}
