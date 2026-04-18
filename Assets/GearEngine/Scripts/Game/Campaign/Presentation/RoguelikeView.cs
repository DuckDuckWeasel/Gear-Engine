using System;
using GearEngine.GearEngine.Presentation.UI;
using Scaffold.MVVM;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.Campaign.Presentation
{
    public sealed class RoguelikeView : View<RoguelikeViewModel>
    {
        [SerializeField] private BoardViewComponent boardView;
        [SerializeField] private GearInventoryViewComponent inventoryView;
        [SerializeField] private TrashDropZoneViewComponent trashDropZone;
        [SerializeField] private CardOptionView[] cardOptionViews;
        [SerializeField] private Button confirmButton;

        protected override void OnBind()
        {
            ValidateHierarchy();
            BindGearSubtree();
            BindCardSelection();
            BindConfirmUi();
        }

        protected override void OnUnbind()
        {
            confirmButton.onClick.RemoveListener(OnConfirmClicked);

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

            base.OnUnbind();
        }

        private void BindGearSubtree()
        {
            boardView.gameObject.SetActive(true);
            boardView.Bind(viewModel.Board);
            inventoryView.gameObject.SetActive(true);
            inventoryView.SetBoardScaleReference(boardView.transform);
            inventoryView.Bind(viewModel.Inventory);
            trashDropZone.gameObject.SetActive(true);
            trashDropZone.SetDragService(viewModel.DragService);
            trashDropZone.Bind(viewModel.TrashZone);
        }

        private void BindCardSelection()
        {
            for (int i = 0; i < cardOptionViews.Length && i < viewModel.CardOptions.Count; i++)
            {
                CardOptionViewModel option = viewModel.CardOptions[i];
                cardOptionViews[i].Bind(option);
                Bind<bool, bool>(() => option.IsSelected, selected => OnCardOptionSelectionChanged(option, selected));
            }
        }

        private void OnCardOptionSelectionChanged(CardOptionViewModel option, bool selected)
        {
            if (selected)
            {
                viewModel.SelectCard(option);
            }
        }

        private void BindConfirmUi()
        {
            Bind<bool, bool>(() => viewModel.CanConfirm, UpdateConfirmInteractable);
            confirmButton.interactable = false;
            confirmButton.onClick.AddListener(OnConfirmClicked);
        }

        private void UpdateConfirmInteractable(bool ready)
        {
            confirmButton.interactable = ready;
        }

        private void OnConfirmClicked()
        {
            try
            {
                viewModel?.Confirm();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RoguelikeView] OnConfirmClicked failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void ValidateHierarchy()
        {
            RequireReference(boardView, nameof(boardView));
            RequireReference(inventoryView, nameof(inventoryView));
            RequireReference(trashDropZone, nameof(trashDropZone));
            RequireReference(confirmButton, nameof(confirmButton));
            if (cardOptionViews == null || cardOptionViews.Length == 0)
            {
                throw new InvalidOperationException("[RoguelikeView] cardOptionViews array is empty.");
            }
        }

        private void RequireReference(UnityEngine.Object field, string name)
        {
            if (field == null)
            {
                throw new InvalidOperationException(
                    $"[RoguelikeView] {name} must be assigned on the scene instance (shared World gear UI / controls).");
            }
        }
    }
}
