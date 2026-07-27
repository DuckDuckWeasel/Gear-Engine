using GearEngine.Core.Actions;
using System;
using System.Collections;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("Transform", "Auto Rotate", "Makes a GameObject continuously rotate over time.")]
    [Serializable]
    [AddComponentMenu("")]
    public class AutoRotate : ActionBase
    {
        [Tooltip("The GameObject to rotate")]
        [SerializeField] protected GameObjectData targetGameObject;
        
        [Tooltip("Rotation speed (degrees per second)")]
        [SerializeField] protected Vector3Data rotationSpeed = new Vector3Data(new Vector3(0, 90f, 0));

        public override void OnEnter()
        {
            if (targetGameObject.Value != null && blackboard != null)
            {
                blackboard.StartCoroutine(RotateRoutine());
            }
            Continue(); // Always continue immediately
        }

        private IEnumerator RotateRoutine()
        {
            while (targetGameObject.Value != null)
            {
                targetGameObject.Value.transform.Rotate(rotationSpeed.Value * Time.deltaTime);
                yield return null;
            }
        }

        public override string GetSummary()
        {
            if (targetGameObject.Value == null) return "Error: No target";
            return $"AutoRotate {targetGameObject.Value.name}";
        }
        
        public override Color GetButtonColor() { return new Color32(228, 237, 204, 255); }
    }
}
