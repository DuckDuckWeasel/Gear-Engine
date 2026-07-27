using GearEngine.Core.Actions;
using System;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("Transform", "Set Parent", "Changes the parent of a Transform.")]
    [Serializable]
    [AddComponentMenu("")]
    public class SetParent : ActionBase
    {
        [Tooltip("The GameObject whose parent we want to change")]
        [SerializeField] protected GameObjectData targetGameObject;
        
        [Tooltip("The new parent GameObject. Leave empty to unparent (move to root)")]
        [SerializeField] protected GameObjectData newParent;

        [Tooltip("If true, the object will maintain its world position, rotation and scale")]
        [SerializeField] protected bool worldPositionStays = true;

        public override void OnEnter()
        {
            if (targetGameObject.Value != null)
            {
                Transform p = newParent.Value != null ? newParent.Value.transform : null;
                targetGameObject.Value.transform.SetParent(p, worldPositionStays);
            }
            Continue();
        }

        public override string GetSummary()
        {
            if (targetGameObject.Value == null) return "Error: No target";
            return $"Set parent to {(newParent.Value != null ? newParent.Value.name : "None")}";
        }
        
        public override Color GetButtonColor() { return new Color32(228, 237, 204, 255); }
    }
}
