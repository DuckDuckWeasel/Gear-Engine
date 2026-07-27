using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Force a loop to terminate immediately.
    /// </summary>
    [CommandInfo("Flow",
                 "Break",
                 "Force a loop to terminate immediately.")]
    [Serializable]
    public class Break : ActionBase
    {
        #region Public members

        //located the containing loop and tell it to end
        public override void OnEnter()
        {
            Condition loopingCond = null;
            // Find index of previous looping command
            for (int i = CommandIndex - 1; i >= 0; --i)
            {
                Condition cond = CurrentActions[i] as Condition;
                if (cond != null && cond.IsLooping)
                {
                    loopingCond = cond;
                    break;
                }
            }

            if (loopingCond == null)
            {
                // No enclosing loop command found, just continue
                Debug.LogError("Break called but found no enclosing looping construct." + GetLocationIdentifier());
                Continue();
            }
            else
            {
                loopingCond.MoveToEnd();
            }
        }

        public override Color GetButtonColor()
        {
            return new Color32(253, 253, 150, 255);
        }

        #endregion
    }
}
