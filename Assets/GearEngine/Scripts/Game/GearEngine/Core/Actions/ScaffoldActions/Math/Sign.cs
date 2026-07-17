using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Command to execute and store the result of a Sign
    /// </summary>
    [CommandInfo("Math",
                 "Sign",
                 "Command to execute and store the result of a Sign")]
    [Serializable]
    public class Sign : BaseUnaryMathCommand
    {
        public override void OnEnter()
        {
            outValue.Value = Mathf.Sign(inValue.Value);

            Continue();
        }
    }
}