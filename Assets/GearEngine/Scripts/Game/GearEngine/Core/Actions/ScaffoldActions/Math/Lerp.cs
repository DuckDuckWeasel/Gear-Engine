using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Linearly Interpolate from A to B
    /// </summary>
    [CommandInfo("Math",
                 "Lerp",
                 "Linearly Interpolate from A to B")]
    [Serializable]
    public class Lerp : ActionBase
    {
        public enum Mode
        {
            Lerp,
            LerpUnclamped,
            LerpAngle
        }

        [SerializeField]
        [Tooltip("The Mode")]
        protected Mode mode = Mode.Lerp;

        //[Tooltip("LHS Value ")]
        [SerializeField]
        [Tooltip("The A")]
        protected FloatData a = new FloatData(0), b = new FloatData(1), percentage;

        //[Tooltip("Where the result of the function is stored.")]
        [SerializeField]
        [Tooltip("The Out value")]
        protected FloatData outValue;

        public override void OnEnter()
        {
            switch (mode)
            {
                case Mode.Lerp:
                    outValue.Value = Mathf.Lerp(a.Value, b.Value, percentage.Value);
                    break;
                case Mode.LerpUnclamped:
                    outValue.Value = Mathf.LerpUnclamped(a.Value, b.Value, percentage.Value);
                    break;
                case Mode.LerpAngle:
                    outValue.Value = Mathf.LerpAngle(a.Value, b.Value, percentage.Value);
                    break;
                default:
                    break;
            }

            Continue();
        }

        public override string GetSummary()
        {
            return mode.ToString() + " [" + a.Value.ToString() + "-" + b.Value.ToString() + "]";
        }

        public override bool HasReference(Variable variable)
        {
            return a.floatRef == variable || b.floatRef == variable || percentage.floatRef == variable ||
                   outValue.floatRef == variable;
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

    }
}