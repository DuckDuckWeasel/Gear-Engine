using System;
using GearEngine.Core.Actions;

using UnityEngine;
using System.Collections.Generic;

namespace Scaffold
{
    /// <summary>
    /// Stops execution of all Blocks in a Blackboard.
    /// </summary>
    [CommandInfo("Flow", 
                 "Stop Blackboard", 
                 "Stops execution of all Blocks in a Blackboard")]
    [Serializable]
    public class StopBlackboard : ActionBase
    {       
        [Tooltip("Stop all executing Blocks in the Blackboard that contains this command")]
        [SerializeField] protected bool stopParentBlackboard;

        [Tooltip("Stop all executing Blocks in a list of target Blackboards")]
        [SerializeField] protected List<Blackboard> targetBlackboards = new List<Blackboard>();

        #region Public members

        public override void OnEnter()
        {
            var blackboard = GetBlackboard();

            for (int i = 0; i < targetBlackboards.Count; i++)
            {
                var f = targetBlackboards[i];
                f.StopAllBlocks();
            }

            //current block and command logic doesn't require it in this order but it makes sense to
            // stop everything but yourself first
            if (stopParentBlackboard)
            {
                blackboard.StopAllBlocks();
            }

            //you might not be stopping this blackboard so keep going
            Continue();
        }

        public override bool IsReorderableArray(string propertyName)
        {
            if (propertyName == "targetBlackboards")
            {
                return true;
            }

            return false;
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        #endregion
    }
}