using System;
using System.Threading;
using System.Threading.Tasks;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Presentation;
using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Bootstrap;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Manager;
using GearEngine.GearEngine.Presentation.UI;
using GearEngine.Race.Rewards;
using Scaffold.MVVM;
using UnityEngine;
using VContainer;

namespace GearEngine.Race
{
    public sealed class RaceViewModel : ViewModel
    {
        public RaceViewModel(RaceStartData startData)
        {
            this.startData = startData ?? throw new ArgumentNullException(nameof(startData));
        }

        public GearInventoryViewModel Inventory { get; } = new GearInventoryViewModel();

        public BoardViewModel Board { get; } = new BoardViewModel();

        public TrackViewModel Track { get; private set; }

        public PlayerGoldViewModel PlayerGold { get; } = new PlayerGoldViewModel();

        public bool IsRaceRunning => engineService?.IsRunning ?? false;

        private readonly RaceStartData startData;

        private TrackSimulation activeSimulation;

        private CancellationTokenSource rewardFlowCts;

        [Inject]
        private IGearEngineService engineService;

        [Inject]
        private IGridManager gridManager;

        [Inject]
        private GearNodeFactory nodeFactory;

        [Inject]
        private BoardConfigSO boardConfig;

        [Inject]
        private TrackSimulationFactory trackFactory;

        [Inject]
        private ITrackSimulationRunner trackSimulationRunner;

        [Inject]
        private IRaceRewardLiveOpsClient raceRewardLiveOps;

        protected override void Initialize()
        {
            base.Initialize();
            ValidateStartData();
            SetupInventory();
            SetupBoard();
            SetupTrack();
            BindChildViewModel(PlayerGold);
            rewardFlowCts = new CancellationTokenSource();
            _ = SyncGoldBalanceAsync(rewardFlowCts.Token);
        }

        public void ToggleRace()
        {
            if (engineService == null || Track == null)
            {
                return;
            }

            if (Track.State == SimulationLifecycleState.Completed)
            {
                return;
            }

            if (engineService.IsRunning)
            {
                engineService.Stop();
                Track.Toggle(false);
            }
            else
            {
                engineService.Play();
                Track.Toggle(true);
            }
        }

        public void TearDown()
        {
            rewardFlowCts?.Cancel();
            rewardFlowCts?.Dispose();
            rewardFlowCts = null;

            if (activeSimulation != null)
            {
                activeSimulation.Completed -= OnTrackSimulationCompleted;
                activeSimulation = null;
            }
        }

        private void ValidateStartData()
        {
            if (startData.TrackDefinition == null)
            {
                throw new InvalidOperationException("[RaceViewModel] RaceStartData.TrackDefinition is missing.");
            }

            if (startData.CarDefinition == null)
            {
                throw new InvalidOperationException("[RaceViewModel] RaceStartData.CarDefinition is missing.");
            }
        }

        private void SetupInventory()
        {
            BindChildViewModel(Inventory);
            Inventory.Initialize(engineService);

            GearEngineStartData gearData = startData.GearEngineData;
            if (gearData?.InventoryGears != null)
            {
                Inventory.LoadInventory(gearData.InventoryGears);
            }
        }

        private void SetupBoard()
        {
            BindChildViewModel(Board);
            Board.Initialize(engineService, gridManager, nodeFactory, boardConfig);

            GearEngineStartData gearData = startData.GearEngineData;
            if (gearData?.BoardLayout != null)
            {
                Board.LoadLayout(gearData.BoardLayout);
            }
        }

        private void SetupTrack()
        {
            TrackSimulation simulation = trackFactory.Create(startData.CarDefinition, startData.TrackDefinition, startData.SimulationConfig);
            activeSimulation = simulation;
            activeSimulation.Completed += OnTrackSimulationCompleted;
            trackSimulationRunner.SetSimulation(simulation);
            Track = new TrackViewModel(simulation);
            BindChildViewModel(Track);
        }

        private void OnTrackSimulationCompleted()
        {
            if (activeSimulation == null)
            {
                return;
            }

            if (engineService != null && engineService.IsRunning)
            {
                engineService.Stop();
            }

            RaceRewardEvaluation evaluation = RaceRewardEvaluator.Evaluate(
                startData.TrackDefinition,
                activeSimulation.Race.CurrentTime,
                activeSimulation.Race.CurrentLap);

            if (!evaluation.MatchedBracket || evaluation.GoldReward <= 0)
            {
                return;
            }

            CancellationToken token = rewardFlowCts?.Token ?? CancellationToken.None;
            _ = GrantRewardFlowAsync(evaluation, token);
        }

        private async Task GrantRewardFlowAsync(RaceRewardEvaluation evaluation, CancellationToken cancellationToken)
        {
            if (raceRewardLiveOps == null)
            {
                return;
            }

            try
            {
                string trackId = startData.TrackDefinition != null ? startData.TrackDefinition.name : string.Empty;
                var request = new RaceRewardGrantRequest(trackId, evaluation.RankId, evaluation.GoldReward, evaluation.FinishTimeSeconds);
                RaceRewardGrantResult result = await raceRewardLiveOps.GrantRaceRewardAsync(request, cancellationToken).ConfigureAwait(true);
                if (result.Success)
                {
                    PlayerGold.Gold = result.NewGoldBalance;
                }
                else if (!string.IsNullOrEmpty(result.Message))
                {
                    Debug.LogWarning($"[RaceViewModel] Race reward not applied: {result.Message}");
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RaceViewModel] GrantRaceRewardAsync failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private async Task SyncGoldBalanceAsync(CancellationToken cancellationToken)
        {
            if (raceRewardLiveOps is not StubRaceRewardLiveOpsClient stub)
            {
                return;
            }

            try
            {
                await Task.Yield();
                cancellationToken.ThrowIfCancellationRequested();
                PlayerGold.Gold = stub.GoldBalance;
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
