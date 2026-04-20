using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GearEngine.GearEngine.Presentation.UI
{
    public sealed class InventoryCollapseToggle : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private float collapsedHeight = 180f;
        [SerializeField] private float expandedHeight = 540f;
        [SerializeField] private float duration = 0.3f;
        [SerializeField] private Ease ease = Ease.OutCubic;

        private bool expanded;
        private Tween activeTween;

        private void OnEnable()
        {
            ApplyImmediate();
        }

        private void OnDisable()
        {
            KillActiveTween();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Toggle();
        }

        private void Toggle()
        {
            if (target == null)
            {
                return;
            }

            expanded = !expanded;
            KillActiveTween();
            float h = expanded ? expandedHeight : collapsedHeight;
            Vector2 endSize = new Vector2(target.sizeDelta.x, h);
            activeTween = DOTween.To(
                () => target.sizeDelta,
                size => target.sizeDelta = size,
                endSize,
                duration).SetEase(ease);
        }

        private void KillActiveTween()
        {
            if (activeTween != null && activeTween.IsActive())
            {
                activeTween.Kill();
            }
        }

        private void ApplyImmediate()
        {
            if (target == null)
            {
                return;
            }

            float h = expanded ? expandedHeight : collapsedHeight;
            target.sizeDelta = new Vector2(target.sizeDelta.x, h);
        }
    }
}
