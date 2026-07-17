using System;
using GearEngine.Core.Actions;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("Transform",
                 "Set Position",
                 "Instantly sets the position of a transform.")]
    [Serializable]
    public class SetPosition : ActionBase
    {
        [Tooltip("The Transform to position. If left empty, uses the GameObject this Block is attached to.")]
        [SerializeField] protected TransformData targetTransform;

        [Tooltip("The position to set the Transform to.")]
        [SerializeField] protected Vector3Data position;

        [Tooltip("If true, sets the local position instead of world position.")]
        [SerializeField] protected bool isLocal = false;

        public override void OnEnter()
        {
            Transform t = (targetTransform.Value != null) ? targetTransform.Value : GetFlowchart().transform;
            if (t != null)
            {
                if (isLocal)
                    t.localPosition = position.Value;
                else
                    t.position = position.Value;
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (targetTransform.Value == null)
            {
                return "Owner to " + position.Value.ToString();
            }
            return targetTransform.Value.name + " to " + position.Value.ToString();
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return targetTransform.transformRef == variable || position.vector3Ref == variable;
        }
    }
}
