using GearEngine.GearEngine.Presentation.UI;

namespace GearEngine.GearEngine
{
    /// <summary>
    /// Central drag state. Sources call <see cref="StartDrag"/>/<see cref="EndDrag"/>.
    /// Registered <see cref="IDragTarget"/> instances receive lifecycle callbacks on those calls.
    /// </summary>
    public interface IDragService
    {
        bool IsDragging { get; }

        /// <summary>Returns the dragged data cast to T, or null if no active drag or type mismatch.</summary>
        T GetDragData<T>() where T : class;

        void StartDrag(object data);
        void EndDrag();

        void Register(IDragTarget target);
        void Unregister(IDragTarget target);
    }
}
