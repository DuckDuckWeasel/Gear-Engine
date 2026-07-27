using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Command to execute and store the result of a Exp
    /// </summary>
    [CommandInfo("Math",
                 "Exp",
                 "Command to execute and store the result of a Exp")]
    [Serializable]
    public class Exp : BaseUnaryMathCommand
    {
        public override void OnEnter()
        {
            outValue.Value = Mathf.Exp(inValue.Value);

            Continue();
        }
    }
}