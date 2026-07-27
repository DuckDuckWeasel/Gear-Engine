using GearEngine.Core.Actions;
using System;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("Transform", "Rotate Around", "Instantly rotates a transform around a specific point and axis by a given angle.")]
    [Serializable]
    [AddComponentMenu("")]
    public class RotateAround : ActionBase
    {
        [Tooltip("The GameObject to rotate")]
        [SerializeField] protected GameObjectData targetGameObject;
        
        [Tooltip("The point to rotate around")]
        [SerializeField] protected Vector3Data point;

        [Tooltip("The axis to rotate around (e.g., Vector3.up)")]
        [SerializeField] protected Vector3Data axis = new Vector3Data(Vector3.up);

        [Tooltip("The angle in degrees")]
        [SerializeField] protected FloatData angle = new FloatData(90f);

        public override void OnEnter()
        {
            if (targetGameObject.Value != null)
            {
                targetGameObject.Value.transform.RotateAround(point.Value, axis.Value, angle.Value);
            }
            Continue();
        }

        public override string GetSummary()
        {
            if (targetGameObject.Value == null) return "Error: No target";
            return $"Rotate {targetGameObject.Value.name} {angle.Value}deg";
        }
        
        public override Color GetButtonColor() { return new Color32(228, 237, 204, 255); }
    }
}
