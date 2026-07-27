using System;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI
{
    public sealed class BoardView : MonoBehaviour
    {
        public BoardViewComponent Board => board;

        public RectTransform DragOverlay => dragOverlay;

        [SerializeField] private BoardViewComponent board;
        [SerializeField] private TrashDropZoneViewComponent trash;
        [SerializeField] private RectTransform dragOverlay;

        public void BindInteractive(
            BoardViewModel boardViewModel,
            TrashZoneViewModel trashViewModel,
            IDragService dragService)
        {
            ValidateReferences(dragService);
            UnbindViews();
            gameObject.SetActive(true);
            board.gameObject.SetActive(true);
            trash.gameObject.SetActive(true);
            board.SetWorkspaceInteractionEnabled(true);
            board.SetDragContext(dragService, dragOverlay);
            board.Bind(boardViewModel);
            trash.SetDragService(dragService);
            trash.Bind(trashViewModel);
            trash.ApplyInitialState();
        }

        public void BindReadOnly(BoardViewModel boardViewModel)
        {
            if (boardViewModel == null)
            {
                throw new ArgumentNullException(nameof(boardViewModel));
            }

            ValidateCommonReferences();
            UnbindViews();
            gameObject.SetActive(true);
            board.gameObject.SetActive(true);
            trash.gameObject.SetActive(false);
            board.SetWorkspaceInteractionEnabled(false);
            board.Bind(boardViewModel);
        }

        public void Unbind()
        {
            UnbindViews();
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        internal void SetReferences(
            BoardViewComponent boardView,
            TrashDropZoneViewComponent trashView,
            RectTransform overlay)
        {
            board = boardView;
            trash = trashView;
            dragOverlay = overlay;
        }

        private void ValidateReferences(IDragService dragService)
        {
            ValidateCommonReferences();
            if (dragService == null)
            {
                throw new ArgumentNullException(nameof(dragService));
            }
        }

        private void ValidateCommonReferences()
        {
            if (board == null)
            {
                throw new InvalidOperationException("[BoardView] Board is missing.");
            }

            if (trash == null)
            {
                throw new InvalidOperationException("[BoardView] Trash zone is missing.");
            }

            if (dragOverlay == null)
            {
                throw new InvalidOperationException("[BoardView] Drag overlay is missing.");
            }
        }

        private void UnbindViews()
        {
            board?.Unbind();
            trash?.Unbind();
        }
    }
}
