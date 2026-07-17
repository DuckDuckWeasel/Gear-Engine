using GearEngine.Core.Actions;
using System;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("Transform", "Pulse Scale", "Instantly scales up and then smoothly returns to original scale. Great for UI or impacts.")]
    [Serializable]
    [AddComponentMenu("")]
    public class PulseScale : ActionBase
    {
        [Tooltip("The GameObject to pulse")]
        [SerializeField] protected GameObjectData targetGameObject;
        
        [Tooltip("The scale multiplier to punch to")]
        [SerializeField] protected FloatData punchMultiplier = new FloatData(1.5f);

        [Tooltip("Time to return to normal")]
        [SerializeField] protected FloatData duration = new FloatData(0.2f);
        
        [Tooltip("Wait until finished?")]
        [SerializeField] protected bool waitUntilFinished = true;

        public override void OnEnter()
        {
            if (targetGameObject.Value != null)
            {
                Vector3 originalScale = targetGameObject.Value.transform.localScale;
                Vector3 punchScale = originalScale * punchMultiplier.Value;
                
                targetGameObject.Value.transform.localScale = punchScale;
                
                LeanTween.scale(targetGameObject.Value, originalScale, duration.Value).setEaseOutQuad().setOnComplete(() =>
                {
                    if (targetGameObject.Value != null) targetGameObject.Value.transform.localScale = originalScale;
                    if (waitUntilFinished) Continue();
                });

                if (!waitUntilFinished)
                {
                    Continue();
                }
            }
            else
            {
                Continue();
            }
        }

        public override string GetSummary()
        {
            if (targetGameObject.Value == null) return "Error: No target";
            return $"Pulse {targetGameObject.Value.name} x{punchMultiplier.Value}";
        }
        
        public override Color GetButtonColor() { return new Color32(228, 237, 204, 255); }
    }
}
