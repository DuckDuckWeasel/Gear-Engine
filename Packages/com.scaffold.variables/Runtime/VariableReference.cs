using System;
using UnityEngine;

namespace Scaffold.Variables
{
    [Serializable]
    public class VariableReference<T, TVariable> where TVariable : VariableSO<T>
    {
        [SerializeField]
        private bool useConstant = true;
        
        [SerializeField]
        private T constantValue;

        [SerializeField]
        private TVariable variable;

        public VariableReference() { }

        public VariableReference(T value)
        {
            useConstant = true;
            constantValue = value;
        }

        public bool UseConstant => useConstant;
        public TVariable Variable => variable;

        public T Value
        {
            get => useConstant ? constantValue : (variable != null ? variable.Value : default(T));
            set
            {
                if (useConstant)
                {
                    constantValue = value;
                }
                else if (variable != null)
                {
                    variable.Value = value;
                }
            }
        }

        public static implicit operator T(VariableReference<T, TVariable> reference)
        {
            return reference != null ? reference.Value : default(T);
        }
    }
}
