using System;
using GearEngine.Core.Actions;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Decrease the ScaffoldPriority count, causing the related ScaffoldPrioritySignals to fire.
    /// Intended to be used to notify external systems that scaffold is doing something important and they should perhaps resume.
    /// </summary>
    [CommandInfo("PrioritySignals",
                 "Priority Down",
                 "Decrease the ScaffoldPriority count, causing the related ScaffoldPrioritySignals to fire. " +
                "Intended to be used to notify external systems that scaffold is doing something important and they should perhaps resume.")]
    [Serializable]
    public class ScaffoldPriorityDecrease : ActionBase
    {
        public override void OnEnter()
        {
            ScaffoldPrioritySignals.DoDecreasePriorityDepth();

            Continue();
        }
    }
}