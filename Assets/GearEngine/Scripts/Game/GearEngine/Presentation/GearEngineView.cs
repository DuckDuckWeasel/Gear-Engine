using System;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Presentation.UI;
using Scaffold.MVVM;
using TMPro;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation
{
    public class GearEngineView : View<GearEngineViewModel>
    {
        [SerializeField] private SimulationControlView simControlView;
        [SerializeField] private GearInventoryView inventoryView;
        [SerializeField] private BoardView boardView;
        [SerializeField] private TextMeshProUGUI boardLimitLabel;
        [SerializeField] private TextMeshProUGUI inventoryLimitLabel;

        private GearTrashFeature trashFeature;

        protected override void OnBind()
        {
            simControlView.Bind(viewModel.SimControl);
            inventoryView.SetDragService(viewModel.DragService);
            
            boardView.Bind(viewModel.Board, interactable: true);

            var frustumFit = GameObject.FindObjectOfType<GearEngine.Presentation.World.FrustumFit>();
            if (frustumFit != null)
            {
                frustumFit.Apply();
                inventoryView.SetBoardReference(frustumFit.transform);
            }
            else
            {
                inventoryView.SetBoardReference(boardView.transform);
            }

            inventoryView.Bind(viewModel.Inventory);

            boardView.OnGearDroppedOverUI += HandleGearDroppedOverUI;
            viewModel.Inventory.OnGearDraggedToBoard += HandleGearDraggedToBoard;

            viewModel.Board.OnGearPlaced += HandleBoardGearChanged;
            viewModel.Board.OnGearRemoved += HandleBoardGearChanged;
            if (viewModel.Inventory.InventoryModel.AvailableGears != null)
            {
                viewModel.Inventory.InventoryModel.AvailableGears.CollectionChanged += HandleInventoryChanged;
            }

            InitializeTrashFeature();
            UpdateLimitLabels();
        }

        private void InitializeTrashFeature()
        {
            GearEngineFeatureToggleSO toggle = viewModel.FeatureToggle;

            if (toggle != null && !toggle.EnableTrashDeletion)
            {
                return;
            }

            Canvas overlayCanvas = GetComponentInParent<Canvas>();
            if (overlayCanvas == null)
            {
                overlayCanvas = FindObjectOfType<Canvas>();
            }

            if (overlayCanvas == null)
            {
                Debug.LogWarning("[GearEngineView] No Canvas found — trash feature disabled.");
                return;
            }

            trashFeature = new GearTrashFeature(
                viewModel.Board,
                viewModel.Inventory,
                overlayCanvas,
                viewModel.Board.BoardConfig,
                viewModel.DragService,
                toggle != null ? toggle.TrashAlignment : TrashZoneAlignment.Right,
                viewModel.Board.BoardConfig != null ? viewModel.Board.BoardConfig.TrashZoneYOffset : 80f,
                toggle?.TrashZoneTag,
                toggle?.TrashIcon);

            // Board trash drop request — still event-based since it's a view-to-feature command
            boardView.OnTrashDropRequested += trashFeature.OnTrashDropRequested;
        }

        private void HandleGearDroppedOverUI(GearConfigData config, Vector3 _)
        {
            if (config != null)
            {
                viewModel.Inventory.AddGearToInventory(config);
            }

            UpdateLimitLabels();
        }

        private void HandleGearDraggedToBoard(Vector3 worldPos, GearConfigData gearData)
        {
            try
            {
                TryPlaceInventoryFromDrag(worldPos, gearData);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GearEngineView] HandleGearDraggedToBoard failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void TryPlaceInventoryFromDrag(Vector3 worldPos, GearConfigData gearData)
        {
            if (viewModel.Board.EngineService.IsRunning)
            {
                return;
            }

            bool placed = viewModel.Board.HandleInventoryDrop(worldPos, gearData);
            if (placed)
            {
                viewModel.Inventory.ConsumeSpecificGear(gearData);
                UpdateLimitLabels();
            }
        }

        private void UpdateLimitLabels()
        {
            if (boardLimitLabel != null)
            {
                boardLimitLabel.text = $"Board: {viewModel.Board.CurrentBoardGearCount}/{viewModel.Board.MaxAllowedBoardGears}";
            }

            if (inventoryLimitLabel != null)
            {
                inventoryLimitLabel.text = $"Inventory: {viewModel.Inventory.CurrentCount}/{viewModel.Inventory.MaxSlots}";
            }
        }

        private void HandleBoardGearChanged(Nodes.IGridNode _)
        {
            UpdateLimitLabels();
        }

        private void HandleInventoryChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateLimitLabels();
        }

        private void OnDestroy()
        {
            if (boardView != null)
            {
                boardView.OnGearDroppedOverUI -= HandleGearDroppedOverUI;

                if (trashFeature != null)
                {
                    boardView.OnTrashDropRequested -= trashFeature.OnTrashDropRequested;
                }

                boardView.Unbind();
            }

            if (viewModel != null)
            {
                viewModel.Inventory.OnGearDraggedToBoard -= HandleGearDraggedToBoard;
                viewModel.Board.OnGearPlaced -= HandleBoardGearChanged;
                viewModel.Board.OnGearRemoved -= HandleBoardGearChanged;
                if (viewModel.Inventory.InventoryModel?.AvailableGears != null)
                {
                    viewModel.Inventory.InventoryModel.AvailableGears.CollectionChanged -= HandleInventoryChanged;
                }
            }

            trashFeature?.Dispose();
            trashFeature = null;
        }
    }
}
