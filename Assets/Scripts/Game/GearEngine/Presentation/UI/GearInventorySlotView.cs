using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI
{
    [RequireComponent(typeof(DragHandler))]
    public class GearInventorySlotView : MonoBehaviour
    {
        private GearInventoryViewModel viewModel;
        private DragHandler dragHandler;
        private GearConfigData boundGearData;

        public GearConfigData BoundGearData => boundGearData;

        public void Bind(GearConfigData config, GearInventoryViewModel vm)
        {
            boundGearData = config;
            viewModel = vm;

            dragHandler = GetComponent<DragHandler>();
            dragHandler.OnValidDropWorldPos -= HandleValidDrop;
            dragHandler.OnValidDropWorldPos += HandleValidDrop;
        }

        private void HandleValidDrop(Vector3 targetWorldPosition)
        {
            if (BoundGearData == null)
            {
                return;
            }

            viewModel?.NotifyGearDropped(targetWorldPosition, BoundGearData);

            Debug.Log($"<color=#aaaaff>[GearInventorySlotView]</color> Drag & Drop confirmed valid tag overlap. Notified ViewModel.");
        }

        private void Update()
        {
            if (dragHandler != null && viewModel != null)
            {
                dragHandler.IsInteractable = viewModel.CanDrag;
            }
        }

        private void OnDestroy()
        {
            if (dragHandler != null)
            {
                dragHandler.OnValidDropWorldPos -= HandleValidDrop;
            }
        }
    }
}
