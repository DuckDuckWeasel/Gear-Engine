using System;
using GearEngine.Core.Actions;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scaffold
{
    // <summary>
    /// Stop velocity and angular velocity on a Rigidbody2D
    /// </summary>
    [CommandInfo("Rigidbody2D",
                 "StopMotion2D",
                 "Stop velocity and angular velocity on a Rigidbody2D")]
    [Serializable]
    public class StopMotionRigidBody2D : ActionBase
    {
        [SerializeField]
        protected Rigidbody2DData rb;

        public enum Motion
        {
            Velocity,
            AngularVelocity,
            AngularAndLinearVelocity
        }

        [SerializeField]
        protected Motion motionToStop = Motion.AngularAndLinearVelocity;

        public override void OnEnter()
        {
            switch (motionToStop)
            {
                case Motion.Velocity:
                    rb.Value.linearVelocity = Vector2.zero;
                    break;
                case Motion.AngularVelocity:
                    rb.Value.angularVelocity = 0;
                    break;
                case Motion.AngularAndLinearVelocity:
                    rb.Value.angularVelocity = 0;
                    rb.Value.linearVelocity = Vector2.zero;
                    break;
                default:
                    break;
            }

            Continue();
        }

        public override string GetSummary()
        {
            return motionToStop.ToString();
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        public override bool HasReference(Variable variable)
        {
            if (rb.rigidbody2DRef == variable)
                return true;

            return false;
        }

    }
}