using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;
using System;

namespace Scaffold
{
    /// <summary>
    /// Supported modes for calling a block.
    /// </summary>
    public enum CallMode
    {
        /// <summary> Stop executing the current block after calling. </summary>
        Stop,
        /// <summary> Continue executing the current block after calling  </summary>
        Continue,
        /// <summary> Wait until the called block finishes executing, then continue executing current block. </summary>
        WaitUntilFinished,
        /// <summary> Stop executing the current block before attempting to call. This allows for circular calls within the same frame </summary>
        StopThenCall
    }

    /// <summary>
    /// Execute another block in the same Blackboard as the command, or in a different Blackboard.
    /// </summary>
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
            if (targetBlock != null)
            {
                // Check if calling your own parent block
                if (ParentBlock != null && ParentBlock.Equals(targetBlock))
                {
                    // Just ignore the callmode in this case, and jump to first command in list
                    Continue(0);
                    return;
                }

                if(targetBlock.IsExecuting())
                {
                    Debug.LogWarning(targetBlock.BlockName + " cannot be called/executed, it is already running.");
                    Continue();
                    return;
                }

                // Callback action for Wait Until Finished mode
                Action onComplete = null;
                if (callMode == CallMode.WaitUntilFinished)
                {
                    onComplete = delegate {
                        Continue();
                    };
                }

                // Find the command index to start execution at
                int index = startIndex;
                if (startLabel.Value != "")
                {
                    int labelIndex = targetBlock.GetLabelIndex(startLabel.Value);
                    if (labelIndex != -1)
                    {
                        index = labelIndex;
                    }
                }

                if (targetBlackboard == null ||
                    targetBlackboard.Equals(GetBlackboard()))
                {
                    if (callMode == CallMode.StopThenCall)
                    {
                        StopParentBlock();
                    }
                    host.StartCoroutine(targetBlock.Execute(index, onComplete));
                }
                else
                {
                    if (callMode == CallMode.StopThenCall)
                    {
                        StopParentBlock();
                    }
                    // Execute block in another Blackboard
                    targetBlackboard.ExecuteBlock(targetBlock, index, onComplete);
                }
            }

            if (callMode == CallMode.Stop)
            {
                StopParentBlock();
            }
            else if (callMode == CallMode.Continue)
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