using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI
{
    /// <summary>
    /// Data passed between drag sources and targets during lifecycle and drop resolution.
    /// </summary>
    public readonly struct DragPayload
    {
        public readonly object Data;
        public readonly Vector3 WorldPosition;

        public DragPayload(object data, Vector3 worldPos)
        {
            Data = data;
            WorldPosition = worldPos;
        }

        public T GetData<T>() where T : class => Data as T;
    }
}
