using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Raise a value to the power of another
    /// </summary>
    [CommandInfo("Math",
                 "Pow",
                 "Raise a value to the power of another.")]
    [Serializable]
    public class Pow : ActionBase
    {
        [SerializeField]
        protected FloatData baseValue, exponentValue;

        [Tooltip("Where the result of the function is stored.")]
        [SerializeField]
        protected FloatData outValue;

        public override void OnEnter()
        {
            outValue.Value = Mathf.Pow(baseValue.Value, exponentValue.Value);

            Continue();
        }

        public override string GetSummary()
        {
            if (outValue.floatRef == null)
                return "Error: No out value selected";

            return outValue.floatRef.Key + " = " + baseValue.Value.ToString() + "^" + exponentValue.Value.ToString();
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return baseValue.floatRef == variable || exponentValue.floatRef == variable ||
                   outValue.floatRef == variable;
        }
    }
}