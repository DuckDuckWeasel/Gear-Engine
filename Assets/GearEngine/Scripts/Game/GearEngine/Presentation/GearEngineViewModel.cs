using System;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using GearEngine.CarSimulation;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Bootstrap;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Services;
using Scaffold.Events.Contracts;
using Scaffold.MVVM;
using UnityEngine;
using VContainer;

namespace GearEngine.GearEngine.Presentation
{
    public partial class GearEngineViewModel : ViewModel
    {
        public GearEngineViewModel(GearEngineStartData startData, TrackSimulation simulation)
        {
            this.startData = startData ?? throw new ArgumentNullException(nameof(startData));
        }
        public GearEngineFeatureToggleSO FeatureToggle => featureToggle;
        public IGearTrashService TrashService => trashService;
        public IDragService DragService => dragService;
        public SimulationControlViewModel SimControl { get; } = new SimulationControlViewModel();
        public GearInventoryViewModel Inventory { get; } = new GearInventoryViewModel();
        public BoardViewModel Board { get; } = new BoardViewModel();
        public TrashZoneViewModel TrashZone { get; } = new TrashZoneViewModel();

        [ObservableProperty] private string boardLimitText = string.Empty;
        [ObservableProperty] private string inventoryLimitText = string.Empty;

        private readonly GearEngineStartData startData;

        [Inject] private IGearEngineService engineService;
        [Inject] private IGridManager gridManager;
        [Inject] private GearNodeFactory nodeFactory;
        [Inject] private BoardConfigSO boardConfig;
        [Inject] private IEventBus eventBus;
        [Inject] private GearEngineFeatureToggleSO featureToggle;
        [Inject] private IDragService dragService;
        [Inject] private IGearTransferService transferService;
        [Inject] private IGearTrashService trashService;

        protected override void Initialize()
        {
            base.Initialize();

            BindChildViewModel(SimControl);
            BindChildViewModel(Inventory);
            BindChildViewModel(Board);
            BindChildViewModel(TrashZone);

            SimControl.Initialize(engineService);
            Inventory.Initialize(engineService, startData.MaxInventorySlots, dragService);

            if (startData.InventoryGears != null)
            {
                Inventory.LoadInventory(startData.InventoryGears);
            }

            Board.Initialize(engineService, gridManager, nodeFactory, boardConfig, eventBus, featureToggle, dragService);

            if (transferService != null)
            {
                transferService.LinkBoard(Board);
                transferService.LinkInventory(Inventory);
            }

            if (trashService != null)
            {
                trashService.LinkedBoard = Board;
                trashService.LinkedInventory = Inventory;
            }

            if (startData.BoardLayout != null)
            {
                Board.LoadLayout(startData.BoardLayout);
            }

            Board.OnGearPlaced += _ => UpdateLimitLabels();
            Board.OnGearRemoved += _ => UpdateLimitLabels();
            if (Inventory.InventoryModel?.AvailableGears != null)
            {
                Inventory.InventoryModel.AvailableGears.CollectionChanged += (_, _) => UpdateLimitLabels();
            }

            if (dragService != null)
            {
                dragService.OnDragStarted += TrashZone.HandleDragStarted;
                dragService.OnDragEnded += TrashZone.HandleDragEnded;
            }

            if (trashService != null)
            {
                TrashZone.OnGearDropped += trashService.HandleInventoryGearDropped;
            }

            UpdateLimitLabels();
        }

        private void UpdateLimitLabels()
        {
            BoardLimitText = $"Board: {Board.CurrentBoardGearCount}/{Board.MaxAllowedBoardGears}";
            InventoryLimitText = $"Inventory: {Inventory.CurrentCount}/{Inventory.MaxSlots}";
        }

        /// <summary>
        /// Places an inventory gear onto the board at the given local position.
        /// Handles both placement and inventory consumption in one call.
        /// </summary>
        /// <param name="boardLocalPos">Position relative to the board transform origin.</param>
        /// <param name="gearData">The gear config data to place.</param>
        /// <returns>True if placement succeeded.</returns>
        public bool TryPlaceFromInventory(Vector3 boardLocalPos, GearConfigData gearData)
        {
            try
            {
                if (engineService?.IsRunning ?? false)
                {
                    return false;
                }

                bool placed = Board.HandleInventoryDrop(boardLocalPos, gearData);
                if (placed)
                {
                    Inventory.ConsumeSpecificGear(gearData);
                }

                return placed;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GearEngineViewModel] TryPlaceFromInventory failed: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Returns a gear back to the inventory (e.g. when dropped over the UI instead of the board).
        /// </summary>
        public void ReturnGearToInventory(GearConfigData config)
        {
            if (config != null)
            {
                Inventory.AddGearToInventory(config);
            }
        }
    }
}