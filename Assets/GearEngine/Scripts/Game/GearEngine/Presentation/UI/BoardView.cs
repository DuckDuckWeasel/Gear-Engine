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
            board.gameObject.SetActive(true);
            trash.gameObject.SetActive(true);
            board.SetWorkspaceInteractionEnabled(true);
            board.SetDragContext(dragService, dragOverlay);
            board.Bind(boardViewModel);
            trash.SetDragService(dragService);
            trash.Bind(trashViewModel);
            trash.ApplyInitialState();
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

            if (dragService == null)
            {
                throw new ArgumentNullException(nameof(dragService));
            }
        }
    }
}
