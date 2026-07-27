using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI
{
    public readonly struct DragPayload
    {
        public DragPayload(object data, Vector2 screenPosition)
        {
            Data = data;
            ScreenPosition = screenPosition;
        }

        public readonly object Data;
        public readonly Vector2 ScreenPosition;

        public T GetData<T>() where T : class
        {
            return Data as T;
        }
    }
}
