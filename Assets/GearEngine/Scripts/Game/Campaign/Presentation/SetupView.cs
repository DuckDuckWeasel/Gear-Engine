using System;
using GearEngine.CarSimulation.Tracks;
using GearEngine.FrustumFit;
using GearEngine.GearEngine.Presentation.UI;
using Scaffold.MVVM;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.Campaign.Presentation
{
    public sealed class SetupView : View<SetupViewModel>
    {
        [SerializeField] private TrackViewComponent track;
        [SerializeField] private BoardViewComponent boardView;
        [SerializeField] private GearInventoryViewComponent inventoryView;
        [SerializeField] private TrashDropZoneViewComponent trashDropZone;
        [SerializeField] private Button raceButton;
        [SerializeField] private Button returnToMainButton;
        [SerializeField] private FrustumFitAnchor[] openTransitionAnchors;
        [SerializeField] private float openTransitionDurationSeconds = 0.35f;

        protected override void OnBind()
        {
            ValidateHierarchy();
            if (track == null)
            {
                throw new InvalidOperationException(
                    "[SetupView] Track must be assigned on the scene instance (not baked into the prefab).");
            }

            track.Bind(viewModel.Track);
            
            boardView.Bind(viewModel.Board);
            inventoryView.SetBoardRoot(boardView.GetBoardSpaceRoot());
            inventoryView.Bind(viewModel.Inventory);
            
            trashDropZone.SetDragService(viewModel.DragService);
            trashDropZone.Bind(viewModel.TrashZone);
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            track.gameObject.SetActive(true);
            boardView.gameObject.SetActive(true);
            inventoryView.gameObject.SetActive(true);
            trashDropZone.gameObject.SetActive(true);
            
            raceButton.onClick.RemoveListener(OnRaceClicked);
            raceButton.onClick.AddListener(OnRaceClicked);
            returnToMainButton.onClick.RemoveListener(OnReturnClicked);
            returnToMainButton.onClick.AddListener(OnReturnClicked);

            FrustumFitAnchorOpenTransition.PlayAfterCanvasLayout(this, openTransitionAnchors, openTransitionDurationSeconds);
        }

        protected override void OnClose()
        {
            base.OnClose();
            raceButton.onClick.RemoveListener(OnRaceClicked);
            returnToMainButton.onClick.RemoveListener(OnReturnClicked);
            if (trashDropZone != null)
            {
                trashDropZone.gameObject.SetActive(false);
            }

            if (inventoryView != null)
            {
                inventoryView.gameObject.SetActive(false);
            }

            if (boardView != null)
            {
                boardView.gameObject.SetActive(false);
            }

            if (track != null)
            {
                track.gameObject.SetActive(false);
            }
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
