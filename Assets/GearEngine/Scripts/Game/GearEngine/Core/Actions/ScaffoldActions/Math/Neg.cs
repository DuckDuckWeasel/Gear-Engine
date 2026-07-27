using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Negate a float
    /// </summary>
    [CommandInfo("Math",
                 "Negate",
                 "Negate a float")]
    [Serializable]
    public class Neg : BaseUnaryMathCommand
    {
        public override void OnEnter()
        {
            outValue.Value = -(inValue.Value);

            Continue();
        }
    }
}