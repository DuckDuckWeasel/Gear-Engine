using System;
using CommunityToolkit.Mvvm.ComponentModel;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Entity;
using Scaffold.MVVM;
using Scaffold.MVVM.Binding;

namespace GearEngine.CarSimulation.Presentation
{
    public sealed partial class TrackViewModel : ViewModel
    {
        public TrackViewModel(LapRaceSession session, bool spawnCarOnBindIfNoChild = false, bool spawnCarWhenSessionStartsRunning = false)
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
            SpawnCarOnBindIfNoChild = spawnCarOnBindIfNoChild;
            SpawnCarWhenSessionStartsRunning = spawnCarWhenSessionStartsRunning;
        }

        public TrackDefinition Track => session.Track;

        public CarEntity Car => session.Car;

        public LapRaceSession Session => session;

        public bool SpawnCarOnBindIfNoChild { get; }

        public bool SpawnCarWhenSessionStartsRunning { get; }

        [ObservableProperty]
        private SimulationLifecycleState state;

        [ObservableProperty]
        private float hudRaceTime;

        [ObservableProperty]
        private int hudCurrentLap;

        private readonly LapRaceSession session;

        protected override void Initialize()
        {
            base.Initialize();
            session.PresentationChanged += OnSessionPresentationChanged;
            session.AfterTick += OnSessionAfterTick;
            RefreshUiState();
        }

        public void Toggle(bool running)
        {
            if (running)
            {
                if (session.Phase == SimulationLifecycleState.Completed)
                {
                    session.Reset();
                }

                session.SetClockRunning(true);
            }
            else
            {
                session.SetClockRunning(false);
            }

            RefreshUiState();
        }

        public void Complete()
        {
            session.ForceFinish();
            RefreshUiState();
        }

        internal void TearDown()
        {
            session.PresentationChanged -= OnSessionPresentationChanged;
            session.AfterTick -= OnSessionAfterTick;
        }

        private void OnSessionPresentationChanged()
        {
            RefreshUiState();
        }

        private void OnSessionAfterTick()
        {
            RefreshHudMetrics();
        }

        private void RefreshUiState()
        {
            State = session.Phase;
            RefreshHudMetrics();
        }

        private void RefreshHudMetrics()
        {
            HudRaceTime = session.RaceTime;
            HudCurrentLap = session.CurrentLap;
        }
    }
}
