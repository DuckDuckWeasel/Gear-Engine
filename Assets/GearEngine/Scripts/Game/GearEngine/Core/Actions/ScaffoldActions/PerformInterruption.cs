using System;
using System.Collections.Generic;
using GearEngine.Core.Actions;
using UnityEngine;
using CoreActionExecutionStatus =
    Scaffold.VisualScripting.ActionExecutionStatus;

namespace Scaffold
{
    [Serializable]
    [CommandInfo("Flow", "Perform Interruption", "Stops selected running actions with a success or failure status.")]
    [AddComponentMenu("")]
    public sealed class PerformInterruption : ActionBase
    {
        [Tooltip("Stable identifiers of the nested actions that should be interrupted.")]
        [SerializeField] private List<string> targetActionIds = new List<string>();

        [Tooltip("The status assigned to interrupted actions and returned by this action.")]
        [SerializeField] private BooleanData interruptSuccess = new BooleanData(true);

        [Tooltip("The Target action ids")]
        public IList<string> TargetActionIds => targetActionIds;

        public bool InterruptSuccess
        {
            get => interruptSuccess.Value;
            set => interruptSuccess.Value = value;
        }

        public override void OnEnter()
        {
            List<int> targetIndexes = new List<int>();
            for (int index = 0; index < CurrentActions.Count; index++)
            {
                if (targetActionIds.Contains(
                    CurrentActions[index].DefinitionId.Value))
                {
                    targetIndexes.Add(index);
                }
            }

            if (targetIndexes.Count == 0)
            {
                Debug.LogError("[PerformInterruption] None of the configured action IDs exist in the current Action List.");
                Fail();
                return;
            }

            CoreActionExecutionStatus interruptionStatus = interruptSuccess.Value
                ? CoreActionExecutionStatus.Success
                : CoreActionExecutionStatus.Failure;
            Context.ActionList.InterruptActions(targetIndexes, interruptionStatus);

            if (interruptSuccess.Value)
            {
                Continue();
                return;
            }

            Fail();
        }

        public override string GetSummary()
        {
            return $"Interrupt {targetActionIds.Count} action(s) in the current Action List";
        }
    }
}
