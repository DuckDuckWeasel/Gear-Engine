using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;
using System;

namespace Scaffold
{
    [CommandInfo("Flow",
                 "Call",
                 "Execute another block in the same Blackboard as the command, or in a different Blackboard.")]
    [Serializable]
    public class Call : ActionBase, IBlockCaller
    {
        [Tooltip("Blackboard which contains the block to execute. If none is specified then the current Blackboard is used.")]
        [SerializeField] protected Blackboard targetBlackboard;

        [FormerlySerializedAs("targetSequence")]
        [Tooltip("Block to start executing")]
        [SerializeField] protected Block targetBlock;

        [Tooltip("Label to start execution at. Takes priority over startIndex.")]
        [SerializeField] protected StringData startLabel = new StringData();

        [Tooltip("Command index to start executing")]
        [FormerlySerializedAs("commandIndex")]
        [SerializeField] protected int startIndex;

        [Tooltip("Select if the calling block should stop or continue executing commands, or wait until the called block finishes.")]
        [SerializeField] protected CallMode callMode;

        #region Public members

        public override void OnEnter()
        {
            if (targetBlock != null && !TryCallTarget())
            {
                return;
            }

            CompleteCaller();
        }

        private bool TryCallTarget()
        {
            if (IsSelfCall())
            {
                Continue(0);
                return false;
            }
            if (IsTargetRunning())
            {
                Continue();
                return false;
            }

            ExecuteTarget(ResolveStartIndex(), CreateCompletion());
            return true;
        }

        private bool IsSelfCall()
        {
            return ParentBlock != null && ParentBlock.Equals(targetBlock);
        }

        private bool IsTargetRunning()
        {
            if (!targetBlock.IsExecuting())
            {
                return false;
            }

            Debug.LogWarning(targetBlock.BlockName + " cannot be called/executed, it is already running.");
            return true;
        }

        private int ResolveStartIndex()
        {
            if (startLabel.Value == "")
            {
                return startIndex;
            }

            int labelIndex = targetBlock.GetLabelIndex(startLabel.Value);
            return labelIndex == -1 ? startIndex : labelIndex;
        }

        private Action CreateCompletion()
        {
            return callMode == CallMode.WaitUntilFinished ? () => Continue() : null;
        }

        private void ExecuteTarget(int index, Action completion)
        {
            StopBeforeCall();
            if (IsLocalTarget())
            {
                bool detached = callMode != CallMode.WaitUntilFinished;
                RunRoutine(targetBlock.Execute(index, completion), detached);
                return;
            }

            targetBlackboard.ExecuteBlock(targetBlock, index, completion);
        }

        private void StopBeforeCall()
        {
            if (callMode == CallMode.StopThenCall)
            {
                StopParentBlock();
            }
        }

        private bool IsLocalTarget()
        {
            return targetBlackboard == null || targetBlackboard.Equals(GetBlackboard());
        }

        private void CompleteCaller()
        {
            if (callMode == CallMode.Stop)
            {
                StopParentBlock();
            }
            if (callMode == CallMode.Continue)
            {
                Continue();
            }
        }

        public override void GetConnectedBlocks(ref List<Block> connectedBlocks)
        {
            if (targetBlock != null)
            {
                connectedBlocks.Add(targetBlock);
            }
        }

        public override string GetSummary()
        {
            string summary = "";

            if (targetBlock == null)
            {
                summary = "<None>";
            }
            else
            {
                summary = targetBlock.BlockName;
            }

            summary += " : " + callMode.ToString();

            return summary;
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return startLabel.stringRef == variable || base.HasReference(variable);
        }

        public bool MayCallBlock(Block block)
        {
            return block == targetBlock;
        }

        #endregion
    }
}
