using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GearEngine.GearEngine.Presentation.UI
{
    public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDragSource
    {
        public bool IsInteractable { get; set; } = true;

        public Action<PointerEventData> OnDragBegin;
        public Action<PointerEventData> OnDragMoved;
        public Action<PointerEventData> OnDragEnd;

        /// <summary>Builds a <see cref="DragPayload"/> for the current drag at the given world hit position.</summary>
        public Func<Vector3, DragPayload> BuildPayload;

        /// <summary>Invoked when a target accepts the drop (after <see cref="IDragTarget.OnDrop"/>).</summary>
        public Action<IDragTarget> OnDragAccepted;

        /// <summary>
        /// World position from the pointer ray (same logic as drop resolution). Use for positioning a board-space drag ghost.
        /// </summary>
        public static bool TryGetPointerWorldPosition(PointerEventData eventData, out Vector3 worldPosition)
        {
            worldPosition = Vector3.zero;
            Camera cam = Camera.main;
            if (cam == null || eventData == null)
            {
                return false;
            }

            Ray ray = cam.ScreenPointToRay(eventData.position);
            worldPosition = Physics.Raycast(ray, out RaycastHit hit) ? hit.point : Vector3.zero;
            return true;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!IsInteractable)
            {
                return;
            }

            OnDragBegin?.Invoke(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!IsInteractable)
            {
                return;
            }

            OnDragMoved?.Invoke(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!IsInteractable)
            {
                return;
            }

            try
            {
                TryProcessDrop(eventData);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DragHandler] TryProcessDrop failed: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                OnDragEnd?.Invoke(eventData);
            }
        }

        private void TryProcessDrop(PointerEventData eventData)
        {
            Camera cam = Camera.main;
            if (cam == null || BuildPayload == null)
            {
                return;
            }

            Vector2 screenPos = eventData.position;
            Ray ray = cam.ScreenPointToRay(screenPos);
            Vector3 worldPos = Physics.Raycast(ray, out RaycastHit hit) ? hit.point : Vector3.zero;
            DragPayload payload = BuildPayload(worldPos);

            IDragTarget target = DragTargetFinder.Find(payload, screenPos, cam);
            if (target != null)
            {
                target.OnDrop(payload);
            }
            else
            {
                Debug.Log($"<color=#ff5555>[DragHandler]</color> Drop missed — no accepting IDragTarget under pointer.");
            }
        }

        public void OnDropAccepted(IDragTarget by)
        {
            OnDragAccepted?.Invoke(by);
        }

        public void OnDropRejected()
        {
        }
    }
}
