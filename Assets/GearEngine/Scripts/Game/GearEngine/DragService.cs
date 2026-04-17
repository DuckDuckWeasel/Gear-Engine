using System;
using System.Collections.Generic;
using GearEngine.GearEngine.Presentation.UI;
using UnityEngine;

namespace GearEngine.GearEngine
{
    /// <summary>
    /// Centralized drag state. Broadcasts lifecycle to registered <see cref="IDragTarget"/> instances.
    /// </summary>
    public sealed class DragService : IDragService
    {
        private readonly List<IDragTarget> registeredTargets = new List<IDragTarget>();

        public bool IsDragging { get; private set; }

        private object dragData;

        public T GetDragData<T>() where T : class
        {
            return dragData as T;
        }

        public void Register(IDragTarget target)
        {
            if (target == null)
            {
                return;
            }

            if (!registeredTargets.Contains(target))
            {
                registeredTargets.Add(target);
            }
        }

        public void Unregister(IDragTarget target)
        {
            if (target == null)
            {
                return;
            }

            registeredTargets.Remove(target);
        }

        public void StartDrag(object data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (IsDragging)
            {
                Debug.LogWarning("[DragService] StartDrag called while already dragging. Ending previous drag first.");
                EndDrag();
            }

            dragData = data;
            IsDragging = true;

            var payload = new DragPayload(dragData, Vector3.zero, null);
            BroadcastDragStarted(payload);
        }

        public void EndDrag()
        {
            dragData = null;
            IsDragging = false;
            BroadcastDragEnded();
        }

        private void BroadcastDragStarted(DragPayload payload)
        {
            IDragTarget[] snapshot = registeredTargets.ToArray();
            foreach (IDragTarget target in snapshot)
            {
                try
                {
                    target.OnDragStarted(payload);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[DragService] IDragTarget.OnDragStarted failed: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }

        private void BroadcastDragEnded()
        {
            IDragTarget[] snapshot = registeredTargets.ToArray();
            foreach (IDragTarget target in snapshot)
            {
                try
                {
                    target.OnDragEnded();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[DragService] IDragTarget.OnDragEnded failed: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }
    }
}
