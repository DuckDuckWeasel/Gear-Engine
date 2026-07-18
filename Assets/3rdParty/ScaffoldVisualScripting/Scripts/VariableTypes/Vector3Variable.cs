
using UnityEngine;
using System.Collections;

namespace Scaffold
{
    /// <summary>
    /// Vector3 variable type.
    /// </summary>
    [VariableInfo("Other", "Vector3")]
    [AddComponentMenu("")]
    [System.Serializable]
    public class Vector3Variable : VariableBase<Vector3>
    {
        public override bool IsArithmeticSupported(SetOperator setOperator)
        {
            return true;
        }

        public override void Apply(SetOperator setOperator, Vector3 value)
        {
            Vector3 local = Value;

            switch (setOperator)
            {
            case SetOperator.Negate:
                Value = Value * -1;
                break;
            case SetOperator.Add:
                Value += value;
                break;
            case SetOperator.Subtract:
                Value -= value;
                break;
            case SetOperator.Multiply:
                local.Scale(value);
                Value = local;
                break;
            case SetOperator.Divide:
                local.Scale(new Vector3(1.0f / value.x, 1.0f / value.y, 1.0f / value.z));
                Value = local;
                break;
            default:
                base.Apply(setOperator, value);
                break;
            }
        }
    }

    /// <summary>
    /// Container for a Vector3 variable reference or constant value.
    /// </summary>
    [System.Serializable]
    public struct Vector3Data
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(Vector3Variable))]
        public Vector3Variable vector3Ref;


        [SerializeField]
        public VariableDataSource source;

        [SerializeField]
        public Vector3ValueSO vector3SO;
        [SerializeField]
        public Vector3 vector3Val;

        public Vector3Data(Vector3 v)
        {
            vector3Val = v;
            vector3Ref = null;
            source = VariableDataSource.Unspecified;
            vector3SO = null;
        }

        public static implicit operator Vector3(Vector3Data vector3Data)
        {
            return vector3Data.Value;
        }

        public Vector3 Value
        {
            get { return VariableValueReference.Resolve(vector3Ref, vector3Val, vector3SO, source); }
            set { VariableValueReference.Assign(vector3Ref, ref vector3Val, vector3SO, source, value); }
        }

        public string GetDescription()
        {
            return VariableValueReference.Describe(vector3Ref, vector3Val, vector3SO, source);
        }
    }
}