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
        [Inject] private RaceManagerService raceManager;
        [Inject] private SplineCarRunnerService aiRunner;

        protected override void Initialize()
        {
            base.Initialize();

            RaceState current = trackService.CurrentSession;
            if (current == null || current.Phase == SimulationLifecycleState.Completed)
            {
                if (current != null)
                {
                    raceManager.UnregisterRace(current);
                }

                RaceState preview = trackFactory.Create(trackService.CurrentCar, trackService.CurrentTrack, null);
                trackService.SetCurrentSession(preview);
            }

            raceManager.RegisterRace(trackService.CurrentSession);

            Track = new TrackViewModel(trackService.CurrentSession, raceManager, aiRunner, trackFactory);
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
