using System;
using GearEngine.Core.Actions;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Copy the value of the Priority Count to a local IntegerVariable, intended primarily to assist with debugging use of Priority.
    /// </summary>
    [CommandInfo("PrioritySignals",
                 "Get Priority Count",
                 "Copy the value of the Priority Count to a local IntegerVariable, intended primarily to assist with debugging use of Priority.")]
    [Serializable]
    public class ScaffoldPriorityCount : ActionBase
    {
        [VariableProperty(typeof(IntegerVariable))]
        [Tooltip("The Out var")]
        public IntegerVariable outVar;

        public override void OnEnter()
        {
            outVar.Value = ScaffoldPrioritySignals.CurrentPriorityDepth;

            Continue();
        }

        public override string GetSummary()
        {
            if (outVar == null)
            {
                return "Error: No out var supplied";
            }
            return outVar.Key;
        }

        public override bool HasReference(Variable variable)
        {
            return outVar == variable;
        }
    }
}