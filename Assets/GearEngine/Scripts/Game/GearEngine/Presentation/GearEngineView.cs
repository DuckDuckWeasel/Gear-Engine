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
                toggle != null ? toggle.TrashAlignment : TrashZoneAlignment.Right,
                toggle?.TrashZoneTag,
                toggle?.TrashIcon);

            // Board drag lifecycle → trash feature
            boardView.OnDragStarted += trashFeature.OnDragStarted;
            boardView.OnDragEnded += trashFeature.OnDragEnded;
            boardView.OnTrashDropRequested += trashFeature.OnTrashDropRequested;

            // Inventory drag lifecycle → trash feature
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
            if (boardView != null)
            {
                boardView.OnGearDroppedOverUI -= HandleGearDroppedOverUI;

                if (trashFeature != null)
                {
                    boardView.OnDragStarted -= trashFeature.OnDragStarted;
                    boardView.OnDragEnded -= trashFeature.OnDragEnded;
                    boardView.OnTrashDropRequested -= trashFeature.OnTrashDropRequested;
                }

                boardView.Unbind();
            }

            if (inventoryView != null && trashFeature != null)
            {
                inventoryView.OnInventoryDragStarted -= trashFeature.OnDragStarted;
                inventoryView.OnInventoryDragEnded -= trashFeature.OnDragEnded;
            }

            if (viewModel != null)
            {
                viewModel.Inventory.OnGearDraggedToBoard -= HandleGearDraggedToBoard;
            }

            trashFeature?.Dispose();
            trashFeature = null;
        }
    }
}
