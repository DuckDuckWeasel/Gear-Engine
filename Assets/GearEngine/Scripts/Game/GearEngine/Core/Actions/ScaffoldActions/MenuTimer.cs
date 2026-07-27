using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Displays a timer bar and executes a target block if the player fails to select a menu option in time.
    /// </summary>
    [CommandInfo("Narrative",
                 "Menu Timer",
                 "Displays a timer bar and executes a target block if the player fails to select a menu option in time.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class MenuTimer : ActionBase
    {
        [Tooltip("Length of time to display the timer for")]
        [SerializeField] protected FloatData duration = new FloatData(1);

        [Tooltip("Menu Dialog that displays the timer")]
        [SerializeField] private MenuDialog menuDialog;

        [Tooltip("Name of the Block to execute when the timer expires")]
        [SerializeField] private StringData targetBlockName = new StringData();

        #region Public members

        public override void OnEnter()
        {
            if (menuDialog != null &&
                !string.IsNullOrWhiteSpace(targetBlockName.Value))
            {
                VisualScripting.Blackboard blackboard = GetBlackboard();
                menuDialog.ShowTimer(
                    duration.Value,
                    () => blackboard.ExecuteBlock(targetBlockName.Value));
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (string.IsNullOrWhiteSpace(targetBlockName.Value))
            {
                return "Error: No target block selected";
            }

            return targetBlockName.Value;
        }

        public override Color GetButtonColor()
        {
            return new Color32(184, 210, 235, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return duration.floatRef == variable ||
                targetBlockName.stringRef == variable ||
                base.HasReference(variable);
        }

        #endregion

    }
}
