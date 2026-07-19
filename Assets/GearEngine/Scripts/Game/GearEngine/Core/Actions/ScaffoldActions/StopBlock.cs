using System;
using GearEngine.Core.Actions;

﻿using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Stops executing the named Block.
    /// </summary>
    [CommandInfo("Flow", 
                 "Stop Block", 
                 "Stops executing the named Block")]
    [Serializable]
    public class StopBlock : ActionBase, IBlockCaller
    {
        [Tooltip("Blackboard containing the Block. If none is specified, the parent Blackboard is used.")]
        [SerializeField] protected Blackboard blackboard;

        [Tooltip("Name of the Block to stop")]
        [SerializeField] protected StringData blockName = new StringData("");

        #region Public members

        public override void OnEnter()
        {
            if (blockName.Value == "")
            {
                Continue();
            }

            if (blackboard == null)
            {
                blackboard = (Blackboard)GetBlackboard();
            }

            var block = blackboard.FindBlock(blockName.Value);
            if (block == null ||
                !block.IsExecuting())
            {
                Continue();
            }

            block.Stop();

            Continue();
        }

        public override string GetSummary()
        {
            return blockName;
        }
            
        public override Color GetButtonColor()
        {
            return new Color32(253, 253, 150, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return blockName.stringRef == variable || base.HasReference(variable);
        }

        public bool MayCallBlock(Block block)
        {
            if(blackboard != null)
                return block == blackboard.FindBlock(blockName.Value);
            return false;
        }

        #endregion
    }
}