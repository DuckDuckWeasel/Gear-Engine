using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Stops executing the named Block.
    /// </summary>
    [CommandInfo("Flow",
                 "Stop Block",
                 "Stops executing the named Block")]
    [Serializable]
    public class StopBlock : ActionBase
    {
        [Tooltip("Name of the Block to stop")]
        [SerializeField] protected StringData blockName = new StringData("");

        #region Public members

        public override void OnEnter()
        {
            if (blockName.Value == "")
            {
                Continue();
                return;
            }

            VisualScripting.Block block = GetBlackboard().FindBlock(blockName.Value);
            if (block == null ||
                !block.IsExecuting())
            {
                Continue();
                return;
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

        #endregion
    }
}
