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
        public readonly IDragSource Source;

        public DragPayload(object data, Vector3 worldPos, IDragSource source)
        {
            Data = data;
            WorldPosition = worldPos;
            Source = source;
        }

        public T GetData<T>() where T : class => Data as T;
    }
}
