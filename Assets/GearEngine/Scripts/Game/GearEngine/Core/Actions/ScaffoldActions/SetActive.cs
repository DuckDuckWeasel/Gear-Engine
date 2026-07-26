using System;
using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Serialization;

namespace Scaffold
{
    /// <summary>
    /// Sets a game object in the scene to be active / inactive.
    /// </summary>
    [CommandInfo("Scripting",
                 "Set Active",
                 "Sets a game object in the scene to be active / inactive.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class SetActive : ActionBase
    {
        [Tooltip("Reference to game object to enable / disable")]
        [SerializeField] protected GameObjectData targetGameObject;

        [Tooltip("Set to true to enable the game object")]
        [SerializeField] protected BooleanData activeState;

        #region Public members

        public override void OnEnter()
        {
            if (targetGameObject.Value != null)
            {
                targetGameObject.Value.SetActive(activeState.Value);
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (targetGameObject.Value == null)
            {
                return "Error: No game object selected";
            }

            return targetGameObject.Value.name + " = " + activeState.GetDescription();
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return targetGameObject.gameObjectRef == variable || activeState.booleanRef == variable ||
                base.HasReference(variable);
        }

        #endregion

    }
}