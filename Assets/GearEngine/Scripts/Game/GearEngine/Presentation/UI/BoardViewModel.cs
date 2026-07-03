using System;
using System.Collections.Generic;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Nodes;
using CommunityToolkit.Mvvm.ComponentModel;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Services;
using GearEngine.GearEngine.Services.Board;
using GearEngine.GearEngine.Services.Inventory;
using Scaffold.MVVM;
using UnityEngine;
using Scaffold.Events.Contracts;
using GearEngine.GearEngine.Events;

namespace GearEngine.GearEngine.Presentation.UI
{
    public partial class BoardViewModel : ViewModel
    {
        public BoardViewModel(IBoardService boardService, IGearEngineService engineService, IInventoryService inventoryService, IEventBus eventBus)
        {
            this.boardService = boardService ?? throw new ArgumentNullException(nameof(boardService));
            this.engineService = engineService ?? throw new ArgumentNullException(nameof(engineService));
            this.inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            boardService.GearPlaced += OnBoardGearPlaced;
            boardService.GearRemoved += OnBoardGearRemoved;
            eventBus.AddListener<GearRotatedEvent>(OnGearRotated);

            RefreshSimulationRunningFromGrid();
            UpdateLabels();
        }

        public IGearEngineService EngineService => engineService;

        public BoardRulesSO BoardRules => boardService.BoardRules;

        public int GridWidth => boardService.BoardRules != null ? boardService.BoardRules.GridWidth : 0;

        public int GridHeight => boardService.BoardRules != null ? boardService.BoardRules.GridHeight : 0;

        public BoardModel Board => boardService.GetBoard();

        public string MotorCogGearId => inventoryService.MotorCogGearId;

        public int CurrentBoardGearCount => boardService.CurrentBoardGearCount;
        public int MaxAllowedBoardGears => boardService.MaxAllowedBoardGears;

        private readonly IBoardService boardService;
        private readonly IGearEngineService engineService;
        private readonly IInventoryService inventoryService;
        private readonly IEventBus eventBus;

        [ObservableProperty] private bool interactable = true;
        [ObservableProperty] private string boardLimitText = string.Empty;
        [ObservableProperty] private bool isSimulationRunning;

        public event Action<IGridNode> OnGearPlaced;
        public event Action<IGridNode> OnGearRemoved;
        public event Action<IGridNode> OnBoardClicked;

        public void PublishCombatTextExploded(int score)
        {
            eventBus.Raise(new GearEngine.Events.CombatTextCollectedEvent(score));
        }
        public event Action<Vector2Int, string, float> OnGearTriggered;
        public event Action<IGridNode> OnGearChargeCompleted;

        internal void HandleBoardClick(IGridNode node)
        {
            Debug.Log($"[BoardViewModel] HandleBoardClick called. Node: {node?.ConfigData?.Id}");
            if (node != null)
            {
                OnBoardClicked?.Invoke(node);
            }
        }

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

        public void CompleteBoardGearReturnToInventory(IGridNode node, GearItemData config)
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

        public bool HandleInventoryDrop(Vector2Int targetDropPos, GearItemData gearData) =>
            boardService.TryPlace(targetDropPos, gearData);

        protected override void OnClosed()
        {
            boardService.GearPlaced -= OnBoardGearPlaced;
            boardService.GearRemoved -= OnBoardGearRemoved;
            eventBus.RemoveListener<GearRotatedEvent>(OnGearRotated);
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

        private void OnGearRotated(GearRotatedEvent evt)
        {
            IGridNode node = boardService.GetNode(evt.Source);
            if (node?.ConfigData == null) return;

            OnGearChargeCompleted?.Invoke(node);

            var sb = new System.Text.StringBuilder();
            float maxDuration = 0f;
            foreach (var ability in node.ConfigData.Abilities)
            {
                if (ability is IDescribable describable)
                {
                    sb.AppendLine(describable.GetFloatingTextDescription());
                }
                
                if (ability is GearEngine.Abilities.GearAbilitySO gearAbility)
                {
                    float d = gearAbility.GetDuration();
                    if (d > maxDuration) maxDuration = d;
                }
            }
            
            string text = sb.ToString().TrimEnd();
            if (!string.IsNullOrEmpty(text))
            {
                OnGearTriggered?.Invoke(evt.Source, text, maxDuration);
            }
        }
    }
}
