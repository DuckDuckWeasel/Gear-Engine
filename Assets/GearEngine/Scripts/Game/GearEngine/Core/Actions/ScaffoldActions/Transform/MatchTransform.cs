using GearEngine.Core.Actions;
using System;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("Transform", "Match Transform", "Snaps a GameObject's position, rotation, and/or scale to match a target.")]
    [Serializable]
    [AddComponentMenu("")]
    public class MatchTransform : ActionBase
    {
        [Tooltip("The GameObject to move")]
        [SerializeField] protected GameObjectData targetGameObject;
        
        [Tooltip("The GameObject to match")]
        [SerializeField] protected GameObjectData objectToMatch;

        [SerializeField] protected bool matchPosition = true;
        [SerializeField] protected bool matchRotation = true;
        [SerializeField] protected bool matchScale = false;

        public override void OnEnter()
        {
            if (targetGameObject.Value != null && objectToMatch.Value != null)
            {
                if (matchPosition) targetGameObject.Value.transform.position = objectToMatch.Value.transform.position;
                if (matchRotation) targetGameObject.Value.transform.rotation = objectToMatch.Value.transform.rotation;
                if (matchScale) targetGameObject.Value.transform.localScale = objectToMatch.Value.transform.localScale;
            }
            Continue();
        }

        public override string GetSummary()
        {
            if (targetGameObject.Value == null || objectToMatch.Value == null) return "Error: Missing objects";
            return $"Match {targetGameObject.Value.name} to {objectToMatch.Value.name}";
        }
        
        public override Color GetButtonColor() { return new Color32(228, 237, 204, 255); }
    }
}
