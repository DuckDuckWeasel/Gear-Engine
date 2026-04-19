namespace GearEngine.GearEngine.Presentation.UI
{
    /// <summary>
    /// Spatial drop target resolved by <see cref="DragTargetFinder"/> at drop time.
    /// </summary>
    public interface IDragTarget
    {
        bool CanAccept(DragPayload payload);

        // todo: return value documents whether the drop produced a state change.
        bool OnDrop(DragPayload payload);
    }

    /// <summary>
    /// Receives drag lifecycle from <see cref="GearEngine.IDragService"/> when registered.
    /// </summary>
    public interface IDragLifecycleListener
    {
        void OnDragStarted(DragPayload payload);

        void OnDragEnded();
    }
}
