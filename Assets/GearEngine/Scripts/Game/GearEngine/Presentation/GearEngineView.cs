using System;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Presentation.UI;
using Scaffold.MVVM;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation
{
    public class GearEngineView : View<GearEngineViewModel>
    {
        [SerializeField] private SimulationControlView simControlView;
        [SerializeField] private GearInventoryView inventoryView;
        [SerializeField] private BoardView boardView;

        private GearTrashFeature trashFeature;

        protected override void OnBind()
        {
            simControlView.Bind(viewModel.SimControl);
            inventoryView.Bind(viewModel.Inventory);
            boardView.Bind(viewModel.Board, interactable: true);

            boardView.OnGearDroppedOverUI += HandleGearDroppedOverUI;
            viewModel.Inventory.OnGearDraggedToBoard += HandleGearDraggedToBoard;

            InitializeTrashFeature();
        }

        private void InitializeTrashFeature()
        {
            GearEngineFeatureToggleSO toggle = viewModel.FeatureToggle;

            if (toggle != null && !toggle.EnableTrashDeletion)
            {
                return;
            }

            Canvas overlayCanvas = ResolveOverlayCanvas();
            if (overlayCanvas == null)
            {
                Debug.LogWarning("[GearEngineView] No Canvas found — trash feature disabled.");
                return;
            }

            trashFeature = BuildTrashFeature(overlayCanvas, toggle);
            WireTrashFeatureToBoard();
            WireTrashFeatureToInventory();
        }

        private Canvas ResolveOverlayCanvas()
        {
            Canvas overlayCanvas = GetComponentInParent<Canvas>();
            if (overlayCanvas == null)
            {
                overlayCanvas = FindObjectOfType<Canvas>();
            }

            return overlayCanvas;
        }

        private GearTrashFeature BuildTrashFeature(Canvas overlayCanvas, GearEngineFeatureToggleSO toggle)
        {
            TrashZoneAlignment alignment = toggle != null ? toggle.TrashAlignment : TrashZoneAlignment.Right;
            return new GearTrashFeature(viewModel.Board, viewModel.Inventory, overlayCanvas, viewModel.Board.BoardConfig, alignment, toggle?.TrashZoneTag, toggle?.TrashIcon);
        }

        private void WireTrashFeatureToBoard()
        {
            boardView.OnDragStarted += trashFeature.OnDragStarted;
            boardView.OnDragEnded += trashFeature.OnDragEnded;
            boardView.OnTrashDropRequested += trashFeature.OnTrashDropRequested;
        }

        private void WireTrashFeatureToInventory()
        {
            inventoryView.OnInventoryDragStarted += trashFeature.OnDragStarted;
            inventoryView.OnInventoryDragEnded += trashFeature.OnDragEnded;
        }

        private void HandleGearDroppedOverUI(GearConfigData config, Vector3 _)
        {
            if (config != null)
            {
                viewModel.Inventory.AddGearToInventory(config);
            }
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
            }
        }

        private void OnDestroy()
        {
            UnsubscribeBoardTrash();
            UnsubscribeInventoryTrash();
            UnsubscribeViewModel();
            DisposeTrashFeature();
        }

        private void UnsubscribeBoardTrash()
        {
            if (boardView == null)
            {
                return;
            }

            boardView.OnGearDroppedOverUI -= HandleGearDroppedOverUI;
            if (trashFeature != null)
            {
                boardView.OnDragStarted -= trashFeature.OnDragStarted;
                boardView.OnDragEnded -= trashFeature.OnDragEnded;
                boardView.OnTrashDropRequested -= trashFeature.OnTrashDropRequested;
            }

            boardView.Unbind();
        }

        private void UnsubscribeInventoryTrash()
        {
            if (inventoryView == null || trashFeature == null)
            {
                return;
            }

            inventoryView.OnInventoryDragStarted -= trashFeature.OnDragStarted;
            inventoryView.OnInventoryDragEnded -= trashFeature.OnDragEnded;
        }

        private void UnsubscribeViewModel()
        {
            if (viewModel != null)
            {
                viewModel.Inventory.OnGearDraggedToBoard -= HandleGearDraggedToBoard;
            }
        }

        private void DisposeTrashFeature()
        {
            trashFeature?.Dispose();
            trashFeature = null;
        }
    }
}
