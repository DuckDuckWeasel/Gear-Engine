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

            RaceState preview = trackFactory.Create(trackService.CurrentCar, trackService.CurrentTrack, null);
            Track = new TrackViewModel(preview, raceManager, aiRunner, trackFactory);
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
