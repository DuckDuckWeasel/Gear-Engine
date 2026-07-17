using System;
using GearEngine.Core.Actions;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scaffold
{
    // <summary>
    /// Store UnityEngine.Input.GetAxis in a variable
    /// </summary>
    [CommandInfo("Input",
                 "GetAxis",
                 "Store UnityEngine.Input.GetAxis in a variable")]
    [Serializable]
    public class GetAxis : ActionBase
    {
        [SerializeField]
        protected StringData axisName;

        [Tooltip("If true, calls GetAxisRaw instead of GetAxis")]
        [SerializeField]
        protected bool axisRaw = false;

        [Tooltip("Float to store the value of the GetAxis")]
        [SerializeField]
        protected FloatData outValue;

        public override void OnEnter()
        {
            if (axisRaw)
            {
                outValue.Value = UnityEngine.Input.GetAxisRaw(axisName.Value);
            }
            else
            {
                outValue.Value = UnityEngine.Input.GetAxis(axisName.Value);
            }

            Continue();
        }

        public override string GetSummary()
        {
            return axisName + (axisRaw ? " Raw" : "");
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        public override bool HasReference(Variable variable)
        {
            if (axisName.stringRef == variable || outValue.floatRef == variable)
                return true;

            return false;
        }

    }
}