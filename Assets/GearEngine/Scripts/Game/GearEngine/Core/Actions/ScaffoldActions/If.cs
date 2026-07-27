using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// If the test expression is true, execute the following command block.
    /// </summary>
    [CommandInfo("Conditional",
                 "If",
                 "If the test expression is true, execute the following command block.")]
    [Serializable]
    public class If : VariableCondition
    {
    }
}
