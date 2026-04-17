namespace GearEngine.GearEngine.Presentation.UI
{
    /// <summary>
    /// Drop target: receives lifecycle broadcasts from <see cref="GearEngine.IDragService"/> when registered,
    /// and spatial resolution from <see cref="DragTargetFinder"/> at drop time.
    /// </summary>
    public interface IDragTarget
    {
        void OnDragStarted(DragPayload payload);
        void OnDragEnded();

        bool CanAccept(DragPayload payload);
        void OnDrop(DragPayload payload);

        void OnHoverEnter(DragPayload payload);
        void OnHoverExit();
    }

    /// <summary>
    /// Drag origin — receives callbacks when a spatial <see cref="IDragTarget"/> accepts or rejects a drop.
    /// </summary>
    public interface IDragSource
    {
        void OnDropAccepted(IDragTarget by);
        void OnDropRejected();
    }
}
