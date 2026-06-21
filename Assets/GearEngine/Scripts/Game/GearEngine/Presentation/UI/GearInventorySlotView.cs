using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GearEngine.GearEngine.Presentation.UI
{
    [RequireComponent(typeof(Draggable))]
    public class GearInventorySlotView : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField]
        private Transform visualContainer;
        [SerializeField]
        private Image slotImage;

        public GearItemData BoundGearData => boundGearData;

        public Transform VisualContainer => visualContainer != null ? visualContainer : transform;

        private GearItemData boundGearData;
        private GearInventoryViewModel viewModel;
        private Draggable draggable;

        public void Bind(GearItemData config, GearInventoryViewModel vm)
        {
            boundGearData = config;
            viewModel = vm;

            draggable = GetComponent<Draggable>();
        }

        private void Update()
        {
            if (draggable != null && viewModel != null)
            {
                draggable.IsInteractable = viewModel.CanDrag;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.dragging || viewModel == null || boundGearData == null)
            {
                return;
            }

            Debug.Log($"[GearInventorySlotView] OnPointerClick fired! dragging={eventData.dragging}");
            viewModel.HandleInventoryClick(boundGearData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (slotImage)
            {
                slotImage.enabled =  false;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (slotImage)
            {
                slotImage.enabled =  true;
            }
        }
    }
}
