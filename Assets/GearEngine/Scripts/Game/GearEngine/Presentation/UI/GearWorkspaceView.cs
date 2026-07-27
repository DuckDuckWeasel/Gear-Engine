using System;
using GearEngine.GearEngine.Config;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI
{
    public sealed class GearWorkspaceView : MonoBehaviour
    {
        public BoardViewComponent Board => board;

        public GearWorkspaceMode Mode => mode;

        [SerializeField]
        private GearWorkspaceMode mode;

        [SerializeField]
        private BoardViewComponent board;

        [SerializeField]
        private GearInventoryViewComponent inventory;

        [SerializeField]
        private TrashDropZoneViewComponent trash;

        [SerializeField]
        private RectTransform dragOverlay;

        public void BindInteractive(BoardViewModel boardViewModel, GearInventoryViewModel inventoryViewModel, TrashZoneViewModel trashViewModel, IDragService dragService)
        {
            ValidateInteractiveReferences(dragService);
            mode = GearWorkspaceMode.Interactive;
            ActivateInteractiveViews();
            ConfigureDragContext(dragService);
            BindInteractiveViews(boardViewModel, inventoryViewModel, trashViewModel, dragService);
        }

        private void ActivateInteractiveViews()
        {
            board.gameObject.SetActive(true);
            inventory.gameObject.SetActive(true);
            trash.gameObject.SetActive(true);
            board.SetWorkspaceInteractionEnabled(true);
        }

        private void ConfigureDragContext(IDragService dragService)
        {
            board.SetDragContext(dragService, dragOverlay);
            inventory.SetDragContext(dragService, dragOverlay);
        }

        private void BindInteractiveViews(BoardViewModel boardViewModel, GearInventoryViewModel inventoryViewModel, TrashZoneViewModel trashViewModel, IDragService dragService)
        {
            board.Bind(boardViewModel);
            inventory.Bind(inventoryViewModel);
            inventory.RebuildAndFit();
            trash.SetDragService(dragService);
            trash.SetBoardPresentation(board.BoardLayout, board.TopRightCell);
            trash.Bind(trashViewModel);
            trash.ApplyInitialPlacement();
        }

        public void BindReadOnly(BoardViewModel boardViewModel)
        {
            ValidateCommonReferences();
            mode = GearWorkspaceMode.ReadOnly;
            board.gameObject.SetActive(true);
            board.SetWorkspaceInteractionEnabled(false);
            if (inventory != null)
            {
                inventory.gameObject.SetActive(false);
            }

            if (trash != null)
            {
                trash.gameObject.SetActive(false);
            }

            board.Bind(boardViewModel);
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        internal void SetReferences(GearWorkspaceMode workspaceMode, BoardViewComponent boardView, GearInventoryViewComponent inventoryView, TrashDropZoneViewComponent trashView, RectTransform overlay)
        {
            mode = workspaceMode;
            board = boardView;
            inventory = inventoryView;
            trash = trashView;
            dragOverlay = overlay;
        }

        private void ValidateInteractiveReferences(IDragService dragService)
        {
            ValidateCommonReferences();
            if (inventory == null)
            {
                throw new InvalidOperationException("[GearWorkspaceView] Inventory is missing.");
            }
            if (trash == null)
            {
                throw new InvalidOperationException("[GearWorkspaceView] Trash zone is missing.");
            }
            if (dragService == null)
            {
                throw new ArgumentNullException(nameof(dragService));
            }
        }

        private void ValidateCommonReferences()
        {
            if (board == null)
            {
                throw new InvalidOperationException("[GearWorkspaceView] Board is missing.");
            }
            if (dragOverlay == null)
            {
                throw new InvalidOperationException("[GearWorkspaceView] Drag overlay is missing.");
            }
        }
    }
}
