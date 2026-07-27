using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.GearEngine.Presentation.UI.Tags.Highlight
{
    public class TargetCanvasState
    {
        public Canvas canvas;
        public bool wasAdded;
        public bool originalOverride;
        public int originalOrder;
        public GraphicRaycaster raycaster;
        public bool raycasterWasAdded;

        public UnityEngine.Rendering.SortingGroup sortingGroup;
        public bool wasSortingGroupAdded;
        public int originalSortingLayer;
        public int originalSortingOrder;
    }
}
