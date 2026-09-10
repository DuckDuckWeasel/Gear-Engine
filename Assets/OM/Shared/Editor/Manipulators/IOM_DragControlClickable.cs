using UnityEngine;
using UnityEngine.UIElements;

namespace OM.Editor
{
    /// <summary>
    /// Receives click operations routed by <see cref="OM_DragControlManipulator"/>.
    /// </summary>
    public interface IOM_DragControlClickable
    {
        /// <summary>
        /// Handles a completed click.
        /// </summary>
        /// <param name="mouseButton">The mouse button that was clicked.</param>
        /// <param name="mouseUpEvent">The source mouse-up event.</param>
        /// <param name="mousePosition">The local mouse position where the click completed.</param>
        void Click(MouseButton mouseButton, MouseUpEvent mouseUpEvent, Vector2 mousePosition);
    }
}
