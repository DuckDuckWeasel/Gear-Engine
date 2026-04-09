using UnityEngine;
using Scaffold.Events;
using Game.GearEngine.Events;
using VContainer;

namespace Game.GearEngine.Presentation
{
    [RequireComponent(typeof(DragHandler))]
    public class GearInventorySlotView : MonoBehaviour
    {
        private GearInventoryViewModel viewModel;
        private EventController eventController;
        private DragHandler dragHandler;

        public GearConfigData BoundGearData { get; private set; }

        [Inject]
        public void Construct(EventController eventController)
        {
            this.eventController = eventController;
        }

        public void Bind(GearConfigData config, GearInventoryViewModel vm)
        {
            BoundGearData = config;
            viewModel = vm;
            
            dragHandler = GetComponent<DragHandler>();
            // Standardizing cleanup just in case
            dragHandler.OnValidDropWorldPos -= HandleValidDrop; 
            dragHandler.OnValidDropWorldPos += HandleValidDrop;
        }

        private void HandleValidDrop(Vector3 targetWorldPosition)
        {
            if (BoundGearData == null) return;

            // Decoupled communication: The inventory slot successfully dropped its ghost over the "GridBoard" tag.
            // We blindly notify the system. If the BoardView thinks it's a valid empty grid cell, it will deduct the piece from the VM!
            eventController?.Raise(new GearDroppedFromUIEvent(targetWorldPosition, BoundGearData));
            
            Debug.Log($"<color=#aaaaff>[GearInventorySlotView]</color> Drag & Drop confirmed valid tag overlap. Firing Event.");
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
