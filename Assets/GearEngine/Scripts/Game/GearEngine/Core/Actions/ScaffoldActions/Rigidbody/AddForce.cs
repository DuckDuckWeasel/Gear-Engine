using System;
using GearEngine.Core.Actions;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("Rigidbody",
                 "Add Force",
                 "Adds a force to a 3D Rigidbody.")]
    [Serializable]
    public class AddForce : ActionBase
    {
        [Tooltip("The Rigidbody to apply the force to.")]
        [SerializeField] protected RigidbodyData rb;

        [Tooltip("The mode of the force to apply.")]
        [SerializeField] protected ForceMode forceMode = ForceMode.Force;

        public enum ForceFunction
        {
            AddForce,
            AddForceAtPosition,
            AddRelativeForce
        }

        [Tooltip("How the force should be applied.")]
        [SerializeField] protected ForceFunction forceFunction = ForceFunction.AddForce;

        [Tooltip("The vector of force to be added.")]
        [SerializeField] protected Vector3Data force;

        [Tooltip("Scale factor to be applied to the force.")]
        [SerializeField] protected FloatData forceScaleFactor = new FloatData(1);

        [Tooltip("World position the force is applied from. Used only in AddForceAtPosition.")]
        [SerializeField] protected Vector3Data atPosition;

        public override void OnEnter()
        {
            if (rb.Value != null)
            {
                Vector3 scaledForce = force.Value * forceScaleFactor.Value;

                switch (forceFunction)
                {
                    case ForceFunction.AddForce:
                        rb.Value.AddForce(scaledForce, forceMode);
                        break;
                    case ForceFunction.AddForceAtPosition:
                        rb.Value.AddForceAtPosition(scaledForce, atPosition.Value, forceMode);
                        break;
                    case ForceFunction.AddRelativeForce:
                        rb.Value.AddRelativeForce(scaledForce, forceMode);
                        break;
                }
            }

            Continue();
        }

        public override string GetSummary()
        {
            return forceMode.ToString() + ": " + force.Value.ToString();
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return rb.rigidbodyRef == variable || force.vector3Ref == variable || 
                   forceScaleFactor.floatRef == variable || atPosition.vector3Ref == variable;
        }
    }
}
