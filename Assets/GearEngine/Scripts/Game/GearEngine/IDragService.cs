using GearEngine.GearEngine.Presentation.UI;

namespace GearEngine.GearEngine
{
    /// <summary>
    /// Thin drag state broadcaster. Sources call <see cref="StartDrag"/>/<see cref="EndDrag"/>.
    /// Registered <see cref="IDragLifecycleListener"/> instances receive lifecycle callbacks.
    /// </summary>
    public interface IDragService
    {
        bool IsDragging { get; }

        void StartDrag(DragPayload payload);

        void EndDrag();

        void Register(IDragLifecycleListener listener);

        void Unregister(IDragLifecycleListener listener);
    }
}
