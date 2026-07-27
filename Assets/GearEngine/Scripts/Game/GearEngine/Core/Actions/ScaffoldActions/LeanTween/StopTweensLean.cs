using System;
using GearEngine.Core.Actions;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Scaffold
{
    /// <summary>
    /// 
    /// </summary> 
    [CommandInfo("LeanTween",
                 "StopTweens",
                 "Stops the LeanTweens on a target GameObject")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class StopTweensLean : ActionBase
    {
        [Tooltip("Target game object stop LeanTweens on")]
        [SerializeField]
        protected GameObjectData targetObject;

        public override void OnEnter()
        {
            if (targetObject.Value != null)
            {
                LeanTween.cancel(targetObject.Value);
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (targetObject.Value == null)
            {
                return "Error: No target object selected";
            }

            return "Stop all LeanTweens on " + targetObject.Value.name;
        }

        public override Color GetButtonColor()
        {
            return new Color32(233, 163, 180, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return targetObject.gameObjectRef == variable;
        }
    }
}