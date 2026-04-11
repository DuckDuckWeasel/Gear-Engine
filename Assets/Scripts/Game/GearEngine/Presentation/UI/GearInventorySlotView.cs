using UnityEngine;

namespace Game.GearEngine.Presentation
{
    [RequireComponent(typeof(DragHandler))]
    public class GearInventorySlotView : MonoBehaviour
    {
        private GearInventoryViewModel viewModel;
        private DragHandler dragHandler;

        public GearConfigData BoundGearData { get; private set; }

        public void Bind(GearConfigData config, GearInventoryViewModel vm)
        {
            BoundGearData = config;
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
