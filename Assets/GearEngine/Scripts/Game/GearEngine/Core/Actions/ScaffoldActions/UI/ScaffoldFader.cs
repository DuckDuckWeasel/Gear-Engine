using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Scaffold.UI
{
    public enum ScaffoldFaderType { Alpha, Directional, Round }

    [RequireComponent(typeof(CanvasGroup))]
    public class ScaffoldFader : MonoBehaviour
    {
        public int faderID = 0;
        public ScaffoldFaderType faderType = ScaffoldFaderType.Alpha;
        
        [Header("References")]
        public CanvasGroup canvasGroup;
        public RectTransform maskRect;
        
        private Coroutine _fadeCoroutine;

        private void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        }

        public void FadeTo(float targetAlpha, float duration, Vector2 targetMaskScale = default)
        {
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeRoutine(targetAlpha, duration, targetMaskScale));
        }

        private IEnumerator FadeRoutine(float targetAlpha, float duration, Vector2 targetMaskScale)
        {
            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;
            
            Vector2 startScale = Vector2.one;
            if (maskRect != null) startScale = maskRect.localScale;
            
            canvasGroup.blocksRaycasts = true;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                
                if (maskRect != null && faderType != ScaffoldFaderType.Alpha)
                {
                    maskRect.localScale = Vector2.Lerp(startScale, targetMaskScale, t);
                }
                
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
            if (maskRect != null && faderType != ScaffoldFaderType.Alpha) maskRect.localScale = targetMaskScale;
            
            if (targetAlpha <= 0f) canvasGroup.blocksRaycasts = false;
        }
    }
}
