using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Marks the start of a command block to be executed when the preceding If statement is False.
    /// </summary>
    [CommandInfo("Conditional",
                 "Else",
                 "Marks the start of a command block to be executed when the preceding If statement is False.")]
    [Serializable]
    public class Else : ActionBase
    {
        #region Public members

        public override void OnEnter()
        {
            // Find the next End command at the same indent level as this Else command
            End matchingEnd = Condition.FindMatchingEndCommand(this);
            if (matchingEnd != null)
            {
                // Execute command immediately after the EndIf command
                Continue(matchingEnd.CommandIndex + 1);
            }
            else
            {
                // No End command found
                StopParentBlock();
            }
        }

        public override bool OpenBlock()
        {
            return true;
        }

        public override bool CloseBlock()
        {
            return true;
        }

        public override Color GetButtonColor()
        {
            return new Color32(253, 253, 150, 255);
        }

        #endregion
    }
}
