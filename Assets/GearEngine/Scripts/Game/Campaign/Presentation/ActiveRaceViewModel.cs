using System;
using CommunityToolkit.Mvvm.ComponentModel;
using GearEngine.Campaign;
using GearEngine.Campaign.Services;
using GearEngine.CarSimulation;
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

        [Inject] private ITrackService trackService;
        [Inject] private IWalletService walletService;
        [Inject] private IGearEngineService engineService;
        [Inject] private TrackSimulationFactory trackFactory;
        [Inject] private IRaceSessionRunner raceSessionRunner;



        protected override void Initialize()
        {
            base.Initialize();

            LapRaceSession freshSession = trackFactory.Create(trackService.CurrentCar, trackService.CurrentTrack);
            trackService.SetCurrentSession(freshSession);
            raceSessionRunner.SetSession(freshSession);

            engineService.ResetGridSimulationState();

            Track = new TrackViewModel(trackService.CurrentSession, spawnCarOnBindIfNoChild: true);
            BindChildViewModel(Track);

            engineService.Play();
            Track.Toggle(true);

            Bind<SimulationLifecycleState, SimulationLifecycleState>(() => Track.State, OnTrackStateChanged);
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
            try
            {
                engineService.ResetGridSimulationState();

                LapRaceSession session = trackService.CurrentSession;
                RaceResultModel result = new RaceResultModel(session.RaceTime, session.CurrentLap, trackService.CurrentTrack);
                walletService.AddGold(result.Gold.Amount);
                trackService.RecordResult(result);
                navigation.Open(new ResultPopupViewModel(result));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ActiveRaceViewModel] OnRaceCompleted failed: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
