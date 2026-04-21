using System;
using System.Collections.Generic;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Nodes;
using CommunityToolkit.Mvvm.ComponentModel;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Services.Board;
using Scaffold.MVVM;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI
{
    public partial class BoardViewModel : ViewModel
    {
        public BoardViewModel(IBoardService boardService, IGearEngineService engineService)
        {
            this.boardService = boardService ?? throw new ArgumentNullException(nameof(boardService));
            this.engineService = engineService ?? throw new ArgumentNullException(nameof(engineService));

            boardService.GearPlaced += OnBoardGearPlaced;
            boardService.GearRemoved += OnBoardGearRemoved;

            RefreshSimulationRunningFromGrid();
            UpdateLabels();
        }

        public IGearEngineService EngineService => engineService;

        public BoardRulesSO BoardRules => boardService.BoardRules;

        public int GridWidth => boardService.BoardRules != null ? boardService.BoardRules.GridWidth : 0;

        public int GridHeight => boardService.BoardRules != null ? boardService.BoardRules.GridHeight : 0;

        public BoardModel Board => boardService.GetBoard();

        public int CurrentBoardGearCount => boardService.CurrentBoardGearCount;
        public int MaxAllowedBoardGears => boardService.MaxAllowedBoardGears;

        private readonly IBoardService boardService;
        private readonly IGearEngineService engineService;

        [ObservableProperty] private bool interactable = true;
        [ObservableProperty] private string boardLimitText = string.Empty;
        [ObservableProperty] private bool isSimulationRunning;

        public event Action<IGridNode> OnGearPlaced;
        public event Action<IGridNode> OnGearRemoved;

        public void ToggleSimulation()
        {
            try
            {
                boardService.ToggleSimulation();
                RefreshSimulationRunningFromGrid();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BoardViewModel] ToggleSimulation failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void RefreshSimulationRunningFromGrid()
        {
            IsSimulationRunning = boardService.IsSimulationRunning;
        }

        public IGridNode GetNode(Vector2Int coord) => boardService.GetNode(coord);

        public IEnumerable<IGridNode> GetCurrentNodes() => boardService.GetAllNodes();

        public void LoadLayout(BoardLayoutData layout)
        {
            boardService.LoadLayout(layout);
            RefreshSimulationRunningFromGrid();
            UpdateLabels();
        }

        public void CompleteBoardGearReturnToInventory(IGridNode node, GearConfigData config)
        {
            _ = config;
            try
            {
                boardService.TryRemoveBoardGear(node);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BoardViewModel] CompleteBoardGearReturnToInventory failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public bool TryMoveBoardGear(IGridNode node, Vector2Int toPos)
        {
            bool moved = boardService.TryMoveBoardGear(node, toPos, node.Position);
            RefreshSimulationRunningFromGrid();
            UpdateLabels();
            return moved;
        }

        public bool DeleteGear(IGridNode node) => boardService.TryDeleteBoardGear(node);

        public bool HandleInventoryDrop(Vector2Int targetDropPos, GearConfigData gearData) =>
            boardService.TryPlace(targetDropPos, gearData);

        protected override void OnClosed()
        {
            boardService.GearPlaced -= OnBoardGearPlaced;
            boardService.GearRemoved -= OnBoardGearRemoved;
            base.OnClosed();
        }

        private void OnBoardGearPlaced(IGridNode node)
        {
            OnGearPlaced?.Invoke(node);
            UpdateLabels();
        }

        private void OnBoardGearRemoved(IGridNode node)
        {
            OnGearRemoved?.Invoke(node);
            UpdateLabels();
        }

        private void UpdateLabels()
        {
            BoardLimitText = $"Board: {CurrentBoardGearCount}/{MaxAllowedBoardGears}";
        }
    }
}
