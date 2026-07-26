using System;
using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Serialization;

namespace Scaffold
{
    /// <summary>
    /// Sets a float parameter on an Animator component to control a Unity animation.
    /// </summary>
    [CommandInfo("Animation",
                 "Set Anim Float",
                 "Sets a float parameter on an Animator component to control a Unity animation")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class SetAnimFloat : ActionBase
    {
        [Tooltip("Reference to an Animator component in a game object")]
        [SerializeField] protected AnimatorData animator;

        [Tooltip("Name of the float Animator parameter that will have its value changed")]
        [SerializeField] protected StringData parameterName;

        [Tooltip("The float value to set the parameter to")]
        [SerializeField] protected FloatData value;

        #region Public members

        public override void OnEnter()
        {
            if (animator.Value != null)
            {
                animator.Value.SetFloat(parameterName.Value, value.Value);
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
            return animator.animatorRef == variable || parameterName.stringRef == variable || value.floatRef == variable ||
                base.HasReference(variable);
        }

        #endregion

    }
}