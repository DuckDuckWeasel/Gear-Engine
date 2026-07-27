using System;
using GearEngine.Core.Actions;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("Transform",
                 "Get Rotation",
                 "Gets the rotation of a transform as Euler angles and stores it in a Vector3 variable.")]
    [Serializable]
    public class GetRotation : ActionBase
    {
        [Tooltip("The Transform to get the rotation from.")]
        [SerializeField] protected TransformData targetTransform;

        [Tooltip("If true, gets the local rotation instead of world rotation.")]
        [SerializeField] protected bool isLocal = false;

        [Tooltip("The Vector3 variable to store the Euler angles in.")]
        [VariableProperty(typeof(Vector3Variable))]
        [SerializeField] protected Vector3Variable outRotation;

        public override void OnEnter()
        {
            Transform t = targetTransform.Value;
            if (t == null || outRotation == null)
            {
                Debug.LogError("[GetRotation] A target Transform and output variable are required.");
                Fail();
                return;
            }

            outRotation.Value = isLocal ? t.localEulerAngles : t.eulerAngles;
            Continue();
        }

        public override string GetSummary()
        {
            if (outRotation == null)
            {
                return "Error: No out variable selected";
            }

            string tName = targetTransform.Value != null ? targetTransform.Value.name : "Missing Transform";
            return tName + " to " + outRotation.Key;
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return targetTransform.transformRef == variable || outRotation == variable;
        }
    }
}
