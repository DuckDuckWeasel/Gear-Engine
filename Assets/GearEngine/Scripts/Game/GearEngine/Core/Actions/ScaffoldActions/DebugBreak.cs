using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Writes a log message to the debug console.
    /// </summary>
    [CommandInfo("Scripting",
                 "Debug Break",
                 "Calls Debug.Break if enabled. Also useful for putting a visual studio breakbpoint within.")]
    [Serializable]
    public class DebugBreak : ActionBase
    {
        [SerializeField] new protected BooleanData enabled = new BooleanData(true);

        public override void OnEnter()
        {
            if (enabled.Value)
                Debug.Break();

            Continue();
        }

        public override string GetSummary()
        {
            return enabled.Value ? "enabled" : "disabled";
        }

        public override bool HasReference(Variable variable)
        {
            return variable == enabled.booleanRef;
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }
    }
}