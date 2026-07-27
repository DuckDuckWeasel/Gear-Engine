using System;
using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// A single expression in a variable condition.
    /// </summary>
    [Serializable]
    public class ConditionExpression
    {
        [Tooltip("The comparison operator.")]
        [SerializeField] protected CompareOperator compareOperator;

        [Tooltip("The variable and comparison value.")]
        [SerializeField] protected AnyVariableAndDataPair anyVar;

        public ConditionExpression()
        {
        }

        public ConditionExpression(
            CompareOperator op,
            AnyVariableAndDataPair variablePair)
        {
            compareOperator = op;
            anyVar = variablePair;
        }

        public virtual AnyVariableAndDataPair AnyVar => anyVar;

        public virtual CompareOperator CompareOperator => compareOperator;
    }
}
