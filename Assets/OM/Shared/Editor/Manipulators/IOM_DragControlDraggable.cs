using UnityEngine;

namespace OM.Editor
{
    /// <summary>
    /// Receives drag operations routed by <see cref="OM_DragControlManipulator"/>.
    /// </summary>
    public interface IOM_DragControlDraggable
    {
        /// <summary>
        /// Starts a drag at the supplied local mouse position.
        /// </summary>
        /// <param name="mousePosition">The local mouse position where the drag started.</param>
        void StartDrag(Vector2 mousePosition);

        /// <summary>
        /// Updates an active drag.
        /// </summary>
        /// <param name="delta">The total movement since the drag started.</param>
        /// <param name="mousePosition">The current local mouse position.</param>
        void Drag(Vector2 delta, Vector2 mousePosition);

        /// <summary>
        /// Completes an active drag.
        /// </summary>
        /// <param name="delta">The total movement since the drag started.</param>
        /// <param name="mousePosition">The local mouse position where the drag ended.</param>
        void EndDrag(Vector2 delta, Vector2 mousePosition);
    }
}
