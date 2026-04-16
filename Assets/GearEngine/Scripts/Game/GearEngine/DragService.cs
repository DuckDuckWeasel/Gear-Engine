using System;
using UnityEngine;

namespace GearEngine.GearEngine
{
    public sealed class DragService : IDragService
    {
        public bool IsDragging { get; private set; }

        private object dragData;

        public event Action<object> OnDragStarted;
        public event Action OnDragEnded;

        public T GetDragData<T>() where T : class
        {
            return dragData as T;
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
            OnDragStarted?.Invoke(data);
        }

        public void EndDrag()
        {
            dragData = null;
            IsDragging = false;
            OnDragEnded?.Invoke();
        }
    }
}
