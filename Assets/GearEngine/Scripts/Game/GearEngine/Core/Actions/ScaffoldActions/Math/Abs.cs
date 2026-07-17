using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Command to execute and store the result of a Abs
    /// </summary>
    [CommandInfo("Math",
                 "Abs",
                 "Command to execute and store the result of a Abs")]
    [Serializable]
    public class Abs : BaseUnaryMathCommand
    {
        public override void OnEnter()
        {
            outValue.Value = Mathf.Abs(inValue.Value);

            Continue();
        }
    }
}