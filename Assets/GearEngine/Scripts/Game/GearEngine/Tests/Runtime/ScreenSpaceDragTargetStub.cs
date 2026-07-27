using GearEngine.GearEngine.Presentation.UI;
using UnityEngine;

namespace GearEngine.GearEngine.Tests.Runtime
{
    public sealed class ScreenSpaceDragTargetStub : MonoBehaviour, IDragTarget
    {
        public bool AcceptsPayload { get; set; } = true;

        public DragPayload LastPayload { get; private set; }

        public bool CanAccept(DragPayload payload)
        {
            return AcceptsPayload;
        }

        public bool OnDrop(DragPayload payload)
        {
            LastPayload = payload;
            return AcceptsPayload;
        }
    }
}
