using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI
{
    public static class BoardPointerProjectionUtility
    {
        public static bool TryProjectScreenPointToPlane(
            Camera camera,
            Vector2 screenPoint,
            Transform planeTransform,
            out Vector3 worldPoint)
        {
            worldPoint = Vector3.zero;
            if (camera == null || planeTransform == null)
            {
                return false;
            }

            Plane plane = new Plane(planeTransform.forward, planeTransform.position);
            Ray ray = camera.ScreenPointToRay(screenPoint);
            if (!plane.Raycast(ray, out float enter))
            {
                return false;
            }

            worldPoint = ray.GetPoint(enter);
            return true;
        }
    }
}
