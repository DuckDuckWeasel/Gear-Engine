using System;
using GearEngine.Core.Actions;
using UnityEngine;

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

            for (int i = 0; i < CurrentActions.Count; i++)
            {
                Label label = CurrentActions[i] as Label;
                if (label != null && label.Key == targetLabel.Value)
                {
                    Continue(i + 1);
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

    }
}
