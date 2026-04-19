using GearEngine.GearEngine;
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
                Camera cam = Camera.main;
                if (cam == null)
                {
                    return;
                }

                Vector3 sp = new Vector3(e.position.x, e.position.y, Mathf.Abs(cam.transform.position.z));
                Vector3 w = cam.ScreenToWorldPoint(sp);
                w.z = preview.transform.position.z;
                preview.transform.position = w;
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
        }
    }
}
