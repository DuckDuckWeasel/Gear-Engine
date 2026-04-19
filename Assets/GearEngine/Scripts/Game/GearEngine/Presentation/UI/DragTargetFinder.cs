using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GearEngine.GearEngine.Presentation.UI
{
    /// <summary>
    /// Resolves the top-most accepting <see cref="IDragTarget"/> under the pointer (UI first, then world 2D/3D).
    /// </summary>
    public static class DragTargetFinder
    {
        private const float MaxRayDistance = 500f;

        public static IDragTarget Find(DragPayload payload, Vector2 screenPos, Camera cam)
        {
            return FindInUI(screenPos, payload) ?? FindInWorld(screenPos, cam, payload);
        }

        private static IDragTarget FindInUI(Vector2 screenPos, DragPayload payload)
        {
            if (EventSystem.current == null)
            {
                return null;
            }

            var results = new List<RaycastResult>();
            var ped = new PointerEventData(EventSystem.current) { position = screenPos };
            EventSystem.current.RaycastAll(ped, results);

            foreach (RaycastResult r in results)
            {
                IDragTarget t = r.gameObject.GetComponentInParent<IDragTarget>();
                if (t != null && t.CanAccept(payload))
                {
                    return t;
                }
            }

            return null;
        }

        private static IDragTarget FindInWorld(Vector2 screenPos, Camera cam, DragPayload payload)
        {
            if (cam == null)
            {
                return null;
            }

            Ray ray = cam.ScreenPointToRay(screenPos);

            RaycastHit2D hit2d = Physics2D.GetRayIntersection(ray, MaxRayDistance);
            if (hit2d.collider != null)
            {
                IDragTarget t = hit2d.collider.GetComponentInParent<IDragTarget>();
                if (t != null && t.CanAccept(payload))
                {
                    return t;
                }
            }

            if (Physics.Raycast(ray, out RaycastHit hit3d, MaxRayDistance))
            {
                IDragTarget t = hit3d.collider.GetComponentInParent<IDragTarget>();
                if (t != null && t.CanAccept(payload))
                {
                    return t;
                }
            }

            return null;
        }
    }
}
