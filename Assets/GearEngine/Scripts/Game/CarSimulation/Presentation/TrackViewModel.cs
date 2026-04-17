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
        public TrackViewModel(LapRaceSession session)
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public TrackDefinition Track => session.Track;

        public CarEntity Car => session.Car;

        public LapRaceSession Session => session;

        [ObservableProperty]
        private SimulationLifecycleState state;

        private readonly LapRaceSession session;

        protected override void Initialize()
        {
            base.Initialize();
            session.PresentationChanged += OnSessionPresentationChanged;
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
        }

        private void OnSessionPresentationChanged()
        {
            RefreshUiState();
        }

        private void RefreshUiState()
        {
            State = session.Phase;
        }
    }
}
