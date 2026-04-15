using System;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Presentation.World;
using TMPro;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI
{
    /// <summary>
    /// Encapsulates all shared gear interaction wiring between Board and Inventory.
    /// Used by both GearEngineView and RaceView to avoid logic duplication.
    /// Call <see cref="Bind"/> during OnBind and <see cref="Unbind"/> during OnUnbind/OnDestroy.
    /// </summary>
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

        /// <summary>
        /// Creates a new GearInteractionBinder.
        /// </summary>
        /// <param name="boardView">The board view to bind.</param>
        /// <param name="inventoryView">The inventory view to bind.</param>
        /// <param name="boardVm">The board view model.</param>
        /// <param name="inventoryVm">The inventory view model.</param>
        /// <param name="boardLimitLabel">Optional label for board gear count.</param>
        /// <param name="inventoryLimitLabel">Optional label for inventory gear count.</param>
        /// <param name="isRunningCheck">Optional delegate to check if the engine is running. Defaults to boardVm.EngineService.IsRunning.</param>
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

        /// <summary>
        /// Binds board and inventory views, applies FrustumFit, wires all drag/drop events,
        /// and performs the initial limit label update.
        /// </summary>
        public void Bind()
        {
            if (isBound)
            {
                return;
            }

            isBound = true;

            boardView.Bind(boardVm, interactable: true);

            var frustumFit = GameObject.FindObjectOfType<FrustumFit>();
            if (frustumFit != null)
            {
                frustumFit.Apply();
            }

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

        /// <summary>
        /// Unsubscribes all events and unbinds the board view.
        /// Safe to call multiple times.
        /// </summary>
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
                boardView.Unbind();
            }

            inventoryVm.OnGearDraggedToBoard -= HandleGearDraggedToBoard;
            boardVm.OnGearPlaced -= HandleBoardGearChanged;
            boardVm.OnGearRemoved -= HandleBoardGearChanged;

            if (inventoryVm.InventoryModel?.AvailableGears != null)
            {
                inventoryVm.InventoryModel.AvailableGears.CollectionChanged -= HandleInventoryChanged;
            }
        }

        /// <summary>
        /// Forces a refresh of the limit labels.
        /// </summary>
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
