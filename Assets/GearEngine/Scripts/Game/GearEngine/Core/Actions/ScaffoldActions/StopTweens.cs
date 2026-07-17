using System;
using GearEngine.Core.Actions;

﻿using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Stop all active iTweens in the current scene.
    /// </summary>
    [CommandInfo("iTween", 
                 "Stop Tweens", 
                 "Stop all active iTweens in the current scene.")]
    [Serializable]
    public class StopTweens : ActionBase
    {
        #region Public members

        public override void OnEnter()
        {
            iTween.Stop();
            Continue();
        }

        #endregion
    }
}