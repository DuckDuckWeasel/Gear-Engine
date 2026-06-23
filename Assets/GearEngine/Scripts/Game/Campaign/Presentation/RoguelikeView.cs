using System;
using GearEngine.GearEngine.Presentation.UI;
using Scaffold.MVVM;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.Campaign.Presentation
{
    public sealed class RoguelikeView : View<RoguelikeViewModel>
    {
        [SerializeField]
        private BoardViewComponent boardView;

        [SerializeField]
        private GearInventoryViewComponent inventoryView;

        [SerializeField]
        private TrashDropZoneViewComponent trashDropZone;

        [SerializeField]
        private ItemSlotView[] perkOptionViews;

        [SerializeField]
        private Button rerollButton;
        [SerializeField]
        private Button continueButton;

        protected override void OnBind()
        {
            ValidateHierarchy();
            BindGearSubtree();
            Bind<int, int>(() => viewModel.PerkOptionsRevision, _ => RebuildPerkSelection());
            Bind<bool, bool>(() => viewModel.IsProcessingAction, isProcessing => ToggleGearPanels(!isProcessing));
            BindActionUi();
        }

        protected override void OnUnbind()
        {
            continueButton.onClick.RemoveListener(OnContinueClicked);
            if (rerollButton != null)
            {
                rerollButton.onClick.RemoveListener(OnRerollClicked);
            }
            DisposeViewModelIfNeeded();
            ToggleGearPanels(false);
            base.OnUnbind();
        }

        private void BindGearSubtree()
        {
            boardView.gameObject.SetActive(true);
            DragServiceRegistry.Register(viewModel.DragService);
            boardView.Bind(viewModel.Board);
            inventoryView.gameObject.SetActive(true);
            inventoryView.Bind(viewModel.Inventory);
            inventoryView.RebuildAndFit();
            trashDropZone.gameObject.SetActive(true);
            trashDropZone.SetDragService(viewModel.DragService);
            trashDropZone.SetBoardPresentation(boardView.BoardLayout, viewModel.Board.BoardRules);
            trashDropZone.Bind(viewModel.TrashZone);
            trashDropZone.ApplyInitialPlacement();
        }

        private void RebuildPerkSelection()
        {
            if (perkOptionViews == null) return;
            
            for (int i = 0; i < perkOptionViews.Length; i++)
            {
                if (i < viewModel.PerkOptions.Count)
                {
                    ItemSlotViewModel option = viewModel.PerkOptions[i];
                    perkOptionViews[i].gameObject.SetActive(true);
                    perkOptionViews[i].Bind(option);
                }
                else
                {
                    perkOptionViews[i].gameObject.SetActive(false);
                }
            }
        }


        private void BindActionUi()
        {
            continueButton.onClick.AddListener(OnContinueClicked);
            if (rerollButton != null)
            {
                rerollButton.onClick.AddListener(OnRerollClicked);
                Bind<bool, bool>(() => viewModel.CanReroll, canReroll => rerollButton.gameObject.SetActive(canReroll));
            }
        }

        private void OnContinueClicked()
        {
            try
            {
                viewModel?.Continue();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RoguelikeView] OnContinueClicked failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void OnRerollClicked()
        {
            try
            {
                viewModel?.Reroll();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RoguelikeView] OnRerollClicked failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void ValidateHierarchy()
        {
            RequireReference(boardView, nameof(boardView));
            RequireReference(inventoryView, nameof(inventoryView));
            RequireReference(trashDropZone, nameof(trashDropZone));
            RequireReference(continueButton, nameof(continueButton));
            if (perkOptionViews == null)
            {
                throw new InvalidOperationException("[RoguelikeView] perkOptionViews must be assigned on the scene instance.");
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

        private void DisposeViewModelIfNeeded()
        {
            if (viewModel is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        private void ToggleGearPanels(bool isActive)
        {
            if (boardView != null) boardView.gameObject.SetActive(isActive);
            if (inventoryView != null) inventoryView.gameObject.SetActive(isActive);
            if (trashDropZone != null) trashDropZone.gameObject.SetActive(isActive);
        }
    }
}
