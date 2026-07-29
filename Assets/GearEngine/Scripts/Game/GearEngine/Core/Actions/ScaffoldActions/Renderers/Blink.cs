using GearEngine.Core.Actions;
using System;
using System.Collections;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("Renderers", "Blink", "Toggles a Renderer on and off rapidly for a duration. Good for invincibility frames.")]
    [Serializable]
    [AddComponentMenu("")]
    public class Blink : ActionBase
    {
        [Tooltip("The Renderer component to blink")]
        [SerializeField] protected Renderer targetRenderer;

        [Tooltip("Total duration of the blink effect")]
        [SerializeField] protected FloatData duration = new FloatData(1f);

        [Tooltip("How long it stays invisible each blink")]
        [SerializeField] protected FloatData offDuration = new FloatData(0.1f);

        [Tooltip("How long it stays visible each blink")]
        [SerializeField] protected FloatData onDuration = new FloatData(0.1f);

        [Tooltip("Wait until blink is done?")]
        [SerializeField] protected bool waitUntilFinished = true;

        public override void OnEnter()
        {
            if (targetRenderer != null && CanRunScheduledWork)
            {
                RunRoutine(BlinkRoutine(), !waitUntilFinished);
            }
            else
            {
                Continue();
            }
        }

        private IEnumerator BlinkRoutine()
        {
            float elapsed = 0f;
            bool isVisible = true;
            CompleteDetachedAction();
            while (elapsed < duration.Value)
            {
                isVisible = !isVisible;
                targetRenderer.enabled = isVisible;
                float waitTime = isVisible ? onDuration.Value : offDuration.Value;
                yield return new WaitForSeconds(waitTime);
                elapsed += waitTime;
            }

            targetRenderer.enabled = true;
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
            if (targetRenderer == null)
            {
                return "Error: No Renderer";
            }

            return $"Blink {targetRenderer.name} for {duration.Value}s";
        }

        public override Color GetButtonColor() { return new Color32(235, 191, 217, 255); }
    }
}
