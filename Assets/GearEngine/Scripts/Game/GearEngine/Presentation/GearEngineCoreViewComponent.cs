using GearEngine.GearEngine.Presentation.UI;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation
{
    /// <summary>
    /// Binds board, inventory, and trash UI to a <see cref="GearEngineViewModel"/>.
    /// Kept as a plain <see cref="MonoBehaviour"/> so nested prefabs can hold serialized references without a separate core ViewModel.
    /// </summary>
    public sealed class GearEngineCoreViewComponent : MonoBehaviour
    {
        [SerializeField] private GearInventoryViewComponent inventoryView;
        [SerializeField] private BoardViewComponent boardView;
        [SerializeField] private TrashDropZoneViewComponent trashDropZone;

        internal GearInventoryViewComponent InventoryView => inventoryView;
        internal BoardViewComponent BoardView => boardView;
        internal TrashDropZoneViewComponent TrashDropZone => trashDropZone;

        internal void BindPresentation(GearEngineViewModel viewModel)
        {
            if (viewModel == null)
            {
                return;
            }

            boardView?.Bind(viewModel.Board);
            inventoryView?.Bind(viewModel.Inventory);
            trashDropZone?.Bind(viewModel.TrashZone);
        }
    }
}
