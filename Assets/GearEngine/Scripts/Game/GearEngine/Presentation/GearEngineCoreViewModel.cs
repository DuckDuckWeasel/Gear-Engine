using System;
using CommunityToolkit.Mvvm.ComponentModel;
using GearEngine.CarSimulation;
using GearEngine.GearEngine.Services;
using Scaffold.Events.Contracts;
using Scaffold.MVVM;
using UnityEngine;
using VContainer;
using GearEngine.GearEngine.Services.Inventory;

namespace GearEngine.GearEngine.Presentation
{
    public partial class GearEngineCoreViewModel : ViewModel
    {
        public GearEngineCoreViewModel(GearEngineStartData startData, LapRaceSession simulation)
        {
            this.startData = startData ?? throw new ArgumentNullException(nameof(startData));
        }

        public GearEngineFeatureToggleSO FeatureToggle => featureToggle;
        public IGearTrashService TrashService => trashService;
        public IDragService DragService => dragService;

        [Inject] public GearInventoryViewModel Inventory { get; private set; }
        [Inject] public BoardViewModel Board { get; private set; }
        [Inject] public TrashZoneViewModel TrashZone { get; private set; }

        private readonly GearEngineStartData startData;

        [Inject] private IGearEngineService engineService;
        [Inject] private IGridManager gridManager;
        [Inject] private IGearNodeFactory nodeFactory;
        [Inject] private BoardConfigSO boardConfig;
        [Inject] private IEventBus eventBus;
        [Inject] private GearEngineFeatureToggleSO featureToggle;
        [Inject] private IDragService dragService;
        [Inject] private IGearTransferService transferService;
        [Inject] private IGearTrashService trashService;
        [Inject] private IGridSwapService swapService;
        [Inject] private IGridMergeService mergeService;
        [Inject] private Services.Inventory.IInventoryService inventoryService;

        protected override void Initialize()
        {
            base.Initialize();

            if (Inventory == null) throw new ArgumentNullException(nameof(Inventory));
            if (Board == null) throw new ArgumentNullException(nameof(Board));
            if (TrashZone == null) throw new ArgumentNullException(nameof(TrashZone));
            if (engineService == null) throw new ArgumentNullException(nameof(engineService));
            if (gridManager == null) throw new ArgumentNullException(nameof(gridManager));
            if (nodeFactory == null) throw new ArgumentNullException(nameof(nodeFactory));
            if (boardConfig == null) throw new ArgumentNullException(nameof(boardConfig));
            if (swapService == null) throw new ArgumentNullException(nameof(swapService));
            if (mergeService == null) throw new ArgumentNullException(nameof(mergeService));

            BindChildViewModel(Inventory);
            BindChildViewModel(Board);
            BindChildViewModel(TrashZone);

            inventoryService.Initialize(startData.MaxInventorySlots);

            if (startData.InventoryGears != null)
            {
                var runtimeGears = new System.Collections.Generic.List<IItem>();
                foreach (var config in startData.InventoryGears)
                {
                    if (config != null) runtimeGears.Add(config.CreateRuntimeData());
                }
                inventoryService.LoadInventory(runtimeGears);
            }

            Inventory.Initialize(engineService, inventoryService, dragService);

            Board.Initialize(engineService, gridManager, nodeFactory, boardConfig, eventBus, featureToggle, dragService, swapService, mergeService);

            if (transferService != null)
            {
                transferService.LinkBoard(Board);
                transferService.LinkInventory(inventoryService);
            }

            if (trashService != null)
            {
                trashService.LinkedBoard = Board;
                trashService.LinkedInventory = inventoryService;
            }

            if (startData.BoardLayout != null)
            {
                Board.LoadLayout(startData.BoardLayout);
            }

            if (dragService != null)
            {
                dragService.OnDragStarted += TrashZone.HandleDragStarted;
                dragService.OnDragEnded += TrashZone.HandleDragEnded;
            }

            if (trashService != null)
            {
                TrashZone.OnGearDropped += trashService.HandleInventoryGearDropped;
                Board.OnTrashDropRequested += trashService.RequestTrashDrop;
            }

            Board.OnGearReturnRequested += ReturnGearToInventory;
            Inventory.OnGearDraggedToBoard += HandleGearDraggedToBoard;
        }

        private void HandleGearDraggedToBoard(Vector3 worldPos, GearConfigData gearData)
        {
            Vector2Int gridPos = Board.BoardConfig.GetGridPosition(worldPos);
            TryPlaceFromInventory(gridPos, gearData);
        }
        public bool TryPlaceFromInventory(Vector2Int gridPos, GearConfigData gearData)
        {
            try
            {
                if (engineService.IsRunning)
                {
                    return false;
                }

                bool placed = Board.HandleInventoryDrop(gridPos, gearData);
                if (placed)
                {
                    inventoryService.ConsumeSpecificItem(gearData);
                }

                return placed;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GearEngineCoreViewModel] TryPlaceFromInventory failed: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        public void ReturnGearToInventory(GearConfigData config)
        {
            if (config != null)
            {
                inventoryService.AddItem(config);
            }
        }
    }
}
