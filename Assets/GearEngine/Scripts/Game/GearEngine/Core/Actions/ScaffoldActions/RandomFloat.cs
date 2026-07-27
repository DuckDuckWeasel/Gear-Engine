using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Sets an float variable to a random value in the defined range.
    /// </summary>
    [CommandInfo("Variable", 
                 "Random Float", 
                 "Sets an float variable to a random value in the defined range.")]
    [Serializable]
    public class RandomFloat : ActionBase 
    {
        [Tooltip("The variable whos value will be set")]
        [VariableProperty(typeof(FloatVariable))]
        [SerializeField] protected FloatVariable variable;

        [Tooltip("Minimum value for random range")]
        [SerializeField] protected FloatData minValue;

        [Tooltip("Maximum value for random range")]
        [SerializeField] protected FloatData maxValue;

        #region Public members

        public override void OnEnter()
        {
            if (variable != null)
            {
                variable.Value = UnityEngine.Random.Range(minValue.Value, maxValue.Value);
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (variable == null)
            {
                return "Error: Variable not selected";
            }

            return variable.Key;
        }

        public override bool HasReference(Variable variable)
        {
            return (variable == this.variable) || minValue.floatRef == variable || maxValue.floatRef == variable;
        }

        public override Color GetButtonColor()
        {
            return new Color32(253, 253, 150, 255);
        }

        #endregion
    }
}