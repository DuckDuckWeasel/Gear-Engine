using GearEngine.Core.Actions;
using System;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("Transform", "Look At", "Makes a Transform instantly look at a target position or object.")]
    [Serializable]
    [AddComponentMenu("")]
    public class LookAt : ActionBase
    {
        [Tooltip("The GameObject to rotate")]
        [SerializeField] protected GameObjectData targetGameObject;
        
        [Tooltip("The GameObject to look at. Overrides position if set.")]
        [SerializeField] protected GameObjectData targetToLookAt;

        [Tooltip("The position to look at (if targetToLookAt is null)")]
        [SerializeField] protected Vector3Data targetPosition;

        [Tooltip("The up vector, usually Vector3.up")]
        [SerializeField] protected Vector3Data upVector = new Vector3Data(Vector3.up);

        public override void OnEnter()
        {
            if (targetGameObject.Value != null)
            {
                Vector3 lookPos = targetToLookAt.Value != null ? targetToLookAt.Value.transform.position : targetPosition.Value;
                targetGameObject.Value.transform.LookAt(lookPos, upVector.Value);
            }
            Continue();
        }

        public override string GetSummary()
        {
            if (targetGameObject.Value == null) return "Error: No target";
            return $"Look at {(targetToLookAt.Value != null ? targetToLookAt.Value.name : "Position")}";
        }
        
        public override Color GetButtonColor() { return new Color32(228, 237, 204, 255); }
    }
}
