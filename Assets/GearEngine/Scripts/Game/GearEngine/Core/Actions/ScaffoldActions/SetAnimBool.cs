using System;
using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Serialization;

namespace Scaffold
{
    /// <summary>
    /// Sets a boolean parameter on an Animator component to control a Unity animation"
    /// </summary>
    [CommandInfo("Animation",
                 "Set Anim Bool",
                 "Sets a boolean parameter on an Animator component to control a Unity animation")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class SetAnimBool : ActionBase
    {
        [Tooltip("Reference to an Animator component in a game object")]
        [SerializeField] protected AnimatorData animator;

        [Tooltip("Name of the boolean Animator parameter that will have its value changed")]
        [SerializeField] protected StringData parameterName;

        [Tooltip("The boolean value to set the parameter to")]
        [SerializeField] protected BooleanData value;

        #region Public members

        public override void OnEnter()
        {
            if (animator.Value != null)
            {
                animator.Value.SetBool(parameterName.Value, value.Value);
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
            return animator.animatorRef == variable || parameterName.stringRef == variable || value.booleanRef == variable ||
                base.HasReference(variable);
        }

        #endregion

    }
}