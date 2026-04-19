using GearEngine.GearEngine.Config;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI
{
    [RequireComponent(typeof(Draggable))]
    public class GearInventorySlotView : MonoBehaviour
    {
        [SerializeField]
        private Transform visualContainer;

        public GearConfigData BoundGearData => boundGearData;

        public Transform VisualContainer => visualContainer != null ? visualContainer : transform;

        private GearConfigData boundGearData;
        private GearInventoryViewModel viewModel;
        private Draggable draggable;

        public void Bind(GearConfigData config, GearInventoryViewModel vm)
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
    }
}
