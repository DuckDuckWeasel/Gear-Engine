using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Marks the start of a command block to be executed when the preceding If statement is False and the test expression is true.
    /// </summary>
    [CommandInfo("Lua",
                 "Lua Else If",
                 "Marks the start of a command block to be executed when the preceding If statement is False and the test expression is true.")]
    [Serializable]
    public class LuaElseIf : LuaCondition
    {
        protected override bool IsElseIf { get { return true; } }

        #region Public members

        public override bool CloseBlock()
        {
            return true;
        }

        #endregion
    }
}
