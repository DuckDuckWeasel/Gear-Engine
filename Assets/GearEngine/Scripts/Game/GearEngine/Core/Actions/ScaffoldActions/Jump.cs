using System;
using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Serialization;

namespace Scaffold
{
    /// <summary>
    /// Move execution to a specific Label command in the same block.
    /// </summary>
    [CommandInfo("Flow", 
                 "Jump", 
                 "Move execution to a specific Label command in the same block")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class Jump : ActionBase
    {
        [Tooltip("Name of a label in this block to jump to")]
        [SerializeField] protected StringData targetLabel = new StringData("");

        #region Public members

        public override void OnEnter()
        {
            if (targetLabel.Value == "")
            {
                Continue();
                return;
            }

            var commandList = ParentBlock.CommandList;
            for (int i = 0; i < commandList.Count; i++)
            {
                var command = commandList[i];
                Label label = command as object as Label;
                if (label != null && label.Key == targetLabel.Value)
                {
                    Continue(label.CommandIndex + 1);
                    return;
                }
            }

            // Label not found
            Debug.LogWarning("Label not found: " + targetLabel.Value);
            Continue();
        }

        public override string GetSummary()
        {
            if (targetLabel.Value == "")
            {
                return "Error: No label selected";
            }

            return targetLabel.Value;
        }

        public override Color GetButtonColor()
        {
            return new Color32(253, 253, 150, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return targetLabel.stringRef == variable ||
                base.HasReference(variable);
        }

        #endregion

        #region Backwards compatibility

        [HideInInspector] [FormerlySerializedAs("targetLabel")] public Label targetLabelOLD;

        protected virtual void OnEnable()
        {
            if (targetLabelOLD != null)
            {
                targetLabel.Value = targetLabelOLD.Key;
                targetLabelOLD = null;
            }
        }

        #endregion
    }
}