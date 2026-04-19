using UnityEngine;
using UnityEngine.EventSystems;

namespace GearEngine.GearEngine.Presentation.UI
{
    /// <summary>
    /// Resolves a world-space point for drag payloads from UI or world raycasts (no board-root coupling).
    /// </summary>
    public static class DragPointerUtility
    {
        private const float MaxRayDistance = 500f;

        public static Vector3 GetWorldPosition(PointerEventData e)
        {
            Camera cam = Camera.main;
            if (cam == null || e == null)
            {
                return Vector3.zero;
            }

            Ray ray = cam.ScreenPointToRay(e.position);
            if (Physics.Raycast(ray, out RaycastHit hit3d, MaxRayDistance))
            {
                return hit3d.point;
            }

            RaycastHit2D hit2d = Physics2D.GetRayIntersection(ray, MaxRayDistance);
            if (hit2d.collider != null)
            {
                return hit2d.point;
            }

            Plane plane = new Plane(Vector3.forward, Vector3.zero);
            if (plane.Raycast(ray, out float enter))
            {
                return ray.GetPoint(enter);
            }

            return Vector3.zero;
        }
    }
}
