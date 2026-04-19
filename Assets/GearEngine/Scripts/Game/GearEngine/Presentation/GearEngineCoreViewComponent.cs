using System;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Presentation.UI;
using Scaffold.MVVM;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation
{
    public sealed class GearEngineCoreViewComponent : ViewComponent<GearEngineViewModel>
    {
        [SerializeField] private GearInventoryViewComponent inventoryView;
        [SerializeField] private BoardViewComponent boardView;
        [SerializeField] private TrashDropZoneViewComponent trashDropZone;

        protected override void OnBind()
        {
            base.OnBind();
            EnsureBoardCameraPhysics2DRaycaster();
            DragServiceRegistry.Register(viewModel.DragService);
            boardView.Bind(viewModel.Board);
            inventoryView.Bind(viewModel.Inventory);
            trashDropZone.SetDragService(viewModel.DragService);
            trashDropZone.SetBoardPresentation(boardView.BoardLayout, viewModel.Board.BoardRules);
            trashDropZone.Bind(viewModel.TrashZone);
            trashDropZone.ApplyInitialPlacement();
        }

        private static void EnsureBoardCameraPhysics2DRaycaster()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                return;
            }

            Type raycasterType = Type.GetType("UnityEngine.UI.Physics2DRaycaster, UnityEngine.UI");
            if (raycasterType != null && cam.GetComponent(raycasterType) == null)
            {
                cam.gameObject.AddComponent(raycasterType);
            }
        }
    }
}
