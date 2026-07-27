using System;
using GearEngine.Core.Actions;

using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Scaffold
{
    [Serializable]
    public abstract class VariableCondition : Condition
    {
        public enum AnyOrAll
        {
            AnyOf_OR,//Use as a chain of ORs
            AllOf_AND,//Use as a chain of ANDs
        }

        [Tooltip("Selecting AnyOf will result in true if at least one of the conditions is true. Selecting AllOF will result in true only when all the conditions are true.")]
        [SerializeField] protected AnyOrAll anyOrAllConditions;

        [SerializeField] protected List<ConditionExpression> conditions = new List<ConditionExpression>();

        /// <summary>
        /// Called when the script is loaded or a value is changed in the
        /// inspector (Called in the editor only).
        /// </summary>
        public override void OnValidate()
        {
            base.OnValidate();

            if (conditions == null)
            {
                conditions = new List<ConditionExpression>();
            }

            if (conditions.Count == 0)
            {
                conditions.Add(new ConditionExpression());
            }
        }

        protected override bool EvaluateCondition()
        {
            if (conditions == null || conditions.Count == 0)
            {
                return false;
            }

            bool resultAny = false, resultAll = true;
            foreach (ConditionExpression condition in conditions)
            {
                bool curResult = false;
                if (condition.AnyVar == null)
                {
                    resultAll &= curResult;
                    resultAny |= curResult;
                    continue;
                }
                condition.AnyVar.Compare(condition.CompareOperator, ref curResult);
                resultAll &= curResult;
                resultAny |= curResult;
            }

            if (anyOrAllConditions == AnyOrAll.AnyOf_OR)
            {
                return resultAny;
            }

            return resultAll;
        }

        protected override bool HasNeededProperties()
        {
            if (conditions == null || conditions.Count == 0)
            {
                return false;
            }

            foreach (ConditionExpression condition in conditions)
            {
                if (condition.AnyVar == null || condition.AnyVar.variable == null)
                {
                    return false;
                }
            }
            return true;
        }

        public override string GetSummary()
        {
            if (!this.HasNeededProperties())
            {
                return "Error: No variable selected";
            }

            string connector = "";
            if (anyOrAllConditions == AnyOrAll.AnyOf_OR)
            {
                connector = " <b>OR</b> ";
            }
            else
            {
                connector = " <b>AND</b> ";
            }

            StringBuilder summary = new StringBuilder("");
            for (int i = 0; i < conditions.Count; i++)
            {
                summary.Append(conditions[i].AnyVar.variable.Key + " " +
                               VariableUtil.GetCompareOperatorDescription(conditions[i].CompareOperator) + " " +
                               conditions[i].AnyVar.GetDataDescription());

                if (i < conditions.Count - 1)
                {
                    summary.Append(connector);
                }
            }
            return summary.ToString();
        }

        public override bool HasReference(Variable variable)
        {
            if (conditions != null)
            {
                foreach (ConditionExpression condition in conditions)
                {
                    if (condition.AnyVar.HasReference(variable))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
