using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GearEngine.GearEngine.Presentation.UI
{
    public static class DragTargetFinder
    {
        public static IDragTarget Find(DragPayload payload, Vector2 screenPos)
        {
            if (EventSystem.current == null)
            {
                return null;
            }

            List<RaycastResult> results = new List<RaycastResult>();
            PointerEventData ped = new PointerEventData(EventSystem.current) { position = screenPos };
            EventSystem.current.RaycastAll(ped, results);
            return FindFirstAccepting(payload, results);
        }

        internal static IDragTarget FindFirstAccepting(DragPayload payload, IReadOnlyList<RaycastResult> results)
        {
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
    }
}
