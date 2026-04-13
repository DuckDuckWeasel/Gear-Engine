using System;
using GearEngine.CarSimulation.Track;
using GearEngine.GearEngine.Config;
using GearEngine.Race;
using GearEngine.GearEngine.Presentation.UI;
using Scaffold.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.Race.Presentation
{
    public sealed class RaceView : View<RaceViewModel>
    {
        [SerializeField]
        private BoardView boardView;

        [SerializeField]
        private GearInventoryView inventoryView;

        [SerializeField]
        private Track track;

        [SerializeField]
        private Button raceButton;

        [SerializeField]
        private string raceButtonLabel = "Start";

        protected override void OnBind()
        {
            ValidateRaceViewHierarchy();
            ApplyRaceButtonLabel();
            BindRaceChildViews();
            SubscribeRaceUi();
        }

        protected override void OnUnbind()
        {
            UnsubscribeRaceUi();
            UnbindWorldAndBoard();
            base.OnUnbind();
        }

        private void ApplyRaceButtonLabel()
        {
            if (string.IsNullOrEmpty(raceButtonLabel))
            {
                return;
            }

            TextMeshProUGUI label = raceButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
            {
                label.text = raceButtonLabel;
            }
        }

        private void BindRaceChildViews()
        {
            inventoryView.Bind(viewModel.Inventory);
            boardView.Bind(viewModel.Board, interactable: true);
            track.Bind(viewModel.Track);
        }

        private void HandleGearDraggedToBoard(Vector3 worldPos, GearConfigData gearData)
        {
            try
            {
                TryPlaceInventoryFromDrag(worldPos, gearData);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RaceView] HandleGearDraggedToBoard failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void HandleGearDroppedOverUI(GearConfigData config, Vector3 _)
        {
            if (config != null)
            {
                viewModel.Inventory.AddGearToInventory(config);
            }
        }

        private void OnRaceButtonClicked()
        {
            try
            {
                viewModel?.StartRace();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RaceView] StartRace failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void SubscribeRaceUi()
        {
            boardView.OnGearDroppedOverUI += HandleGearDroppedOverUI;
            viewModel.Inventory.OnGearDraggedToBoard += HandleGearDraggedToBoard;
            raceButton.onClick.AddListener(OnRaceButtonClicked);
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

        private void UnbindWorldAndBoard()
        {
            if (track != null)
            {
                track.Unbind();
            }

            if (boardView != null)
            {
                boardView.Unbind();
            }
        }

        private void UnsubscribeRaceUi()
        {
            if (raceButton != null)
            {
                raceButton.onClick.RemoveListener(OnRaceButtonClicked);
            }

            if (viewModel != null)
            {
                viewModel.Inventory.OnGearDraggedToBoard -= HandleGearDraggedToBoard;
            }

            if (boardView != null)
            {
                boardView.OnGearDroppedOverUI -= HandleGearDroppedOverUI;
            }
        }

        private void ValidateRaceViewHierarchy()
        {
            ThrowIfMissing(boardView, "boardView");
            ThrowIfMissing(inventoryView, "inventoryView");
            ThrowIfMissing(track, "track");
            ThrowIfMissing(raceButton, "raceButton");
        }

        private void ThrowIfMissing(object field, string name)
        {
            if (field == null)
            {
                throw new InvalidOperationException($"[RaceView] {name} reference is missing.");
            }
        }
    }
}
