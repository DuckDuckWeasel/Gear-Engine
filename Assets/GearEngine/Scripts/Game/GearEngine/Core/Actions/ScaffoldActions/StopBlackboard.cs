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

        [Tooltip("Runtime instance IDs of additional Blackboards to stop")]
        [SerializeField] protected List<string> targetRuntimeInstanceIds = new List<string>();

        #region Public members

        public override void OnEnter()
        {
            VisualScripting.Blackboard blackboard = GetBlackboard();

            for (int i = 0; i < targetRuntimeInstanceIds.Count; i++)
            {
                string value = targetRuntimeInstanceIds[i];
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                Scaffold.VisualScripting.BlackboardRuntimeInstanceId runtimeId =
                    new Scaffold.VisualScripting.BlackboardRuntimeInstanceId(value);
                if (Context.Registry.TryGet(
                        runtimeId,
                        out Scaffold.VisualScripting.IBlackboardHandle handle) &&
                    handle is Scaffold.VisualScripting.Blackboard target)
                {
                    target.StopAll();
                }
            }

            //current block and command logic doesn't require it in this order but it makes sense to
            // stop everything but yourself first
            if (stopParentBlackboard)
            {
                blackboard.StopAll();
            }

            //you might not be stopping this blackboard so keep going
            Continue();
        }

        public override bool IsReorderableArray(string propertyName)
        {
            if (propertyName == "targetRuntimeInstanceIds")
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
