using System;

namespace GearEngine.GearEngine
{
    /// <summary>
    /// Generic drag service for any UI context.
    /// Drag sources call <see cref="StartDrag"/>/<see cref="EndDrag"/>;
    /// consumers subscribe to <see cref="OnDragStarted"/>/<see cref="OnDragEnded"/>.
    /// </summary>
    public interface IDragService
    {
        bool IsDragging { get; }

        /// <summary>Returns the dragged data cast to T, or null if no active drag or type mismatch.</summary>
        T GetDragData<T>() where T : class;

        event Action<object> OnDragStarted;
        event Action OnDragEnded;

        void StartDrag(object data);
        void EndDrag();
    }
}
