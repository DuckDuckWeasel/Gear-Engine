using System;
using GearEngine.Core.Actions;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("Rigidbody",
                 "Set Velocity",
                 "Instantly sets the velocity of a 3D Rigidbody.")]
    [Serializable]
    public class SetVelocity : ActionBase
    {
        [Tooltip("The Rigidbody to set the velocity for.")]
        [SerializeField] protected RigidbodyData rb;

        [Tooltip("The velocity vector to set.")]
        [SerializeField] protected Vector3Data velocity;

        public override void OnEnter()
        {
            if (rb.Value != null)
            {
                rb.Value.linearVelocity = velocity.Value;
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (rb.Value == null) return "Error: Rigidbody not selected";
            return "Set Velocity: " + velocity.Value.ToString();
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return rb.rigidbodyRef == variable || velocity.vector3Ref == variable;
        }
    }
}
