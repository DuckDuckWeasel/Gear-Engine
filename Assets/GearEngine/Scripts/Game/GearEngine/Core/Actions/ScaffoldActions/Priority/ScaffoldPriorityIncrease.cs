using System;
using GearEngine.Core.Actions;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Increases the ScaffoldPriority count, causing the related ScaffoldPrioritySignals to fire.
    /// Intended to be used to notify external systems that scaffold is doing something important and they should perhaps pause.
    /// </summary>
    [CommandInfo("PrioritySignals",
                 "Priority Up",
                 "Increases the ScaffoldPriority count, causing the related ScaffoldPrioritySignals to fire. " +
                "Intended to be used to notify external systems that scaffold is doing something important and they should perhaps pause.")]
    [Serializable]
    public class ScaffoldPriorityIncrease : ActionBase
    {
        public override void OnEnter()
        {
            ScaffoldPrioritySignals.DoIncreasePriorityDepth();

            Continue();
        }
    }
}