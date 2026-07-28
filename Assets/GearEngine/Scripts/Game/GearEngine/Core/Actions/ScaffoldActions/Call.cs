using System;
using System.Collections.Generic;
using GearEngine.Core.Actions;
using UnityEngine;
using UnityEngine.Serialization;
using CoreActionExecutionStatus =
    Scaffold.VisualScripting.ActionExecutionStatus;
using CoreBlackboard = Scaffold.VisualScripting.Blackboard;
using CoreBlock = Scaffold.VisualScripting.Block;

namespace Scaffold
{
    [CommandInfo(
        "Flow",
        "Call",
        "Execute another Block in the current or a registered Blackboard runtime.")]
    [Serializable]
    public class Call : ActionBase, Scaffold.VisualScripting.IBlockConnectionSource
    {
        [Tooltip(
            "Optional runtime instance ID. Leave empty to use the current Blackboard.")]
        [SerializeField] private string targetRuntimeInstanceId = string.Empty;

        [FormerlySerializedAs("targetSequence")]
        [Tooltip("Name of the Block to execute")]
        [SerializeField] private StringData targetBlockName = new StringData();

        [Tooltip("Label to start execution at. Takes priority over startIndex.")]
        [SerializeField] private StringData startLabel = new StringData();

        [Tooltip("Action index to start executing")]
        [FormerlySerializedAs("commandIndex")]
        [SerializeField] private int startIndex;

        [Tooltip(
            "Select whether the caller stops, continues, or waits for the called Block.")]
        [SerializeField] private CallMode callMode;

        public override void OnEnter()
        {
            if (!TryResolveTarget(out CoreBlackboard blackboard, out CoreBlock block))
            {
                Debug.LogError(
                    $"[Call] Block '{targetBlockName.Value}' could not be resolved.");
                Fail();
                return;
            }

            if (ReferenceEquals(ParentBlock, block))
            {
                Continue(ResolveStartIndex(block));
                return;
            }

            if (block.IsExecuting())
            {
                Debug.LogWarning(
                    $"[Call] Block '{block.BlockName}' is already executing.");
                Continue();
                return;
            }

            Action<CoreActionExecutionStatus> completion =
                callMode == CallMode.WaitUntilFinished
                    ? _ => Continue()
                    : null;
            bool executed = blackboard.ExecuteBlock(
                block,
                ResolveStartIndex(block),
                completion);
            if (!executed)
            {
                Fail();
                return;
            }

            CompleteCaller();
        }

        public override string GetSummary()
        {
            string blockName = string.IsNullOrWhiteSpace(targetBlockName.Value)
                ? "<None>"
                : targetBlockName.Value;
            return $"{blockName} : {callMode}";
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        public void GetConnectedBlockNames(ICollection<string> blockNames)
        {
            if (blockNames == null)
            {
                throw new ArgumentNullException(nameof(blockNames));
            }

            if (!string.IsNullOrWhiteSpace(targetBlockName.Value))
            {
                blockNames.Add(targetBlockName.Value);
            }
        }

        public override bool HasReference(Variable variable)
        {
            return targetBlockName.stringRef == variable ||
                startLabel.stringRef == variable ||
                base.HasReference(variable);
        }

        private bool TryResolveTarget(
            out CoreBlackboard blackboard,
            out CoreBlock block)
        {
            blackboard = ResolveBlackboard();
            block = blackboard?.FindBlock(targetBlockName.Value);
            return blackboard != null && block != null;
        }

        private CoreBlackboard ResolveBlackboard()
        {
            if (string.IsNullOrWhiteSpace(targetRuntimeInstanceId))
            {
                return GetBlackboard();
            }

            Scaffold.VisualScripting.BlackboardRuntimeInstanceId runtimeId =
                new Scaffold.VisualScripting.BlackboardRuntimeInstanceId(
                    targetRuntimeInstanceId);
            return Context.Registry.TryGet(
                    runtimeId,
                    out Scaffold.VisualScripting.IBlackboardHandle handle)
                ? handle as CoreBlackboard
                : null;
        }

        private int ResolveStartIndex(CoreBlock block)
        {
            if (string.IsNullOrWhiteSpace(startLabel.Value) ||
                block.Definition.Tracks.Count == 0)
            {
                return Math.Max(startIndex, 0);
            }

            List<VisualScripting.IAction> actions =
                block.Definition.Tracks[0].ActionList.Actions;
            for (int index = 0; index < actions.Count; index++)
            {
                if (actions[index] is Label label &&
                    label.Key == startLabel.Value)
                {
                    return index + 1;
                }
            }

            return Math.Max(startIndex, 0);
        }

        private void CompleteCaller()
        {
            if (callMode == CallMode.WaitUntilFinished)
            {
                return;
            }

            if (callMode == CallMode.Stop ||
                callMode == CallMode.StopThenCall)
            {
                StopParentBlock();
                return;
            }

            Continue();
        }
    }
}
