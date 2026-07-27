
using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Boolean variable type.
    /// </summary>
    [VariableInfo("", "Boolean")]
    [AddComponentMenu("")]
    [System.Serializable]
    public class BooleanVariable : VariableBase<bool>
    {
        public override bool IsArithmeticSupported(SetOperator setOperator)
        {
            return setOperator == SetOperator.Negate || base.IsArithmeticSupported(setOperator);
        }

        public override void Apply(SetOperator op, bool value)
        {
            switch (op)
            {
            case SetOperator.Negate:
                Value = !value;
                break;
            default:
                base.Apply(op, value);
                break;
            }
        }
    }

    /// <summary>
    /// Container for a Boolean variable reference or constant value.
    /// </summary>
    [System.Serializable]
    public struct BooleanData
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(BooleanVariable))]
        public BooleanVariable booleanRef;


        [SerializeField]
        public VariableDataSource source;

        [SerializeField]
        public BooleanValueSO booleanSO;
        [SerializeField]
        public bool booleanVal;

        public BooleanData(bool v)
        {
            booleanVal = v;
            booleanRef = null;
            source = VariableDataSource.Unspecified;
            booleanSO = null;
        }

        public static implicit operator bool(BooleanData booleanData)
        {
            return booleanData.Value;
        }

        public bool Value
        {
            get { return VariableValueReference.Resolve(booleanRef, booleanVal, booleanSO, source); }
            set { VariableValueReference.Assign(booleanRef, ref booleanVal, booleanSO, source, value); }
        }

        public string GetDescription()
        {
            return VariableValueReference.Describe(booleanRef, booleanVal, booleanSO, source);
        }
    }
}