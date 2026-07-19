using System;
using System.Collections.Generic;
using GearEngine.Core.Actions;
using GearEngine.GearEngine.Presentation.UI.Input;
using UnityEngine;

namespace Scaffold
{
    [Serializable]
    [CommandInfo("Flow", "Perform Interruption", "Stops selected running actions with a success or failure status.")]
    [AddComponentMenu("")]
    public sealed class PerformInterruption : ActionBase
    {
        [Tooltip("Optional Invoke Action to interrupt. Leave empty to use the current Invoke Action.")]
        [SerializeField] private InvokeActionCommand targetCommand;

        [Tooltip("Stable identifiers of the nested actions that should be interrupted.")]
        [SerializeField] private List<string> targetActionIds = new List<string>();

        [Tooltip("The status assigned to interrupted actions and returned by this action.")]
        [SerializeField] private BooleanData interruptSuccess = new BooleanData(true);

        public InvokeActionCommand TargetCommand
        {
            get => targetCommand;
            set => targetCommand = value;
        }

        public IList<string> TargetActionIds => targetActionIds;

        public bool InterruptSuccess
        {
            get => interruptSuccess.Value;
            set => interruptSuccess.Value = value;
        }

        public override void OnEnter()
        {
            InvokeActionCommand interruptionTarget = targetCommand;
            if (interruptionTarget == null)
            {
                interruptionTarget = hostCommand as InvokeActionCommand;
            }

            if (interruptionTarget == null)
            {
                Debug.LogError("[PerformInterruption] No Invoke Action target is available.");
                Fail();
                return;
            }

            ActionExecutionStatus interruptionStatus = interruptSuccess.Value
                ? ActionExecutionStatus.Success
                : ActionExecutionStatus.Failure;
            interruptionTarget.InterruptActions(targetActionIds, interruptionStatus);

            if (interruptSuccess.Value)
            {
                Continue();
                return;
            }

            Fail();
        }

        public override string GetSummary()
        {
            string targetName = targetCommand == null ? "Current Invoke Action" : targetCommand.name;
            return $"Interrupt {targetActionIds.Count} task(s) in {targetName}";
        }
    }
}
