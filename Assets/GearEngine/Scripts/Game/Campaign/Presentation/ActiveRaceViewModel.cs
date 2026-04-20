using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using GearEngine.Campaign;
using GearEngine.Campaign.Bootstrap;
using GearEngine.Campaign.Services;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Presentation;
using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine;
using Scaffold.MVVM;
using Scaffold.Navigation.Contracts;
using UnityEngine;
using VContainer;

namespace GearEngine.Campaign.Presentation
{
    public partial class ActiveRaceViewModel : ViewModel
    {
        [ObservableProperty]
        private TrackViewModel track;

        public CarViewModel Car { get; private set; }

        [Inject] private ITrackService trackService;
        [Inject] private IGearEngineService engineService;
        [Inject] private TrackSimulationFactory trackFactory;
        [Inject] private RaceManagerService raceManager;
        [Inject] private SplineCarRunnerService aiRunner;
        [Inject] private CampaignRaceSessionDefaults raceSessionDefaults;

        protected override void Initialize()
        {
            base.Initialize();

            RaceSessionConfig sessionConfig = raceSessionDefaults.CreateForTrack(trackService.CurrentTrack);
            RaceState freshSession = trackFactory.Create(trackService.CurrentCar, trackService.CurrentTrack, sessionConfig);
            raceManager.RegisterRace(freshSession);

            engineService.ResetGridSimulationState();

            Track = new TrackViewModel(freshSession, raceManager, aiRunner, trackFactory);
            BindChildViewModel(Track);

            Car = new CarViewModel(freshSession, aiRunner, attachRunnerOnBind: false);
            BindChildViewModel(Car);

            engineService.Play();

            Bind<SimulationLifecycleState, SimulationLifecycleState>(() => Track.State, OnTrackStateChanged);
        }

        /// <summary>Called from <see cref="ActiveRaceView"/> after the car is spawned and <see cref="CarView.AttachRunner"/> runs.</summary>
        public void StartRaceAfterCarReady()
        {
            try
            {
                if (Track != null)
                {
                    Track.Toggle(true);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ActiveRaceViewModel] StartRaceAfterCarReady failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        protected override void OnClosed()
        {
            try
            {
                if (Track?.Session != null)
                {
                    raceManager.UnregisterRace(Track.Session);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ActiveRaceViewModel] OnClosed failed: {ex.Message}\n{ex.StackTrace}");
            }

            base.OnClosed();
        }

        private void OnTrackStateChanged(SimulationLifecycleState state)
        {
            if (state == SimulationLifecycleState.Completed)
            {
                OnRaceCompleted();
            }
        }

        private void OnRaceCompleted()
        {
            _ = OnRaceCompletedAsync();
        }

        private async Task OnRaceCompletedAsync()
        {
            try
            {
                engineService.ResetGridSimulationState();

                RaceState session = Track.Session;
                RaceResultModel result = new RaceResultModel(session.RaceTime, session.CurrentLap, trackService.CurrentTrack);
                await trackService.RecordResultAsync(result);
                navigation.Open(new ResultPopupViewModel(result));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ActiveRaceViewModel] OnRaceCompleted failed: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
