using System;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Presentation.World;
using TMPro;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI
{
    public sealed class GearInteractionBinder : IDisposable
    {
        private readonly BoardView boardView;
        private readonly GearInventoryView inventoryView;
        private readonly BoardViewModel boardVm;
        private readonly GearInventoryViewModel inventoryVm;
        private readonly TextMeshProUGUI boardLimitLabel;
        private readonly TextMeshProUGUI inventoryLimitLabel;
        private readonly Func<bool> isRunningCheck;

        private bool isBound;

        public GearInteractionBinder(
            BoardView boardView,
            GearInventoryView inventoryView,
            BoardViewModel boardVm,
            GearInventoryViewModel inventoryVm,
            TextMeshProUGUI boardLimitLabel = null,
            TextMeshProUGUI inventoryLimitLabel = null,
            Func<bool> isRunningCheck = null)
        {
            this.boardView = boardView ?? throw new ArgumentNullException(nameof(boardView));
            this.inventoryView = inventoryView ?? throw new ArgumentNullException(nameof(inventoryView));
            this.boardVm = boardVm ?? throw new ArgumentNullException(nameof(boardVm));
            this.inventoryVm = inventoryVm ?? throw new ArgumentNullException(nameof(inventoryVm));
            this.boardLimitLabel = boardLimitLabel;
            this.inventoryLimitLabel = inventoryLimitLabel;
            this.isRunningCheck = isRunningCheck ?? (() => boardVm.EngineService?.IsRunning ?? false);
        }

        public void Bind()
        {
            if (isBound)
            {
                return;
            }

            isBound = true;

            ApplyFrustum();

            inventoryView.Bind(inventoryVm);

            boardView.OnGearDroppedOverUI += HandleGearDroppedOverUI;
            inventoryVm.OnGearDraggedToBoard += HandleGearDraggedToBoard;

            boardVm.OnGearPlaced += HandleBoardGearChanged;
            boardVm.OnGearRemoved += HandleBoardGearChanged;

            if (inventoryVm.InventoryModel.AvailableGears != null)
            {
                inventoryVm.InventoryModel.AvailableGears.CollectionChanged += HandleInventoryChanged;
            }

            UpdateLimitLabels();
        }

        private static void ApplyFrustum()
        {
            var frustumFit = GameObject.FindObjectOfType<FrustumFit>();
            if (frustumFit != null)
            {
                frustumFit.Apply();
            }
        }

        public void Unbind()
        {
            if (!isBound)
            {
                return;
            }

            isBound = false;

            if (boardView != null)
            {
                boardView.OnGearDroppedOverUI -= HandleGearDroppedOverUI;
            }

            inventoryVm.OnGearDraggedToBoard -= HandleGearDraggedToBoard;
            boardVm.OnGearPlaced -= HandleBoardGearChanged;
            boardVm.OnGearRemoved -= HandleBoardGearChanged;

            if (inventoryVm.InventoryModel?.AvailableGears != null)
            {
                inventoryVm.InventoryModel.AvailableGears.CollectionChanged -= HandleInventoryChanged;
            }
        }

        public void UpdateLimitLabels()
        {
            if (boardLimitLabel != null)
            {
                boardLimitLabel.text = $"Board: {boardVm.CurrentBoardGearCount}/{boardVm.MaxAllowedBoardGears}";
            }

            if (inventoryLimitLabel != null)
            {
                inventoryLimitLabel.text = $"Inventory: {inventoryVm.CurrentCount}/{inventoryVm.MaxSlots}";
            }
        }

        public void Dispose()
        {
            Unbind();
        }

        private void HandleGearDroppedOverUI(GearConfigData config, Vector3 _)
        {
            if (config != null)
            {
                inventoryVm.AddGearToInventory(config);
            }

            UpdateLimitLabels();
        }

        private void HandleGearDraggedToBoard(Vector3 worldPos, GearConfigData gearData)
        {
            try
            {
                if (isRunningCheck())
                {
                    return;
                }

                // Subtract the board's world position to get coordinates relative to the board origin.
                // We use simple subtraction instead of InverseTransformPoint because
                // FrustumFit scales the board transform, but GetGridPosition uses Spacing
                // (not transform scale) — InverseTransformPoint would divide by scale, 
                // multiplying the offset incorrectly.
                Vector3 localPos = worldPos - boardView.transform.position;

                bool placed = boardVm.HandleInventoryDrop(localPos, gearData);
                if (placed)
                {
                    inventoryVm.ConsumeSpecificGear(gearData);
                    UpdateLimitLabels();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GearInteractionBinder] HandleGearDraggedToBoard failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void HandleBoardGearChanged(IGridNode _)
        {
            UpdateLimitLabels();
        }

        private void HandleInventoryChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateLimitLabels();
        }
    }
}
