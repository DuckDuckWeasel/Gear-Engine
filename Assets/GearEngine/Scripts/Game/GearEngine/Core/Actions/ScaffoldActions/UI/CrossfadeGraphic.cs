using GearEngine.Core.Actions;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Scaffold
{
    [CommandInfo("UI", "Crossfade Graphic", "Smoothly fades a UI Graphic's color over time.")]
    [Serializable]
    [AddComponentMenu("")]
    public class CrossfadeGraphic : ActionBase
    {
        [Tooltip("The UI Graphic component to modify")]
        [SerializeField] protected Graphic targetGraphic;

        [Tooltip("The target color")]
        [SerializeField] protected ColorData targetColor = new ColorData(Color.white);

        [Tooltip("Duration of the fade in seconds")]
        [SerializeField] protected FloatData duration = new FloatData(1f);

        [Tooltip("Wait until fade finishes?")]
        [SerializeField] protected bool waitUntilFinished = true;

        public override void OnEnter()
        {
            if (targetGraphic != null && CanRunScheduledWork)
            {
                RunRoutine(FadeRoutine(), !waitUntilFinished);
            }
            else
            {
                Continue();
            }
        }

        private IEnumerator FadeRoutine()
        {
            float elapsed = 0f;
            Color startColor = targetGraphic.color;
            Color endColor = targetColor.Value;
            CompleteDetachedAction();
            while (elapsed < duration.Value)
            {
                elapsed += CurrentDeltaTime;
                targetGraphic.color = Color.Lerp(startColor, endColor, elapsed / duration.Value);
                yield return null;
            }

            targetGraphic.color = endColor;
            CompleteAwaitedAction();
        }

        private void CompleteDetachedAction()
        {
            if (!waitUntilFinished)
            {
                Continue();
            }
        }

        private void CompleteAwaitedAction()
        {
            if (waitUntilFinished)
            {
                Continue();
            }
        }

        public override string GetSummary()
        {
            if (targetGraphic == null)
            {
                return "Error: No Target Graphic";
            }

            return $"Fade {targetGraphic.name} over {duration.Value}s";
        }

        public override Color GetButtonColor() { return new Color32(235, 191, 217, 255); }
    }
}
