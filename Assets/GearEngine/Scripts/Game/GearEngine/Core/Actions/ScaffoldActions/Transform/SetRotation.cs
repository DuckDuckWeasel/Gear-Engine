using System;
using GearEngine.Core.Actions;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("Transform",
                 "Set Rotation",
                 "Instantly sets the rotation of a transform using Euler angles.")]
    [Serializable]
    public class SetRotation : ActionBase
    {
        [Tooltip("The Transform to rotate.")]
        [SerializeField] protected TransformData targetTransform;

        [Tooltip("The rotation to set the Transform to, in Euler angles.")]
        [SerializeField] protected Vector3Data rotation;

        [Tooltip("If true, sets the local rotation instead of world rotation.")]
        [SerializeField] protected bool isLocal = false;

        public override void OnEnter()
        {
            Transform t = targetTransform.Value;
            if (t == null)
            {
                Debug.LogError("[SetRotation] A target Transform is required.");
                Fail();
                return;
            }

            if (isLocal)
            {
                t.localEulerAngles = rotation.Value;
            }
            else
            {
                t.eulerAngles = rotation.Value;
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (targetTransform.Value == null)
            {
                return "Error: No target Transform";
            }
            return targetTransform.Value.name + " to " + rotation.Value.ToString();
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return targetTransform.transformRef == variable || rotation.vector3Ref == variable;
        }
    }
}
