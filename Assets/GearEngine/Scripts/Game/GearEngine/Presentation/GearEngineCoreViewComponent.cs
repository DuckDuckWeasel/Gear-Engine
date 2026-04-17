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
            boardView.Bind(viewModel.Board);
            inventoryView.SetBoardScaleReference(boardView.transform);
            inventoryView.Bind(viewModel.Inventory);
            trashDropZone.SetDragService(viewModel.DragService);
            trashDropZone.Bind(viewModel.TrashZone);
        }
    }
}
