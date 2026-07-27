using System;
using GearEngine.Core.Actions;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Resets the ScaffoldPriority count to zero. Useful if you are among logic that is hard to have matching increase and decreases.
    /// </summary>
    [CommandInfo("PrioritySignals",
                 "Priority Reset",
                 "Resets the ScaffoldPriority count to zero. Useful if you are among logic that is hard to have matching increase and decreases.")]
    [Serializable]
    public class ScaffoldPriorityReset : ActionBase
    {
        public override void OnEnter()
        {
            ScaffoldPrioritySignals.DoResetPriority();

            Continue();
        }
    }
}