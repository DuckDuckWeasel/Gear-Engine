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
        [SerializeField] private GearWorkspaceView workspace;
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

            workspace.BindInteractive(
                viewModel.Board,
                viewModel.Inventory,
                viewModel.TrashZone,
                viewModel.DragService);
        }

        protected override void OnOpen(bool wasHidden)
        {
            base.OnOpen(wasHidden);
            track.gameObject.SetActive(true);
            workspace.SetVisible(true);

            raceButton.onClick.RemoveListener(OnRaceClicked);
            raceButton.onClick.AddListener(OnRaceClicked);
            returnToMainButton.onClick.RemoveListener(OnReturnClicked);
            returnToMainButton.onClick.AddListener(OnReturnClicked);

            FrustumFitAnchorOpenTransition.PlayAfterCanvasLayout(
                this,
                openTransitionAnchors,
                openTransitionDurationSeconds,
                onComplete: () =>
                {
                    workspace.Board.SpinAllGearsOnceVisual();
                });
        }

        protected override void OnClose(bool hiding)
        {
            base.OnClose(hiding);
            if (hiding)
            {
                return;
            }

            raceButton.onClick.RemoveListener(OnRaceClicked);
            returnToMainButton.onClick.RemoveListener(OnReturnClicked);
            workspace?.SetVisible(false);

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
            RequireReference(workspace, nameof(workspace));
            RequireReference(raceButton, nameof(raceButton));
            RequireReference(returnToMainButton, nameof(returnToMainButton));
        }

        private void RequireReference(UnityEngine.Object field, string name)
        {
            if (field == null)
            {
                throw new InvalidOperationException(
                    $"[SetupView] {name} must be assigned in the Setup prefab.");
            }
        }
    }
}
