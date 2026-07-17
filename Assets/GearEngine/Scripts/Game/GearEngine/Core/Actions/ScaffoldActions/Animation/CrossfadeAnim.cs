using GearEngine.Core.Actions;
using System;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("Animation", "Crossfade Anim", "Smoothly transitions to an Animator state instead of snapping to it.")]
    [Serializable]
    [AddComponentMenu("")]
    public class CrossfadeAnim : ActionBase
    {
        [Tooltip("The Animator component")]
        [SerializeField] protected AnimatorData targetAnimator;
        
        [Tooltip("Name of the state to transition to")]
        [SerializeField] protected StringData stateName;

        [Tooltip("Duration of the crossfade transition in seconds")]
        [SerializeField] protected FloatData transitionDuration = new FloatData(0.25f);

        [Tooltip("The layer index to perform the crossfade on (usually 0)")]
        [SerializeField] protected int layer = 0;

        public override void OnEnter()
        {
            if (targetAnimator.Value != null)
            {
                targetAnimator.Value.CrossFade(stateName.Value, transitionDuration.Value, layer);
            }
            Continue();
        }

        public override string GetSummary()
        {
            if (targetAnimator.Value == null) return "Error: No Animator";
            return $"Crossfade to {stateName.Value}";
        }
        
        public override Color GetButtonColor() { return new Color32(170, 204, 169, 255); }
    }
}
