using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Resets the state of all commands and variables in the Flowchart.
    /// </summary>
    [CommandInfo("Variable", 
                 "Reset", 
                 "Resets the state of all commands and variables in the Flowchart.")]
    [Serializable]
    public class Reset : ActionBase
    {   
        [Tooltip("Reset state of all commands in the script")]
        [SerializeField] protected bool resetCommands = true;

        [Tooltip("Reset variables back to their default values")]
        [SerializeField] protected bool resetVariables = true;

        #region Public members

        public override void OnEnter()
        {
            GetFlowchart().Reset(resetCommands, resetVariables);
            Continue();
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        #endregion
    }
}