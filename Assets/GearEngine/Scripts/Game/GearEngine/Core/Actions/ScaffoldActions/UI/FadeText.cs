using GearEngine.Core.Actions;
using System;
using System.Collections;
using UnityEngine;
using TMPro;

namespace Scaffold
{
    [CommandInfo("UI", "Fade Text", "Fades the alpha channel of a TextMeshPro UI element over time.")]
    [Serializable]
    [AddComponentMenu("")]
    public class FadeText : ActionBase
    {
        [Tooltip("The TMP Text component to modify")]
        [SerializeField] protected TextMeshProUGUI targetText;
        
        [Tooltip("The target alpha value (0 is transparent, 1 is opaque)")]
        [SerializeField] protected FloatData targetAlpha = new FloatData(0f);

        [Tooltip("Duration of the fade in seconds")]
        [SerializeField] protected FloatData duration = new FloatData(1f);
        
        [Tooltip("Wait until fade finishes?")]
        [SerializeField] protected bool waitUntilFinished = true;

        public override void OnEnter()
        {
            if (targetText != null && blackboard != null)
            {
                blackboard.StartCoroutine(FadeRoutine());
            }
            else
            {
                Continue();
            }
        }

        private IEnumerator FadeRoutine()
        {
            float elapsed = 0f;
            Color startColor = targetText.color;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha.Value);

            if (!waitUntilFinished)
            {
                Continue();
            }

            while (elapsed < duration.Value)
            {
                elapsed += Time.deltaTime;
                targetText.color = Color.Lerp(startColor, endColor, elapsed / duration.Value);
                yield return null;
            }

            targetText.color = endColor;

            if (waitUntilFinished)
            {
                Continue();
            }
        }

        public override string GetSummary()
        {
            if (targetText == null) return "Error: No Target Text";
            return $"Fade {targetText.name} to {targetAlpha.Value} over {duration.Value}s";
        }
        
        public override Color GetButtonColor() { return new Color32(235, 191, 217, 255); }
    }
}
