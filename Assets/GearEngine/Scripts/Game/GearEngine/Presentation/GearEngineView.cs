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
            BindPrimaryViews();
            ConfigureBoardReference();
            SubscribeViewEvents();
            InitializeTrashFeature();
            UpdateLimitLabels();
        }

        private void InitializeTrashFeature()
        {
            if (!IsTrashFeatureEnabled())
            {
                return;
            }

            Canvas overlayCanvas = FindOverlayCanvas();
            if (overlayCanvas == null)
            {
                return;
            }

            trashFeature = CreateTrashFeature(overlayCanvas);
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
            UnsubscribeBoardViewEvents();
            UnsubscribeViewModelEvents();
            DisposeTrashFeature();
        }

        private void BindPrimaryViews()
        {
            simControlView.Bind(viewModel.SimControl);
            inventoryView.SetDragService(viewModel.DragService);
            boardView.Bind(viewModel.Board, interactable: true);
            inventoryView.Bind(viewModel.Inventory);
        }

        private void ConfigureBoardReference()
        {
            var frustumFit = FindFirstObjectByType<GearEngine.Presentation.World.FrustumFit>();
            if (frustumFit != null)
            {
                frustumFit.Apply();
                inventoryView.SetBoardReference(frustumFit.transform);
                return;
            }

            inventoryView.SetBoardReference(boardView.transform);
        }

        private void SubscribeViewEvents()
        {
            boardView.OnGearDroppedOverUI += HandleGearDroppedOverUI;
            viewModel.Inventory.OnGearDraggedToBoard += HandleGearDraggedToBoard;
            viewModel.Board.OnGearPlaced += HandleBoardGearChanged;
            viewModel.Board.OnGearRemoved += HandleBoardGearChanged;
            SubscribeInventoryCollection();
        }

        private void SubscribeInventoryCollection()
        {
            if (viewModel.Inventory.InventoryModel.AvailableGears != null)
            {
                viewModel.Inventory.InventoryModel.AvailableGears.CollectionChanged += HandleInventoryChanged;
            }
        }

        private bool IsTrashFeatureEnabled()
        {
            return viewModel.FeatureToggle == null || viewModel.FeatureToggle.EnableTrashDeletion;
        }

        private Canvas FindOverlayCanvas()
        {
            Canvas overlayCanvas = GetComponentInParent<Canvas>();
            if (overlayCanvas != null)
            {
                return overlayCanvas;
            }

            overlayCanvas = FindFirstObjectByType<Canvas>();
            if (overlayCanvas == null)
            {
                Debug.LogWarning("[GearEngineView] No Canvas found - trash feature disabled.");
            }

            return overlayCanvas;
        }

        private GearTrashFeature CreateTrashFeature(Canvas overlayCanvas)
        {
            GearEngineFeatureToggleSO toggle = viewModel.FeatureToggle;
            BoardConfigSO config = viewModel.Board.BoardConfig;
            TrashZoneAlignment alignment = toggle != null ? toggle.TrashAlignment : TrashZoneAlignment.Right;
            float trashOffset = config != null ? config.TrashZoneYOffset : 80f;
            return new GearTrashFeature(viewModel.Board, viewModel.Inventory, overlayCanvas, config, viewModel.DragService, alignment, trashOffset, toggle?.TrashZoneTag, toggle?.TrashIcon);
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

        private void UnsubscribeBoardViewEvents()
        {
            if (boardView == null)
            {
                return;
            }

            boardView.OnGearDroppedOverUI -= HandleGearDroppedOverUI;
            if (trashFeature != null)
            {
                boardView.OnTrashDropRequested -= trashFeature.OnTrashDropRequested;
            }

            boardView.Unbind();
        }

        private void UnsubscribeViewModelEvents()
        {
            if (viewModel == null)
            {
                return;
            }

            viewModel.Inventory.OnGearDraggedToBoard -= HandleGearDraggedToBoard;
            viewModel.Board.OnGearPlaced -= HandleBoardGearChanged;
            viewModel.Board.OnGearRemoved -= HandleBoardGearChanged;
            UnsubscribeInventoryCollection();
        }

        private void UnsubscribeInventoryCollection()
        {
            if (viewModel.Inventory.InventoryModel?.AvailableGears != null)
            {
                viewModel.Inventory.InventoryModel.AvailableGears.CollectionChanged -= HandleInventoryChanged;
            }
        }

        private void DisposeTrashFeature()
        {
            trashFeature?.Dispose();
            trashFeature = null;
        }
    }
}
