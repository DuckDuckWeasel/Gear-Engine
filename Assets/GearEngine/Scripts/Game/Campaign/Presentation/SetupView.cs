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

        protected override void OnBind()
        {
            ValidateHierarchy();
            track.Bind(viewModel.Track);
            boardView.Bind(viewModel.Board);
            inventoryView.SetBoardScaleReference(boardView.transform);
            inventoryView.Bind(viewModel.Inventory);
            trashDropZone.SetDragService(viewModel.DragService);
            trashDropZone.Bind(viewModel.TrashZone);
            raceButton.onClick.AddListener(OnRaceClicked);
        }

        protected override void OnUnbind()
        {
            raceButton.onClick.RemoveListener(OnRaceClicked);
            track.Unbind();
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

        private void ValidateHierarchy()
        {
            RequireReference(track, nameof(track));
            RequireReference(boardView, nameof(boardView));
            RequireReference(inventoryView, nameof(inventoryView));
            RequireReference(trashDropZone, nameof(trashDropZone));
            RequireReference(raceButton, nameof(raceButton));
        }

        private void RequireReference(UnityEngine.Object field, string name)
        {
            if (field == null)
            {
                throw new InvalidOperationException($"[SetupView] {name} reference is missing.");
            }
        }
    }
}
