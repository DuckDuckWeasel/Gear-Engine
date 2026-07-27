using System;
using GearEngine.Core.Actions;

using UnityEngine;
using System.Collections.Generic;

namespace Scaffold
{
    /// <summary>
    /// Sets a Boolean, Integer, Float or String variable to a new value using a simple arithmetic operation. The value can be a constant or reference another variable of the same type.
    /// </summary>
    [CommandInfo("Variable",
                 "Set Variable",
                 "Sets a Boolean, Integer, Float or String variable to a new value using a simple arithmetic operation. The value can be a constant or reference another variable of the same type.")]
    [Serializable]
    public class SetVariable : ActionBase
    {
        [Tooltip("The Any var")]
        [SerializeField] protected AnyVariableAndDataPair anyVar = new AnyVariableAndDataPair();

        [Tooltip("The type of math operation to be performed")]
        [SerializeField] protected SetOperator setOperator;

        protected virtual void DoSetOperation()
        {
            if (anyVar.variable == null)
            {
                return;
            }

            anyVar.SetOp(setOperator);
        }

        #region Public members

        /// <summary>
        /// The type of math operation to be performed.
        /// </summary>
        public virtual SetOperator _SetOperator { get { return setOperator; } }

        public override void OnEnter()
        {
            DoSetOperation();

            Continue();
        }

        public override string GetSummary()
        {
            if (anyVar.variable == null)
            {
                return "Error: Variable not selected";
            }

            string description = anyVar.variable.Key;
            description += " " + VariableUtil.GetSetOperatorDescription(setOperator) + " ";
            description += anyVar.GetDataDescription();


            return description;
        }

        public override bool HasReference(Variable variable)
        {
            return anyVar.HasReference(variable);
        }

        public override Color GetButtonColor()
        {
            return new Color32(253, 253, 150, 255);
        }

        #endregion



        #region Editor caches
#if UNITY_EDITOR
        protected override void RefreshVariableCache()
        {
            base.RefreshVariableCache();

            if (anyVar != null)
            {
                anyVar.RefreshVariableCacheHelper(GetBlackboard(), ref referencedVariables);
            }
        }
#endif
        #endregion Editor caches

    }
}
