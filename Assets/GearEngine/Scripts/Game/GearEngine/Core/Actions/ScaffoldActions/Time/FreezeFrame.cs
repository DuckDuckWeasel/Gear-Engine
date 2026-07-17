using GearEngine.Core.Actions;
using System;
using System.Collections;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("Time", "Freeze Frame", "Instantly changes time scale to a target value, waits for unscaled real time, then restores the original time scale. Great for Hit Stop effects.")]
    [Serializable]
    [AddComponentMenu("")]
    public class FreezeFrame : ActionBase
    {
        [Tooltip("The time scale to set during the freeze (usually 0 or very close to 0)")]
        [SerializeField] protected FloatData targetTimeScale = new FloatData(0f);
        
        [Tooltip("How long to stay frozen in real unscaled seconds")]
        [SerializeField] protected FloatData freezeDuration = new FloatData(0.1f);

        [Tooltip("Wait until freeze is over before continuing the block?")]
        [SerializeField] protected bool waitUntilFinished = true;

        public override void OnEnter()
        {
            if (flowchart != null)
            {
                flowchart.StartCoroutine(FreezeRoutine());
            }
            else
            {
                Continue(); // Fallback
            }
        }

        private IEnumerator FreezeRoutine()
        {
            float originalTimeScale = Time.timeScale;
            Time.timeScale = targetTimeScale.Value;
            
            if (!waitUntilFinished)
            {
                Continue();
            }

            yield return new WaitForSecondsRealtime(freezeDuration.Value);

            Time.timeScale = originalTimeScale;

            if (waitUntilFinished)
            {
                Continue();
            }
        }

        public override string GetSummary()
        {
            return $"Scale: {targetTimeScale.Value} for {freezeDuration.Value}s";
        }
        
        public override Color GetButtonColor() { return new Color32(216, 228, 240, 255); }
    }
}
