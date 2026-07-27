using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Map a value that exists in 1 range of numbers to another.
    /// </summary>
    [CommandInfo("Math",
                 "Map",
                 "Map a value that exists in 1 range of numbers to another.")]
    [Serializable]
    public class Map : ActionBase
    {
        //[Tooltip("LHS Value ")]
        [SerializeField]
        [Tooltip("The Initial range lower")]
        protected FloatData initialRangeLower = new FloatData(0), initialRangeUpper = new FloatData(1), value;

        [SerializeField]
        [Tooltip("The New range lower")]
        protected FloatData newRangeLower = new FloatData(0), newRangeUpper = new FloatData(1);

        [SerializeField]
        [Tooltip("The Out value")]
        protected FloatData outValue;

        public override void OnEnter()
        {
            float p = value.Value - initialRangeLower.Value;
            p /= initialRangeUpper.Value - initialRangeLower.Value;

            float res = p * (newRangeUpper.Value - newRangeLower.Value);
            res += newRangeLower.Value;

            outValue.Value = res;

            Continue();
        }

        public override string GetSummary()
        {
            return "Map [" + initialRangeLower.Value.ToString() + "-" + initialRangeUpper.Value.ToString() + "] to [" +
                newRangeLower.Value.ToString() + "-" + newRangeUpper.Value.ToString() + "]";
        }

        public override bool HasReference(Variable variable)
        {
            return initialRangeLower.floatRef == variable || initialRangeUpper.floatRef == variable || value.floatRef == variable ||
                   newRangeLower.floatRef == variable || newRangeUpper.floatRef == variable ||
                   outValue.floatRef == variable;
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }
    }
}