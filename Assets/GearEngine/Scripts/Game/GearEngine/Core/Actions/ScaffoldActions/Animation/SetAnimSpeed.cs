using GearEngine.Core.Actions;
using System;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("Animation", "Set Anim Speed", "Modifies the playback speed multiplier of an Animator.")]
    [Serializable]
    [AddComponentMenu("")]
    public class SetAnimSpeed : ActionBase
    {
        [Tooltip("The Animator to modify")]
        [SerializeField] protected AnimatorData targetAnimator;
        
        [Tooltip("The speed multiplier (1 is normal, 0 is paused, 2 is double speed)")]
        [SerializeField] protected FloatData speed = new FloatData(1f);

        public override void OnEnter()
        {
            if (targetAnimator.Value != null)
            {
                targetAnimator.Value.speed = speed.Value;
            }
            Continue();
        }

        public override string GetSummary()
        {
            if (targetAnimator.Value == null) return "Error: No Animator";
            return $"Set {targetAnimator.Value.name} speed to {speed.Value}";
        }
        
        public override Color GetButtonColor() { return new Color32(170, 204, 169, 255); }
    }
}
