using GearEngine.Core.Actions;
using System;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("Transform", "Squash And Stretch", "Animates the scale of a Transform using an AnimationCurve. Perfect for bouncing/squashing effects.")]
    [Serializable]
    [AddComponentMenu("")]
    public class SquashAndStretch : ActionBase
    {
        [Tooltip("The GameObject to animate")]
        [SerializeField] protected GameObjectData targetGameObject;
        
        [Tooltip("The scale animation curve. Should usually start at 1, go up/down, and end at 1.")]
        [SerializeField] protected AnimationCurve curve = AnimationCurve.Linear(0, 1, 1, 1);
        
        [Tooltip("The duration of the animation")]
        [SerializeField] protected FloatData duration = new FloatData(0.5f);
        
        [Tooltip("Maximum scale multiplier applied by the curve")]
        [SerializeField] protected FloatData scaleMultiplier = new FloatData(2f);

        [Tooltip("Which axis to apply the curve to")]
        [SerializeField] protected Vector3Data axisMask = new Vector3Data(Vector3.one);

        [Tooltip("Wait until animation finishes?")]
        [SerializeField] protected bool waitUntilFinished = true;

        public override void OnEnter()
        {
            if (targetGameObject.Value != null)
            {
                Vector3 originalScale = targetGameObject.Value.transform.localScale;
                
                LTDescr tween = LeanTween.value(targetGameObject.Value, 0f, 1f, duration.Value).setOnUpdate((float val) =>
                {
                    if (targetGameObject.Value == null) return;
                    
                    float curveVal = curve.Evaluate(val);
                    float modifiedScale = curveVal * scaleMultiplier.Value;
                    
                    Vector3 newScale = originalScale;
                    if (axisMask.Value.x > 0) newScale.x *= modifiedScale;
                    if (axisMask.Value.y > 0) newScale.y *= modifiedScale;
                    if (axisMask.Value.z > 0) newScale.z *= modifiedScale;
                    
                    targetGameObject.Value.transform.localScale = newScale;
                }).setOnComplete(() =>
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
            return $"Squash {targetGameObject.Value.name} for {duration.Value}s";
        }
        
        public override Color GetButtonColor() { return new Color32(228, 237, 204, 255); }
    }
}
