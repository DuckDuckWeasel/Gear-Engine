using System;

namespace GearEngine.GearEngine
{
    public interface IDragService
    {
        bool IsDragging { get; }

        T GetDragData<T>() where T : class;

        event Action<object> OnDragStarted;
        event Action OnDragEnded;

        void StartDrag(object data);
        void EndDrag();
    }
}
