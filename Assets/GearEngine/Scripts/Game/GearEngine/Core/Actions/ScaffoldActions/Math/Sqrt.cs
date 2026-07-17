using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Command to execute and store the result of a Sqrt
    /// </summary>
    [CommandInfo("Math",
                 "Sqrt",
                 "Command to execute and store the result of a Sqrt")]
    [Serializable]
    public class Sqrt : BaseUnaryMathCommand
    {
        public override void OnEnter()
        {
            outValue.Value = Mathf.Sqrt(inValue.Value);

            Continue();
        }
    }
}