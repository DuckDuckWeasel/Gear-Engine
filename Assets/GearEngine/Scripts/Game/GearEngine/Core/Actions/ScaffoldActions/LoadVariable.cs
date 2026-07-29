using System;
using GearEngine.Core.Actions;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo(
        "Variable",
        "Load Variables",
        "Load the current Blackboard variable stores from a persistent slot.")]
    [Serializable]
    public class LoadVariable : ActionBase
    {
        [Tooltip(
            "Save slot. Supports Blackboard variable substitution.")]
        [SerializeField] private string key = string.Empty;

        public override void OnEnter()
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                Debug.LogError("[LoadVariable] A save slot is required.");
                Fail();
                return;
            }

            VisualScripting.Blackboard blackboard = GetBlackboard();
            string slot = BuildSlot(blackboard);
            RunTask(
                () => blackboard.LoadAsync(slot),
                $"Loading Blackboard slot '{slot}'");
        }

        public override string GetSummary()
        {
            return string.IsNullOrWhiteSpace(key)
                ? "Error: No save slot selected"
                : key;
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        private string BuildSlot(
            Scaffold.VisualScripting.Blackboard blackboard)
        {
            string slot = blackboard.Substitute(key);
            return string.IsNullOrWhiteSpace(blackboard.SaveProfile)
                ? slot
                : $"{blackboard.SaveProfile}_{slot}";
        }
    }
}
