using System;
using GearEngine.Core.Actions;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("Transform",
                 "Set Scale",
                 "Instantly sets the local scale of a transform.")]
    [Serializable]
    public class SetScale : ActionBase
    {
        [Tooltip("The Transform to scale.")]
        [SerializeField] protected TransformData targetTransform;

        [Tooltip("The scale to set the Transform to.")]
        [SerializeField] protected Vector3Data scale;

        public override void OnEnter()
        {
            Transform t = targetTransform.Value;
            if (t == null)
            {
                Debug.LogError("[SetScale] A target Transform is required.");
                Fail();
                return;
            }

            t.localScale = scale.Value;
            Continue();
        }

        public override string GetSummary()
        {
            if (targetTransform.Value == null)
            {
                return "Error: No target Transform";
            }
            return targetTransform.Value.name + " to " + scale.Value.ToString();
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return targetTransform.transformRef == variable || scale.vector3Ref == variable;
        }
    }
}
