using System;
using GearEngine.Core.Actions;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("Rigidbody",
                 "Add Torque",
                 "Adds torque (rotation) to a 3D Rigidbody.")]
    [Serializable]
    public class AddTorque : ActionBase
    {
        [Tooltip("The Rigidbody to apply the torque to.")]
        [SerializeField] protected RigidbodyData rb;

        [Tooltip("The mode of the torque to apply.")]
        [SerializeField] protected ForceMode forceMode = ForceMode.Force;

        public enum TorqueFunction
        {
            AddTorque,
            AddRelativeTorque
        }

        [Tooltip("How the torque should be applied.")]
        [SerializeField] protected TorqueFunction torqueFunction = TorqueFunction.AddTorque;

        [Tooltip("The vector of torque to be added.")]
        [SerializeField] protected Vector3Data torque;

        [Tooltip("Scale factor to be applied to the torque.")]
        [SerializeField] protected FloatData torqueScaleFactor = new FloatData(1);

        public override void OnEnter()
        {
            if (rb.Value != null)
            {
                Vector3 scaledTorque = torque.Value * torqueScaleFactor.Value;

                switch (torqueFunction)
                {
                    case TorqueFunction.AddTorque:
                        rb.Value.AddTorque(scaledTorque, forceMode);
                        break;
                    case TorqueFunction.AddRelativeTorque:
                        rb.Value.AddRelativeTorque(scaledTorque, forceMode);
                        break;
                }
            }

            Continue();
        }

        public override string GetSummary()
        {
            return forceMode.ToString() + ": " + torque.Value.ToString();
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return rb.rigidbodyRef == variable || torque.vector3Ref == variable || torqueScaleFactor.floatRef == variable;
        }
    }
}
