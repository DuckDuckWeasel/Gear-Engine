using System;
using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Serialization;

namespace Scaffold
{
    /// <summary>
    /// Resets a trigger parameter on an Animator component.
    /// </summary>
    [CommandInfo("Animation", 
                 "Reset Anim Trigger", 
                 "Resets a trigger parameter on an Animator component.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class ResetAnimTrigger : ActionBase
    {
        [Tooltip("Reference to an Animator component in a game object")]
        [SerializeField] protected AnimatorData animator;

        [Tooltip("Name of the trigger Animator parameter that will be reset")]
        [SerializeField] protected StringData parameterName;

        #region Public members

        public override void OnEnter()
        {
            if (animator.Value != null)
            {
                animator.Value.ResetTrigger(parameterName.Value);
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (animator.Value == null)
            {
                return "Error: No animator selected";
            }

            return animator.Value.name + " (" + parameterName.Value + ")";
        }

        public override Color GetButtonColor()
        {
            return new Color32(170, 204, 169, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return animator.animatorRef == variable || parameterName.stringRef == variable ||
                base.HasReference(variable);
        }

        #endregion

        #region Backwards compatibility

        [HideInInspector] [FormerlySerializedAs("animator")] public Animator animatorOLD;
        [HideInInspector] [FormerlySerializedAs("parameterName")] public string parameterNameOLD = "";

        protected virtual void OnEnable()
        {
            if (animatorOLD != null)
            {
                animator.Value = animatorOLD;
                animatorOLD = null;
            }

            if (parameterNameOLD != "")
            {
                parameterName.Value = parameterNameOLD;
                parameterNameOLD = "";
            }
        }

        #endregion
    }
}