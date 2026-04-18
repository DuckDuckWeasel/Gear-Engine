using System;
using GearEngine.Campaign.Services;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Presentation;
using GearEngine.CarSimulation.Simulation;
using Scaffold.MVVM;
using Scaffold.Navigation.Contracts;
using UnityEngine;
using VContainer;

namespace GearEngine.Campaign.Presentation
{
    public sealed class MainViewModel : ViewModel
    {
        public TrackViewModel Track { get; private set; }
        public TrackStatsViewModel Stats { get; private set; }

        [Inject] private ITrackService trackService;
        [Inject] private TrackSimulationFactory trackFactory;
        [Inject] private IRaceSessionRunner raceSessionRunner;

        protected override void Initialize()
        {
            base.Initialize();

            raceSessionRunner.SetSession(null);

            LapRaceSession current = trackService.CurrentSession;
            if (current == null || current.Phase == SimulationLifecycleState.Completed)
            {
                LapRaceSession preview = trackFactory.Create(trackService.CurrentCar, trackService.CurrentTrack);
                trackService.SetCurrentSession(preview);
            }

            Track = new TrackViewModel(trackService.CurrentSession);
            BindChildViewModel(Track);
            Stats = new TrackStatsViewModel(trackService);
            BindChildViewModel(Stats);
        }

        public void ClickedPlay()
        {
            try
            {
                navigation.Open(new SetupViewModel());
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MainViewModel] GoToSetup failed: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
