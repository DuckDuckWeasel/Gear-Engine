using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Continuously loop through a block of commands while the condition is true. Use the Break command to force the loop to terminate immediately.
    /// </summary>
    [CommandInfo("Conditional",
                 "While",
                 "Continuously loop through a block of commands while the condition is true. Use the Break command to force the loop to terminate immediately.")]
    [Serializable]
    public class While : If
    {
        #region Public members

        public override bool IsLooping { get { return true; } }

        #endregion
    }
}
