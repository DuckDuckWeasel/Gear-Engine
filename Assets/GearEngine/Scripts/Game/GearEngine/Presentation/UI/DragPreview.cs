using GearEngine.GearEngine;
using GearEngine.GearEngine.Visuals;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GearEngine.GearEngine.Presentation.UI
{
    internal static class DragPreview
    {
        public static GameObject Spawn(GameObject source, Transform parent)
        {
            if (source == null)
            {
                return null;
            }

            GameObject clone = UnityEngine.Object.Instantiate(source, parent);
            clone.name = source.name + "_DragPreview";
            DisableInteractionRecursively(clone);
            return clone;
        }

        public static void MoveTo(GameObject preview, PointerEventData e)
        {
            if (preview == null)
            {
                return;
            }

            RectTransform rt = preview.transform as RectTransform;
            if (rt != null)
            {
                Camera cam = e.pressEventCamera;
                Canvas canvas = rt.GetComponentInParent<Canvas>();
                if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    cam = canvas.worldCamera;
                }

                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                        rt.parent as RectTransform,
                        e.position,
                        cam,
                        out Vector3 world))
                {
                    rt.position = world;
                }
            }
            else
            {
                Camera cam = e.pressEventCamera != null ? e.pressEventCamera : Camera.main;
                if (cam == null)
                {
                    return;
                }

                // Project the cursor onto the preview's parent plane so the preview
                // tracks the cursor in the same space the source lived in (e.g. the
                // board plane for board gears). Falling back to (cam.position.z) was
                // brittle: it required the camera to sit at (_, _, -depth) and the
                // parent to be at the world origin, neither of which holds when a
                // FrustumFitAnchor places the board off-center.
                Transform parent = preview.transform.parent;
                Vector3 planeOrigin = parent != null ? parent.position : preview.transform.position;
                Vector3 planeNormal = parent != null ? parent.forward : Vector3.forward;
                Plane plane = new Plane(planeNormal, planeOrigin);
                Ray ray = cam.ScreenPointToRay(e.position);
                if (plane.Raycast(ray, out float enter))
                {
                    preview.transform.position = ray.GetPoint(enter);
                }
            }
        }

        private static void DisableInteractionRecursively(GameObject go)
        {
            // Unity's overridden equality means `??` does not work with destroyed/missing components;
            // GetComponent can return a "fake null" sentinel that `??` treats as non-null.
            CanvasGroup cg = go.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = go.AddComponent<CanvasGroup>();
            }

            cg.blocksRaycasts = false;
            cg.interactable = false;
            foreach (Collider2D c in go.GetComponentsInChildren<Collider2D>(true))
            {
                c.enabled = false;
            }

            foreach (Collider c in go.GetComponentsInChildren<Collider>(true))
            {
                c.enabled = false;
            }

            // The preview is a static visual snapshot; logic components that drive its
            // transform (e.g. GearView's settle-to-slot lerp) would otherwise pull the
            // preview to the parent's origin instead of the cursor. Draggable on the
            // clone would let the preview itself start a nested drag.
            foreach (GearView gv in go.GetComponentsInChildren<GearView>(true))
            {
                gv.enabled = false;
            }

            foreach (Draggable d in go.GetComponentsInChildren<Draggable>(true))
            {
                d.enabled = false;
            }
        }
    }
}
