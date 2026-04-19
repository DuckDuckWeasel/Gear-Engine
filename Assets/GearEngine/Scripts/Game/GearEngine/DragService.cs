using System;
using System.Collections.Generic;
using GearEngine.GearEngine.Presentation.UI;
using UnityEngine;

namespace GearEngine.GearEngine
{
    /// <summary>
    /// Centralized drag state. Broadcasts lifecycle to registered <see cref="IDragLifecycleListener"/> instances.
    /// </summary>
    public sealed class DragService : IDragService
    {
        private readonly List<IDragLifecycleListener> listeners = new List<IDragLifecycleListener>();

        public bool IsDragging { get; private set; }

        public void Register(IDragLifecycleListener listener)
        {
            if (listener == null)
            {
                return;
            }

            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }

        public void Unregister(IDragLifecycleListener listener)
        {
            if (listener == null)
            {
                return;
            }

            listeners.Remove(listener);
        }

        public void StartDrag(DragPayload payload)
        {
            if (payload.Data == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            if (IsDragging)
            {
                Debug.LogWarning("[DragService] StartDrag called while already dragging. Ending previous drag first.");
                EndDrag();
            }

            IsDragging = true;
            BroadcastDragStarted(payload);
        }

        public void EndDrag()
        {
            if (!IsDragging)
            {
                return;
            }

            IsDragging = false;
            BroadcastDragEnded();
        }

        private void BroadcastDragStarted(DragPayload payload)
        {
            IDragLifecycleListener[] snapshot = listeners.ToArray();
            foreach (IDragLifecycleListener listener in snapshot)
            {
                try
                {
                    listener.OnDragStarted(payload);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[DragService] IDragLifecycleListener.OnDragStarted failed: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }

        private void BroadcastDragEnded()
        {
            IDragLifecycleListener[] snapshot = listeners.ToArray();
            foreach (IDragLifecycleListener listener in snapshot)
            {
                try
                {
                    listener.OnDragEnded();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[DragService] IDragLifecycleListener.OnDragEnded failed: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }
    }
}
