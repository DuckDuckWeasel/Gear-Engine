using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.Campaign.Presentation
{
    /// <summary>Preallocated UI-only reward effect; no particle or object creation during a race.</summary>
    public sealed class ClubSportStarView : MonoBehaviour
    {
        [SerializeField] private Image fill;
        [SerializeField] private Image glow;
        [SerializeField] private RectTransform[] sparks = System.Array.Empty<RectTransform>();
        private Sequence reward;

        internal float FillAmount => fill.fillAmount;
        internal void SetFill(float value) => fill.fillAmount = Mathf.Clamp01(value);

        internal void Celebrate(float delay, bool reducedMotion)
        {
            ResetEffect();
            if (reducedMotion || !Application.isPlaying)
            {
                return;
            }
            reward = DOTween.Sequence().SetTarget(this).SetDelay(delay);
            reward.Append(transform.DOScale(1.4f, .18f).SetEase(Ease.OutCubic));
            reward.Append(transform.DOScale(1f, .46f).SetEase(Ease.OutBack));
            reward.Insert(0f, Fade(glow, .8f, .12f));
            reward.Insert(.12f, Fade(glow, 0f, .6f));
            for (int i = 0; i < sparks.Length; i++)
            {
                RectTransform spark = sparks[i];
                float angle = i * Mathf.PI * 2f / sparks.Length;
                Vector2 direction = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                spark.anchoredPosition = direction * 6f;
                Image image = spark.GetComponent<Image>();
                reward.Insert(0f, Fade(image, 1f, .08f));
                reward.Insert(.08f, Fade(image, 0f, .5f));
                reward.Insert(0f, DOTween.To(() => spark.anchoredPosition, value => spark.anchoredPosition = value, direction * 22f, .6f).SetEase(Ease.OutCubic));
            }
        }

        private static Tween Fade(Image image, float alpha, float duration)
        {
            return DOTween.To(() => image.color.a, value =>
            {
                Color color = image.color;
                color.a = value;
                image.color = color;
            }, alpha, duration);
        }

        internal void ResetEffect()
        {
            reward?.Kill();
            reward = null;
            transform.localScale = Vector3.one;
            if (glow != null)
            {
                Color color = glow.color;
                color.a = 0f;
                glow.color = color;
            }
            foreach (RectTransform spark in sparks)
            {
                Color color = spark.GetComponent<Image>().color;
                color.a = 0f;
                spark.GetComponent<Image>().color = color;
            }
        }

        private void OnDisable() => ResetEffect();
    }
}
