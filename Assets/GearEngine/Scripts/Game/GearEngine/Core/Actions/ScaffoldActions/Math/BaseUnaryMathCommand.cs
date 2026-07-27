using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Base class for all simple Unary
    /// </summary>
    [Serializable]
    public abstract class BaseUnaryMathCommand : ActionBase
    {
        [Tooltip("Value to be passed in to the function.")]
        [SerializeField]
        protected FloatData inValue;

        [Tooltip("Where the result of the function is stored.")]
        [SerializeField]
        protected FloatData outValue;
        
        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        public override string GetSummary()
        {
            return "in: " + (inValue.floatRef != null ? inValue.floatRef.Key : inValue.Value.ToString()) + 
                   ", out: " + (outValue.floatRef != null ? outValue.floatRef.Key : outValue.Value.ToString());
        }

        public override bool HasReference(Variable variable)
        {
            return variable == inValue.floatRef || variable == outValue.floatRef;
        }
    }
}