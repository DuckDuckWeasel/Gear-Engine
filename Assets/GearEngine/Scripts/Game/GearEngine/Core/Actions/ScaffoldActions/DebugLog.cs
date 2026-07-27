using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Writes a log message to the debug console.
    /// </summary>
    [CommandInfo("Scripting",
                 "Debug Log",
                 "Writes a log message to the debug console.")]
    [Serializable]
    public class DebugLog : ActionBase
    {
        [Tooltip("Display type of debug log info")]
        [SerializeField] protected DebugLogType logType;

        [Tooltip("Text to write to the debug log. Supports variable substitution, e.g. {$Myvar}")]
        [SerializeField] protected StringDataMulti logMessage;

        #region Public members

        public override void OnEnter()
        {
            VisualScripting.Blackboard blackboard = GetBlackboard();
            string message = blackboard.Substitute(logMessage.Value);

            switch (logType)
            {
                case DebugLogType.Info:
                    Debug.Log(message);
                    break;
                case DebugLogType.Warning:
                    Debug.LogWarning(message);
                    break;
                case DebugLogType.Error:
                    Debug.LogError(message);
                    break;
            }

            Continue();
        }

        public override string GetSummary()
        {
            return logMessage.GetDescription();
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return logMessage.stringRef == variable || base.HasReference(variable);
        }

        #endregion

    }
}
