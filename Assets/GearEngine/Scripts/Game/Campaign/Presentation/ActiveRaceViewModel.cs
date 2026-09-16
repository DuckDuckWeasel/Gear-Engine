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
using GearEngine.GearEngine.Presentation.UI;
using GearEngine.GearEngine.Services;
using GearEngine.GearEngine.Services.Board;
using Scaffold.MVVM;
using Scaffold.Navigation.Contracts;
using UnityEngine;
using VContainer;
using Scaffold.Analytics;
using GearEngine.Campaign.Analytics;
using GearEngine.Core.Config.Events;
using Scaffold.Events.Contracts;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Abilities;

namespace GearEngine.Campaign.Presentation
{
    public partial class ActiveRaceViewModel : ViewModel
    {
        private static float ResultPopupDelaySeconds => 2f;

        public CarViewModel Car { get; private set; }
        public RaceDriftScoreViewModel DriftScore { get; private set; }
        public BoardViewModel Board { get; private set; }

        [ObservableProperty]
        private TrackViewModel track;


        [Inject] private ITrackService trackService;
        [Inject] private IGearEngineService engineService;
        [Inject] private IBoardService boardService;
        [Inject] private IInventoryService inventoryService;
        [Inject] private TrackSimulationFactory trackFactory;
        [Inject] private RaceManagerService raceManager;
        [Inject] private ISimulationRunnerService aiRunner;
        [Inject] private CampaignRaceSessionDefaults raceSessionDefaults;
        [Inject] private IAnalyticsService analyticsService;
        [Inject] private IEventBus eventBus;

        public void Tick(float deltaTime)
        {
            DriftScore?.Tick(deltaTime);
        }

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
                RaceResultModel result = CreateRaceResult();
                await Task.Delay(TimeSpan.FromSeconds(ResultPopupDelaySeconds));
                await OpenAndPersistResultAsync(result);
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

        private RaceResultModel CreateRaceResult()
        {
            RaceState session = Track.Session;
            RaceResultModel result = new RaceResultModel(session.RaceTime, session.CurrentLap, trackService.CurrentTrack, session.TotalDriftScore, trackService.GetTrackProgress()?.GetBestTimeSeconds(trackService.CurrentTrack.name));
            analyticsService?.Record(new RaceFinishedEvent(trackService.CurrentTrack.name, trackService.CurrentCar.name, result.RaceTime, result.LapCount, result.Score, result.IsGoodResult));
            return result;
        }

        private async Task OpenAndPersistResultAsync(RaceResultModel result)
        {
            result.BeginPersistence();
            navigation.Open(new ResultPopupViewModel(result));
            try
            {
                await PersistRaceResultAsync(result);
            }
            finally
            {
                result.CompletePersistence();
            }
        }

        private async Task PersistRaceResultAsync(RaceResultModel result)
        {
            try
            {
                await trackService.RecordResultAsync(result);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ActiveRaceViewModel] Race result persistence failed after opening the result screen: {ex.Message}\n{ex.StackTrace}");
            }
        }

        protected override void Initialize()
        {
            base.Initialize();

            eventBus.AddListener<GearEngine.Events.CombatTextCollectedEvent>(OnCombatTextCollected);

            RaceSessionConfig sessionConfig = raceSessionDefaults.CreateForTrack(trackService.CurrentTrack);
            RaceState freshSession = trackFactory.Create(trackService.CurrentCar, trackService.CurrentTrack, sessionConfig);
            raceManager.RegisterRace(freshSession);

            InitializeGearAbilities(freshSession);
            engineService.ResetGridSimulationState();

            BindRaceViews(freshSession);

            Bind<SimulationLifecycleState, SimulationLifecycleState>(() => Track.State, OnTrackStateChanged);
            analyticsService?.Record(new RaceStartedEvent(trackService.CurrentTrack.name, trackService.CurrentCar.name));
        }

        private void InitializeGearAbilities(RaceState session)
        {
            if (engineService == null)
            {
                return;
            }

            foreach (IGridNode node in engineService.GetAllNodes())
            {
                if (node != null)
                {
                    InitializeNodeAbilities(node, session);
                }
            }
        }

        private void InitializeNodeAbilities(IGridNode node, RaceState session)
        {
            foreach (GearAbilitySO ability in node.GetAbilities())
            {
                if (ability is ActiveRaceGearAbilitySO activeGear)
                {
                    activeGear.Initialize(session, engineService);
                }
            }
        }

        private void BindRaceViews(RaceState freshSession)
        {
            Track = new TrackViewModel(freshSession, raceManager, aiRunner, trackFactory);
            BindChildViewModel(Track);

            Car = new CarViewModel(freshSession, aiRunner, attachRunnerOnBind: false);
            BindChildViewModel(Car);

            DriftScore = new RaceDriftScoreViewModel(freshSession, Car);
            BindChildViewModel(DriftScore);

            Board = new BoardViewModel(boardService, engineService, inventoryService, eventBus);
            Board.Interactable = false;
            BindChildViewModel(Board);

        }

    }
}
