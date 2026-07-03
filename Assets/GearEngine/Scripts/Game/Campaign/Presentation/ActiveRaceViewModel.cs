using GearEngine.Campaign.Gear;
using GearEngine.CarSimulation.PhysicsSimulation;
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
using Scaffold.Analytics;
using GearEngine.Campaign.Analytics;
using GearEngine.Core.Config.Events;
using Scaffold.Events.Contracts;

namespace GearEngine.Campaign.Presentation
{
    public partial class ActiveRaceViewModel : ViewModel
    {
        private const float ResultPopupDelaySeconds = 2f;

        [ObservableProperty]
        private TrackViewModel track;

        public CarViewModel Car { get; private set; }
        public RaceDriftScoreViewModel DriftScore { get; private set; }

        [Inject] private ITrackService trackService;
        [Inject] private IGearEngineService engineService;
        [Inject] private TrackSimulationFactory trackFactory;
        [Inject] private RaceManagerService raceManager;
        [Inject] private ISimulationRunnerService aiRunner;
        [Inject] private CampaignRaceSessionDefaults raceSessionDefaults;
        [Inject] private IAnalyticsService analyticsService;
        [Inject] private IEventBus eventBus;

        protected override void Initialize()
        {
            base.Initialize();
            
            eventBus.AddListener<GearEngine.Events.CombatTextCollectedEvent>(OnCombatTextCollected);

            RaceSessionConfig sessionConfig = raceSessionDefaults.CreateForTrack(trackService.CurrentTrack);
            RaceState freshSession = trackFactory.Create(trackService.CurrentCar, trackService.CurrentTrack, sessionConfig);
            raceManager.RegisterRace(freshSession);

            if (engineService != null)
            {
                foreach (var node in engineService.GetAllNodes())
                {
                    if (node == null) continue;
                    foreach (var ability in node.GetAbilities())
                    {
                        if (ability is ActiveRaceGearAbilitySO activeGear)
                        {
                            activeGear.Initialize(freshSession, engineService);
                        }
                    }
                }
            }

            engineService.ResetGridSimulationState();

            Track = new TrackViewModel(freshSession, raceManager, aiRunner, trackFactory);
            BindChildViewModel(Track);

            Car = new CarViewModel(freshSession, aiRunner, attachRunnerOnBind: false);
            BindChildViewModel(Car);

            DriftScore = new RaceDriftScoreViewModel(freshSession, Car);
            BindChildViewModel(DriftScore);

            Bind<SimulationLifecycleState, SimulationLifecycleState>(() => Track.State, OnTrackStateChanged);
            analyticsService?.Record(new RaceStartedEvent(trackService.CurrentTrack.name, trackService.CurrentCar.name));
        }

        public void Tick(float deltaTime)
        {
            DriftScore?.Tick(deltaTime);
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
                engineService?.Play();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ActiveRaceViewModel] StartRaceAfterCarReady failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        protected override void OnClosed()
        {
            eventBus.RemoveListener<GearEngine.Events.CombatTextCollectedEvent>(OnCombatTextCollected);

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
                RaceResultModel result = new RaceResultModel(session.RaceTime, session.CurrentLap, trackService.CurrentTrack, session.TotalDriftScore);
                analyticsService?.Record(new RaceFinishedEvent(
                    trackService.CurrentTrack.name,
                    trackService.CurrentCar.name,
                    result.RaceTime,
                    result.LapCount,
                    result.Score,
                    result.IsGoodResult
                ));

                eventBus?.Raise(new GlobalLoadingEvent(true));
                try
                {
                    await trackService.RecordResultAsync(result);
                }
                finally
                {
                    eventBus?.Raise(new GlobalLoadingEvent(false));
                }
                
                await Task.Delay(TimeSpan.FromSeconds(ResultPopupDelaySeconds));
                
                navigation.Open(new ResultPopupViewModel(result));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ActiveRaceViewModel] OnRaceCompleted failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void OnCombatTextCollected(GearEngine.Events.CombatTextCollectedEvent evt)
        {
            if (DriftScore != null)
            {
                DriftScore.CurrentPoints += evt.Score;
                DriftScore.IsDisplayingScore = true;
            }
        }
    }
}
