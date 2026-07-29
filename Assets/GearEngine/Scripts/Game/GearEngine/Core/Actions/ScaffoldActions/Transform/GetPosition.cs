using System;
using GearEngine.Core.Actions;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("Transform",
                 "Get Position",
                 "Gets the position of a transform and stores it in a Vector3 variable.")]
    [Serializable]
    public class GetPosition : ActionBase
    {
        [Tooltip("The Transform to get the position from.")]
        [SerializeField] protected TransformData targetTransform;

        [Tooltip("If true, gets the local position instead of world position.")]
        [SerializeField] protected bool isLocal = false;

        [Tooltip("The Vector3 variable to store the position in.")]
        [VariableProperty(typeof(Vector3Variable))]
        [SerializeField] protected Vector3Variable outPosition;

        public override void OnEnter()
        {
            Transform t = targetTransform.Value;
            if (t == null || outPosition == null)
            {
                Debug.LogError("[GetPosition] A target Transform and output variable are required.");
                Fail();
                return;
            }

            outPosition.Value = isLocal ? t.localPosition : t.position;
            Continue();
        }

        public override string GetSummary()
        {
            if (outPosition == null)
            {
                return "Error: No out variable selected";
            }

            string tName = targetTransform.Value != null ? targetTransform.Value.name : "Missing Transform";
            return tName + " to " + outPosition.Key;
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return targetTransform.transformRef == variable || outPosition == variable;
        }
    }
}
