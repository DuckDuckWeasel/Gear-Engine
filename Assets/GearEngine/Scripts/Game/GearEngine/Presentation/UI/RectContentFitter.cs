using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI
{
    /// <summary>Scales the first child to fit this RectTransform using combined renderer bounds.</summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class RectContentFitter : MonoBehaviour
    {
        [SerializeField, Range(0.1f, 1f)]
        private float padding = 0.9f;

        private RectTransform rect;
        private int lastChildCount = -1;

        private void Awake()
        {
            rect = (RectTransform)transform;
        }

        private void OnEnable()
        {
            Refit();
        }

        private void OnRectTransformDimensionsChange()
        {
            Refit();
        }

        private void OnTransformChildrenChanged()
        {
            Refit();
        }

        private void LateUpdate()
        {
            if (transform.childCount != lastChildCount)
            {
                lastChildCount = transform.childCount;
                Refit();
            }
        }

        private void Refit()
        {
            if (rect == null)
            {
                rect = (RectTransform)transform;
            }

            if (transform.childCount == 0)
            {
                return;
            }

            Transform content = transform.GetChild(0);
            content.localScale = Vector3.one;
            Renderer[] rs = content.GetComponentsInChildren<Renderer>(true);
            if (rs.Length == 0)
            {
                return;
            }

            Bounds b = rs[0].bounds;
            for (int i = 1; i < rs.Length; i++)
            {
                b.Encapsulate(rs[i].bounds);
            }

            if (b.size.x <= Mathf.Epsilon || b.size.y <= Mathf.Epsilon)
            {
                return;
            }

            Vector2 target = new Vector2(
                Mathf.Abs(rect.rect.width * rect.lossyScale.x),
                Mathf.Abs(rect.rect.height * rect.lossyScale.y));
            if (target.x <= Mathf.Epsilon || target.y <= Mathf.Epsilon)
            {
                return;
            }

            float fit = Mathf.Min(target.x / b.size.x, target.y / b.size.y) * padding;
            content.localScale = Vector3.one * fit;
        }
    }
}
