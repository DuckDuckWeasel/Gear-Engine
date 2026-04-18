using System;
using GearEngine.CarSimulation.Tracks;
using GearEngine.GearEngine.Presentation.UI;
using Scaffold.MVVM;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.Campaign.Presentation
{
    public sealed class SetupView : View<SetupViewModel>
    {
        [SerializeField] private Track track;
        [SerializeField] private BoardViewComponent boardView;
        [SerializeField] private GearInventoryViewComponent inventoryView;
        [SerializeField] private TrashDropZoneViewComponent trashDropZone;
        [SerializeField] private Button raceButton;
        [SerializeField] private Button returnToMainButton;

        protected override void OnBind()
        {
            ValidateHierarchy();
            if (track == null)
            {
                throw new InvalidOperationException(
                    "[SetupView] Track must be assigned on the scene instance (not baked into the prefab).");
            }

            track.gameObject.SetActive(true);
            track.Bind(viewModel.Track);

            boardView.gameObject.SetActive(true);
            boardView.Bind(viewModel.Board);
            inventoryView.gameObject.SetActive(true);
            inventoryView.SetBoardScaleReference(boardView.transform);
            inventoryView.Bind(viewModel.Inventory);
            trashDropZone.gameObject.SetActive(true);
            trashDropZone.SetDragService(viewModel.DragService);
            trashDropZone.Bind(viewModel.TrashZone);
            raceButton.onClick.AddListener(OnRaceClicked);
            returnToMainButton.onClick.AddListener(OnReturnClicked);
        }

        protected override void OnUnbind()
        {
            raceButton.onClick.RemoveListener(OnRaceClicked);
            returnToMainButton.onClick.RemoveListener(OnReturnClicked);

            if (trashDropZone != null)
            {
                trashDropZone.Unbind();
                trashDropZone.gameObject.SetActive(false);
            }

            if (inventoryView != null)
            {
                inventoryView.Unbind();
                inventoryView.gameObject.SetActive(false);
            }

            if (boardView != null)
            {
                boardView.Unbind();
                boardView.gameObject.SetActive(false);
            }

            if (track != null)
            {
                track.Unbind();
                track.gameObject.SetActive(false);
            }

            base.OnUnbind();
        }

        private void OnRaceClicked()
        {
            try
            {
                viewModel?.GoToRace();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SetupView] OnRaceClicked failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void OnReturnClicked()
        {
            try
            {
                viewModel?.ReturnClicked();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SetupView] OnReturnToMainClicked failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void ValidateHierarchy()
        {
            RequireReference(boardView, nameof(boardView));
            RequireReference(inventoryView, nameof(inventoryView));
            RequireReference(trashDropZone, nameof(trashDropZone));
            RequireReference(raceButton, nameof(raceButton));
            RequireReference(returnToMainButton, nameof(returnToMainButton));
        }

        private void RequireReference(UnityEngine.Object field, string name)
        {
            if (field == null)
            {
                throw new InvalidOperationException(
                    $"[SetupView] {name} must be assigned on the scene instance (shared World gear UI / controls).");
            }
        }
    }
}
