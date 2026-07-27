using GearEngine.GearEngine.Visuals;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GearEngine.GearEngine.Presentation.UI
{
    internal static class DragPreview
    {
        public static GameObject Spawn(GameObject source, RectTransform parent)
        {
            if (source == null || parent == null)
            {
                return null;
            }

            RectTransform sourceRect = source.transform as RectTransform;
            GameObject clone = UnityEngine.Object.Instantiate(source, parent);
            clone.name = source.name + "_DragPreview";
            NormalizeRectTransform(clone.transform as RectTransform, sourceRect, parent);
            DisableInteractionRecursively(clone);
            return clone;
        }

        public static void MoveTo(GameObject preview, PointerEventData e)
        {
            if (preview == null)
            {
                return;
            }

            RectTransform rect = preview.transform as RectTransform;
            RectTransform parentRect = rect != null ? rect.parent as RectTransform : null;
            if (rect == null || parentRect == null)
            {
                return;
            }

            if (TryResolveLocalPoint(parentRect, e.position, out Vector2 localPoint))
            {
                rect.anchoredPosition = localPoint;
            }
        }

        private static bool TryResolveLocalPoint(RectTransform parentRect, Vector2 screenPosition, out Vector2 localPoint)
        {
            Canvas canvas = parentRect.GetComponentInParent<Canvas>();
            Camera eventCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPosition, eventCamera, out localPoint);
        }

        private static void NormalizeRectTransform(
            RectTransform previewRect,
            RectTransform sourceRect,
            RectTransform parent)
        {
            if (previewRect == null || sourceRect == null)
            {
                return;
            }

            previewRect.anchorMin = new Vector2(0.5f, 0.5f);
            previewRect.anchorMax = new Vector2(0.5f, 0.5f);
            previewRect.pivot = sourceRect.pivot;
            previewRect.sizeDelta = GetSizeInParentSpace(sourceRect, parent);
            previewRect.localScale = Vector3.one;
        }

        private static Vector2 GetSizeInParentSpace(
            RectTransform sourceRect,
            RectTransform parent)
        {
            Vector3 sourceScale = sourceRect.lossyScale;
            Vector3 parentScale = parent.lossyScale;
            float widthScale = SafeScaleRatio(sourceScale.x, parentScale.x);
            float heightScale = SafeScaleRatio(sourceScale.y, parentScale.y);
            return new Vector2(
                sourceRect.rect.width * widthScale,
                sourceRect.rect.height * heightScale);
        }

        private static float SafeScaleRatio(float sourceScale, float parentScale)
        {
            return Mathf.Abs(parentScale) > Mathf.Epsilon
                ? Mathf.Abs(sourceScale / parentScale)
                : 1f;
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
            DisableGearViews(go);
            DisableDraggables(go);
        }

        private static void DisableGearViews(GameObject go)
        {
            foreach (GearView gv in go.GetComponentsInChildren<GearView>(true))
            {
                gv.enabled = false;
            }
        }

        private static void DisableDraggables(GameObject go)
        {
            foreach (Draggable d in go.GetComponentsInChildren<Draggable>(true))
            {
                d.enabled = false;
            }
        }
    }
}
