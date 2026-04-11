using System;
using Scaffold.MVVM;
using UnityEngine;

namespace Game.GearEngine.Presentation
{
    public class GearEngineView : View<GearEngineViewModel>
    {
        [SerializeField] private SimulationControlView simControlView;
        [SerializeField] private GearInventoryView inventoryView;
        [SerializeField] private BoardView boardView;

        protected override void OnBind()
        {
            simControlView.Bind(viewModel.SimControl);
            inventoryView.Bind(viewModel.Inventory);
            boardView.Bind(viewModel.Board, interactable: true);

            boardView.OnGearDroppedOverUI += HandleGearDroppedOverUI;
            viewModel.Inventory.OnGearDraggedToBoard += HandleGearDraggedToBoard;
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
            catch (Exception ex)
            {
                Debug.LogError($"[GearEngineView] HandleGearDraggedToBoard failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void OnDestroy()
        {
            if (boardView != null)
            {
                boardView.OnGearDroppedOverUI -= HandleGearDroppedOverUI;
                boardView.Unbind();
            }

            if (viewModel != null)
            {
                viewModel.Inventory.OnGearDraggedToBoard -= HandleGearDraggedToBoard;
            }
        }
    }
}
