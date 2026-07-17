using GearEngine.Core.Actions;
using System;
using System.Collections;
using UnityEngine;
using TMPro;

namespace Scaffold
{
    [CommandInfo("UI", "Count To", "Animates a number counting up or down in a TextMeshPro UI text.")]
    [Serializable]
    [AddComponentMenu("")]
    public class CountTo : ActionBase
    {
        [Tooltip("The TMP Text component to modify")]
        [SerializeField] protected TextMeshProUGUI targetText;
        
        [Tooltip("The number to start from")]
        [SerializeField] protected FloatData startValue = new FloatData(0);
        
        [Tooltip("The number to count to")]
        [SerializeField] protected FloatData endValue = new FloatData(100);

        [Tooltip("Duration of the counting animation in seconds")]
        [SerializeField] protected FloatData duration = new FloatData(1f);
        
        [Tooltip("Wait until counting finishes?")]
        [SerializeField] protected bool waitUntilFinished = true;

        public override void OnEnter()
        {
            if (targetText != null && flowchart != null)
            {
                flowchart.StartCoroutine(CountRoutine());
            }
            else
            {
                Continue();
            }
        }

        private IEnumerator CountRoutine()
        {
            float elapsed = 0f;

            if (!waitUntilFinished)
            {
                Continue();
            }

            while (elapsed < duration.Value)
            {
                elapsed += Time.deltaTime;
                float currentVal = Mathf.Lerp(startValue.Value, endValue.Value, elapsed / duration.Value);
                targetText.text = Mathf.RoundToInt(currentVal).ToString();
                yield return null;
            }

            targetText.text = endValue.Value.ToString();

            if (waitUntilFinished)
            {
                Continue();
            }
        }

        public override string GetSummary()
        {
            if (targetText == null) return "Error: No Target Text";
            return $"Count to {endValue.Value} over {duration.Value}s";
        }
        
        public override Color GetButtonColor() { return new Color32(235, 191, 217, 255); }
    }
}
