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

        internal GearInventoryViewComponent InventoryView => inventoryView;
        internal BoardViewComponent BoardView => boardView;
        internal TrashDropZoneViewComponent TrashDropZone => trashDropZone;

        protected override void OnBind()
        {
            base.OnBind();

            viewModel.BindSubPresentation(boardView, inventoryView, trashDropZone);
        }
    }
}
