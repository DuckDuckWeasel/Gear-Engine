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
        private CardOptionView[] cardOptionViews;

        [SerializeField]
        private Button rerollButton;
        [SerializeField]
        private Button continueButton;

        protected override void OnBind()
        {
            ValidateHierarchy();
            BindGearSubtree();
            Bind<int, int>(() => viewModel.CardOptionsRevision, _ => RebuildCardSelection());
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
            DeactivateGearPanels();
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

        private void RebuildCardSelection()
        {
            for (int i = 0; i < cardOptionViews.Length; i++)
            {
                if (i < viewModel.CardOptions.Count)
                {
                    CardOptionViewModel option = viewModel.CardOptions[i];
                    cardOptionViews[i].gameObject.SetActive(true);
                    cardOptionViews[i].Bind(option);
                }
                else
                {
                    cardOptionViews[i].gameObject.SetActive(false);
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
            if (cardOptionViews == null)
            {
                throw new InvalidOperationException("[RoguelikeView] cardOptionViews must be assigned on the scene instance.");
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

        private void DeactivateGearPanels()
        {
            TryDeactivatePanel(trashDropZone?.gameObject);
            TryDeactivatePanel(inventoryView?.gameObject);
            TryDeactivatePanel(boardView?.gameObject);
        }

        private void TryDeactivatePanel(GameObject go)
        {
            if (go != null)
            {
                go.SetActive(false);
            }
        }
    }
}
