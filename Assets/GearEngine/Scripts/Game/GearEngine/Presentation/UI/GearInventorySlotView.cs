using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI
{
    [RequireComponent(typeof(DragHandler))]
    public class GearInventorySlotView : MonoBehaviour
    {
        public GearConfigData BoundGearData => boundGearData;

        private GearConfigData boundGearData;
        private GearInventoryViewModel viewModel;
        private DragHandler dragHandler;

        public void Bind(GearConfigData config, GearInventoryViewModel vm)
        {
            boundGearData = config;
            viewModel = vm;

            dragHandler = GetComponent<DragHandler>();
        }

        private void Update()
        {
            if (dragHandler != null && viewModel != null)
            {
                dragHandler.IsInteractable = viewModel.CanDrag;
            }
        }
    }
}
