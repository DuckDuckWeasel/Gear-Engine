using UnityEngine;
using UnityEngine.UIElements;

namespace OM.Editor
{
    /// <summary>
    /// Routes mouse clicks and thresholded drag operations to the nearest compatible visual ancestor.
    /// </summary>
    public class OM_DragControlManipulator : MouseManipulator
    {
        private const float k_clickMaxDuration = 0.35f;
        private const float k_dragThreshold = 6f;

        private bool isActive;
        private bool isDragging;
        private int mouseButton;
        private Vector2 startMousePosition;
        private Vector2 lastMousePosition;
        private float startTime;
        private IOM_DragControlDraggable draggable;
        private IOM_DragControlClickable clickable;

        /// <summary>
        /// Registers the necessary mouse event callbacks on the target VisualElement.
        /// Called automatically when the manipulator is added to an element.
        /// </summary>
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
            target.RegisterCallback<MouseCaptureOutEvent>(OnMouseCaptureOut);
        }

        /// <summary>
        /// Unregisters the mouse event callbacks from the target VisualElement.
        /// Called automatically when the manipulator is removed from an element or the element is removed from the hierarchy.
        /// </summary>
        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
            target.UnregisterCallback<MouseCaptureOutEvent>(OnMouseCaptureOut);
        }

        /// <summary>
        /// Handles the MouseDownEvent. Initiates a potential drag or click operation.
        /// </summary>
        /// <param name="e">The MouseDownEvent arguments.</param>
        private void OnMouseDown(MouseDownEvent e)
        {
            if (isActive)
            {
                e.StopImmediatePropagation();
                return;
            }

            if (e.target is not VisualElement visualElement)
            {
                return;
            }

            clickable = FindClosestHandler<IOM_DragControlClickable>(visualElement);
            draggable = e.button == (int)MouseButton.LeftMouse
                ? FindClosestHandler<IOM_DragControlDraggable>(visualElement)
                : null;

            if (clickable == null && draggable == null)
            {
                return;
            }

            isActive = true;
            isDragging = false;
            mouseButton = e.button;
            startMousePosition = e.localMousePosition;
            lastMousePosition = startMousePosition;
            startTime = Time.realtimeSinceStartup;

            target.CaptureMouse();
            e.StopPropagation();
        }

        /// <summary>
        /// Handles the MouseMoveEvent. Updates the drag operation if active.
        /// </summary>
        /// <param name="e">The MouseMoveEvent arguments.</param>
        private void OnMouseMove(MouseMoveEvent e)
        {
            if (!isActive || draggable == null)
            {
                return;
            }

            lastMousePosition = e.localMousePosition;
            Vector2 mouseDelta = e.localMousePosition - startMousePosition;

            if (!isDragging)
            {
                if (mouseDelta.magnitude < k_dragThreshold)
                {
                    return;
                }

                isDragging = true;
                draggable.StartDrag(startMousePosition);
            }

            draggable.Drag(mouseDelta, e.localMousePosition);
            e.StopPropagation();
        }

        /// <summary>
        /// Handles the MouseUpEvent. Determines if it was a click or the end of a drag,
        /// and calls the appropriate interface methods. Cleans up state.
        /// </summary>
        /// <param name="e">The MouseUpEvent arguments.</param>
        private void OnMouseUp(MouseUpEvent e)
        {
            if (!isActive || e.button != mouseButton)
            {
                return;
            }

            lastMousePosition = e.localMousePosition;
            Vector2 finalDelta = e.localMousePosition - startMousePosition;

            try
            {
                if (isDragging)
                {
                    draggable?.EndDrag(finalDelta, e.localMousePosition);
                }
                else if (Time.realtimeSinceStartup - startTime < k_clickMaxDuration &&
                         finalDelta.magnitude < k_dragThreshold)
                {
                    clickable?.Click((MouseButton)e.button, e, e.mousePosition);
                }
            }
            finally
            {
                ResetInteraction(true);
                e.StopPropagation();
            }
        }

        private void OnMouseCaptureOut(MouseCaptureOutEvent e)
        {
            if (!isActive)
            {
                return;
            }

            try
            {
                if (isDragging)
                {
                    draggable?.EndDrag(lastMousePosition - startMousePosition, lastMousePosition);
                }
            }
            finally
            {
                ResetInteraction(false);
            }
        }

        private THandler FindClosestHandler<THandler>(VisualElement element)
            where THandler : class
        {
            while (element != null)
            {
                if (element is THandler handler)
                {
                    return handler;
                }

                if (element == target)
                {
                    break;
                }

                element = element.parent;
            }

            return null;
        }

        private void ResetInteraction(bool releaseMouse)
        {
            isActive = false;
            isDragging = false;
            mouseButton = -1;
            draggable = null;
            clickable = null;

            if (releaseMouse && target.HasMouseCapture())
            {
                target.ReleaseMouse();
            }
        }
    }
}
